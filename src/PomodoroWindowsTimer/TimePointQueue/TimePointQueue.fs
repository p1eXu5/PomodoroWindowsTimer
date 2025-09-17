namespace PomodoroWindowsTimer.TimePointQueue

open System
open System.Threading
open Microsoft.Extensions.Logging

open FSharp.Control
open Priority_Queue

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open LoggingExtensions

type private State =
    {
        Queue : SimplePriorityQueue<TimePoint>
        MaxPriority : float32
    }
    static member Default =
        {
            Queue = SimplePriorityQueue<TimePoint>()
            MaxPriority = 0f
        }

type private Msg =
    | AddMany of TimePoint seq * AsyncReplyChannel<unit>
    | GetAllWithPriority of AsyncReplyChannel<(TimePoint * float32) seq>
    | GetNext of AsyncReplyChannel<TimePoint option>
    | Pick of AsyncReplyChannel<TimePoint option>
    | ScrollTo of TimePointId * AsyncReplyChannel<unit>
    | Reset
    | TryFind of TimePointId * AsyncReplyChannel<TimePoint option>


type TimePointQueue(timePoints: TimePoint seq, logger: ILogger<TimePointQueue>, ?timeout: int<ms>, ?cancellationToken: System.Threading.CancellationToken) =
    let mutable _isDisposed = false
    let isStarted = ref 0
    let replyTimeout = defaultArg timeout 1000<ms>
    
    let _agent = new MailboxProcessor<Msg>(
        (fun inbox ->
            let rec loop state =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Reset ->
                        use _ = logger.BeginHandle(nameof Reset)
                        let newState = State.Default
                        return! loop newState

                    | AddMany (timePoints, reply) ->
                        use _ = logger.BeginHandle(nameof AddMany)

                        let maxPriority =
                            timePoints
                            |> Seq.fold (fun p tp ->
                                state.Queue.Enqueue(tp, p)
                                p + 1f
                            ) state.MaxPriority

                        let state = { state with MaxPriority = maxPriority }
                        // logNewState state

                        reply.Reply()

                        return! loop state


                    | GetAllWithPriority reply ->
                        use _ = logger.BeginHandle(nameof GetAllWithPriority)
                        let xtp =
                            seq {
                                for tp in state.Queue do
                                    yield (tp, state.Queue.GetPriority(tp))
                            }
                            |> Seq.sortBy snd
                        logger.LogTimePoints(xtp)
                        reply.Reply xtp

                        return! loop state

                    | ScrollTo (id, reply) when state.Queue.Count = 0 ->
                        use _ = logger.BeginHandle(nameof ScrollTo, id)
                        logger.LogDebug("Queue is empty")
                        reply.Reply(())
                        return! loop state

                    | ScrollTo (id, reply) ->
                        use _ = logger.BeginHandle(nameof ScrollTo, id)
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
                                priority
                            | tp :: tail when tp.Id = id ->
                                state.Queue.Enqueue(tp, minPriority)
                                tail
                                |> List.fold (fun (p: float32) tp ->
                                    state.Queue.Enqueue(tp, p)
                                    p + 1.0f
                                ) (minPriority + 1f)
                                |> ignore
                                priority

                            | tp :: tail ->
                                state.Queue.Enqueue(tp, priority)
                                move minPriority tail (priority + 1.0f)

                        let maxPriority = move minPriority timePoints 0f

                        let state = { state with MaxPriority = maxPriority }
                        // logNewState state

                        reply.Reply(())

                        return! loop state

                    | GetNext reply when state.Queue.Count = 0 ->
                        use _ = logger.BeginHandle(nameof GetNext)
                        logger.LogDebug("Queue is empty")
                        reply.Reply(None)
                        return! loop state

                    | GetNext reply ->
                        use _ = logger.BeginHandle(nameof GetNext)

                        let tp = state.Queue.First
                        let priority = state.Queue.GetPriority(tp)
                        let _ = state.Queue.Dequeue()
                        state.Queue.Enqueue(tp, state.MaxPriority)
                        let state = { state with MaxPriority = state.MaxPriority + 1f }
                        // logNewState state

                        logger.LogNextTimePoint(tp, priority)
                        reply.Reply(tp |> Some)

                        return! loop state

                    | Pick reply ->
                        use _ = logger.BeginHandle(nameof Pick)

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

    new(logger: ILogger<TimePointQueue>, timeout: int<ms>, cancellationToken: System.Threading.CancellationToken) = new TimePointQueue(Seq.empty, logger, timeout, cancellationToken)
    new(logger: ILogger<TimePointQueue>) = new TimePointQueue(Seq.empty, logger)

    member this.Start() =
        if Interlocked.CompareExchange(isStarted, 1, 0) = 1 then
            ()
        else
            _agent.Start()
            if timePoints |> (not << Seq.isEmpty) then
                this.AddMany(timePoints)

    member _.AddMany(timePoints: TimePoint seq) =
        _agent.PostAndReply(fun r -> AddMany (timePoints, r))

    member this.Reload(timePoints: TimePoint seq) =
        _agent.Post(Reset)
        this.AddMany(timePoints)

    member _.TryPick() =
        _agent.PostAndReply(Pick, int replyTimeout)

    member _.ScrollTo(id: Guid) =
        _agent.PostAndReply(fun reply -> ScrollTo (id, reply))

    member _.TryGetNext() =
        _agent.PostAndReply(GetNext, int replyTimeout)

    member _.TryFind(id: TimePointId) =
        _agent.PostAndReply((fun reply -> TryFind (id, reply)), int replyTimeout)

    member internal _.GetAllWithPriority() =
        _agent.PostAndReply(GetAllWithPriority, int replyTimeout)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                (_agent :> IDisposable).Dispose()

            _isDisposed <- true


    interface ITimePointQueue with
        member this.AddMany(timePointSeq) = this.AddMany(timePointSeq)
        member this.Start() = this.Start()
        member this.TryGetNext() = this.TryGetNext()
        member this.TryPick() = this.TryPick()
        member this.ScrollTo(id: Guid) = this.ScrollTo(id)
        member this.Reload timePoints = this.Reload(timePoints)
        member this.TryFind(id: Guid) = this.TryFind(id) 

    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)

