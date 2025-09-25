namespace PomodoroWindowsTimer.TimePointQueue

open System
open System.Threading
open Microsoft.Extensions.Logging

open FSharp.Control
open Priority_Queue

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open LoggingExtensions

type private State =
    {
        FirstTimePointId: TimePointId voption
        Queue : SimplePriorityQueue<TimePoint>
        MaxPriority : float32
    }
    static member Default =
        {
            FirstTimePointId = ValueNone
            Queue = SimplePriorityQueue<TimePoint>()
            MaxPriority = 0f
        }

type private Msg =
    | AddMany of TimePoint seq * AsyncReplyChannel<unit>
    | GetAllWithPriority of AsyncReplyChannel<(TimePoint * float32) seq>
    /// Returns time points sorted by TimePoint.Num and first queued time point identificator.
    | GetAll of AsyncReplyChannel<(TimePoint list * TimePointId option)>
    | GetNext of AsyncReplyChannel<TimePoint option>
    | Pick of AsyncReplyChannel<TimePoint option>
    | ScrollTo of TimePointId * AsyncReplyChannel<bool>
    | Reset
    | TryFind of TimePointId * AsyncReplyChannel<TimePoint option>


type TimePointQueue(
    timePointStore: TimePointStore,
    logger: ILogger<TimePointQueue>,
    ?timeout: int<ms>,
    ?cancellationToken: System.Threading.CancellationToken
) =
    let mutable _isDisposed = false
    let isStarted = ref 0
    let replyTimeout = defaultArg timeout 1000<ms>

    let timePointsChangedEvent = new Event<TimePoint list * TimePointId option>()
    let timePointsLoopComlettedEvent = new Event<unit>()

    let _agent = new MailboxProcessor<Msg>(
        (fun inbox ->
            let rec loop state =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Reset ->
                        use _ = logger.BeginHandleScope(nameof Reset)
                        let newState = State.Default
                        timePointsChangedEvent.Trigger([], None)
                        return! loop newState

                    | AddMany (timePoints, reply) ->
                        use _ = logger.BeginHandleScope(nameof AddMany)

                        if timePoints |> Seq.isEmpty then
                            return! loop state
                        else
                            let maxPriority =
                                timePoints
                                |> Seq.fold (fun p tp ->
                                    state.Queue.Enqueue(tp, p)
                                    p + 1f
                                ) state.MaxPriority

                            let state =
                                match state.FirstTimePointId with
                                | ValueNone ->
                                    { state with
                                        MaxPriority = maxPriority;
                                        FirstTimePointId = timePoints |> Seq.tryHead |> function Some tp -> tp.Id |> ValueSome | _ -> ValueNone
                                    }
                                | _ -> { state with MaxPriority = maxPriority }

                            reply.Reply()

                            timePointsChangedEvent.Trigger(
                                state.Queue
                                |> Seq.sortBy _.Num
                                |> Seq.toList
                                , (
                                    if state.Queue.Count > 0 then
                                        state.Queue.First |> _.Id |> Some
                                    else
                                        None
                                )
                            )

                            return! loop state

                    | GetAllWithPriority reply ->
                        use _ = logger.BeginHandleScope(nameof GetAllWithPriority)
                        let xtp =
                            seq {
                                for tp in state.Queue do
                                    yield (tp, state.Queue.GetPriority(tp))
                            }
                            |> Seq.sortBy snd
                        logger.LogTimePointPriorities(xtp)
                        reply.Reply xtp

                        return! loop state

                    | GetAll reply ->
                        use _ = logger.BeginHandleScope(nameof GetAll)
                        if state.Queue.Count = 0 then
                            logger.LogDebug("Queue is empty")
                            reply.Reply([], None)
                        let xtp =
                            seq {
                                for tp in state.Queue do
                                    yield (tp)
                            }
                            |> Seq.sortBy _.Num
                            |> Seq.toList
                        logger.LogTimePoints(xtp)
                        reply.Reply (xtp, state.Queue.First |> _.Id |> Some)

                        return! loop state

                    | ScrollTo (id, reply) when state.Queue.Count = 0 ->
                        use _ = logger.BeginHandleScope(nameof ScrollTo, id)
                        logger.LogDebug("Queue is empty")
                        reply.Reply(false)
                        return! loop state

                    // Reorder state.Queue that interested time point priority becomes lower. Returns True if time point exists.
                    | ScrollTo (id, reply) ->
                        use _ = logger.BeginHandleScope(nameof ScrollTo, id)

                        if state.Queue |> (not << Seq.exists (fun tp -> tp.Id = id)) then
                            reply.Reply(false)
                            return! loop state
                        else
                            let minPriority = -(float32 state.Queue.Count)

                            let timePoints =
                                seq {
                                    for _ in 1 .. state.Queue.Count do
                                        yield state.Queue.Dequeue()
                                }
                                |> Seq.toList

                            state.Queue.Clear()

                            let rec move minPriority (queue: TimePoint list) priority =
                                match queue with
                                | [] ->
                                    logger.LogWarning("Scroll is failed, time point with target id {TimePointId} has not been found in queue", id)
                                    (priority, false)
                                | tp :: tail when tp.Id = id ->
                                    state.Queue.Enqueue(tp, minPriority)
                                    tail
                                    |> List.fold (fun (p: float32) tp ->
                                        state.Queue.Enqueue(tp, p)
                                        p + 1.0f
                                    ) (minPriority + 1f)
                                    |> ignore
                                    (priority, true)

                                | tp :: tail ->
                                    state.Queue.Enqueue(tp, priority)
                                    move minPriority tail (priority + 1.0f)

                            let (maxPriority, tpExists) = move minPriority timePoints 0f

                            let state = { state with MaxPriority = maxPriority }
                            // logNewState state

                            reply.Reply(tpExists)

                            return! loop state

                    | GetNext reply when state.Queue.Count = 0 ->
                        use _ = logger.BeginHandleScope(nameof GetNext)
                        logger.LogDebug("Queue is empty")
                        reply.Reply(None)
                        return! loop state

                    | GetNext reply ->
                        use _ = logger.BeginHandleScope(nameof GetNext)

                        let tp = state.Queue.First
                        let priority = state.Queue.GetPriority(tp)
                        let _ = state.Queue.Dequeue()
                        state.Queue.Enqueue(tp, state.MaxPriority)

                        let state = { state with MaxPriority = state.MaxPriority + 1f }
                        
                        // logNewState state

                        logger.LogNextTimePoint(tp, priority)
                        reply.Reply(tp |> Some)

                        if state.FirstTimePointId |> ValueOption.map (fun ftpId -> ftpId = tp.Id) |> ValueOption.defaultValue false then
                            timePointsLoopComlettedEvent.Trigger ()

                        return! loop state

                    | Pick reply ->
                        use _ = logger.BeginHandleScope(nameof Pick)

                        if state.Queue.Count = 0 then
                            logger.LogDebug("Queue is empty")
                            reply.Reply(None)
                        else
                            let tp = state.Queue.First
                            let priority = state.Queue.GetPriority(tp)
                            logger.LogNextTimePoint(tp, priority)
                            reply.Reply(tp |> Some)

                        return! loop state

                    | TryFind (id, reply) ->
                        reply.Reply(state.Queue |> Seq.tryFind (fun tp -> tp.Id = id))
                        return! loop state
                }

            loop State.Default
        )
        , true
        , defaultArg cancellationToken CancellationToken.None
    )

    [<CLIEvent>]
    member _.TimePointsChanged = timePointsChangedEvent.Publish

    [<CLIEvent>]
    member _.TimePointsLoopCompletted = timePointsLoopComlettedEvent.Publish

    member this.Start() =
        if Interlocked.CompareExchange(isStarted, 1, 0) = 1 then
            ()
        else
            _agent.Start()
            let timePoints = timePointStore.Read()
            if timePoints |> (not << List.isEmpty) then
                this.AddMany(timePoints)

    member _.AddMany(timePoints: TimePoint seq) =
        _agent.PostAndReply(fun r -> AddMany (timePoints, r))

    member this.Reload(timePoints: TimePoint list) =
        let (tpList, _) = _agent.PostAndReply(GetAll, int replyTimeout)
        if tpList <> timePoints then
            _agent.Post(Reset)
            this.AddMany(timePoints)
            timePointStore.Write(timePoints)

    member _.TryPick() =
        _agent.PostAndReply(Pick, int replyTimeout)

    member _.ScrollTo(id: TimePointId) =
        _agent.PostAndReply(fun reply -> ScrollTo (id, reply))

    member _.TryGetNext() =
        _agent.PostAndReply(GetNext, int replyTimeout)

    member _.TryFind(id: TimePointId) =
        _agent.PostAndReply((fun reply -> TryFind (id, reply)), int replyTimeout)

    member internal _.GetAllWithPriority() =
        _agent.PostAndReply(GetAllWithPriority, int replyTimeout)

    member internal _.GetAll() =
        _agent.PostAndReply(GetAll, int replyTimeout)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                (_agent :> IDisposable).Dispose()

            _isDisposed <- true


    interface ITimePointQueue with
        [<CLIEvent>]
        member this.TimePointsChanged = this.TimePointsChanged

        [<CLIEvent>]
        member this.TimePointsLoopCompletted = this.TimePointsLoopCompletted

        member this.AddMany(timePointSeq) = this.AddMany(timePointSeq)
        member this.Start() = this.Start()
        member this.TryGetNext() = this.TryGetNext()
        member this.TryPick() = this.TryPick()
        member this.ScrollTo(id: TimePointId) = this.ScrollTo(id)
        member this.Reload timePoints = this.Reload(timePoints)
        member this.TryFind(id: TimePointId) = this.TryFind(id)
        member this.GetTimePoints (): (TimePoint list * TimePointId option) = 
            this.GetAll()

    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)

