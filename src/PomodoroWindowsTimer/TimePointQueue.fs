namespace PomodoroWindowsTimer.TimePointQueue

open System
open System.Threading
open Microsoft.Extensions.Logging

open FSharp.Control
open Priority_Queue

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer

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
    | AddMany of TimePoint seq
    | GetTimePointsWithPriority of AsyncReplyChannel<(TimePoint * float32) seq>
    | GetNext of AsyncReplyChannel<TimePoint option>
    | Pick of AsyncReplyChannel<TimePoint option>
    | Scroll of Guid * AsyncReplyChannel<unit>
    | Reset


type TimePointQueue(timePoints: TimePoint seq, logger: ILogger<TimePointQueue>, ?cancellationToken: System.Threading.CancellationToken) =
    let mutable _isDisposed = false

    let logNewState (state: State) =
        if logger.IsEnabled(LogLevel.Trace) then
            let stateJson = JsonHelpers.Serialize(state)
            logger.LogTrace("New State: {NewState}", stateJson)

    let logTimePoints l =
        if logger.IsEnabled(LogLevel.Trace) then
            let json = JsonHelpers.Serialize(l)
            logger.LogTrace("TimePoint list: {TimePointList}", json)

    let logStartHandle (scopeName: string) =
        logger.LogDebug("Start handle {TimePointQueueMsgName}", scopeName)

    let beginScope (scopeName: string) =
        let scope = logger.BeginScope(scopeName)
        logStartHandle scopeName
        scope

    let _agent = new MailboxProcessor<Msg>(
        (fun inbox ->
            let rec loop state =
                let enqueue timePoints priority =
                    timePoints
                    |> Seq.fold (fun p tp ->
                        state.Queue.Enqueue(tp, p)
                        p + 1f
                    ) priority


                async {
                    let! msg = inbox.Receive()
                        
                    match msg with
                    | Reset ->
                        logStartHandle (nameof Reset)
                        return! loop State.Default

                    | AddMany timePoints ->
                        use scope = beginScope (nameof AddMany)

                        let state = { state with MaxPriority = enqueue timePoints (state.MaxPriority) }
                        logNewState state

                        scope.Dispose()
                        return! loop state


                    | GetTimePointsWithPriority reply ->
                        use scope = beginScope (nameof GetTimePointsWithPriority)
                        let xtp =
                            seq {
                                for tp in state.Queue do
                                    yield (tp, state.Queue.GetPriority(tp))
                            }
                        logTimePoints xtp
                        reply.Reply xtp

                        scope.Dispose()
                        return! loop state

                    | Scroll (id, reply) ->
                        use scope = beginScope (nameof Scroll)

                        if state.Queue.Count = 0 then
                            logger.LogDebug("Queue is empty")
                            reply.Reply(())
                        else
                            while state.Queue.First.Id <> id do
                                let priority = state.Queue.GetPriority(state.Queue.First)
                                let tp = state.Queue.Dequeue()
                                state.Queue.Enqueue(tp, priority + state.MaxPriority)

                            let state = { state with MaxPriority = state.MaxPriority + (float32 state.Queue.Count) }
                            logNewState state

                            reply.Reply(())

                        scope.Dispose()
                        return! loop state


                    | GetNext reply ->
                        use scope = beginScope (nameof GetNext)

                        if state.Queue.Count = 0 then
                            logger.LogDebug("Queue is empty")
                            reply.Reply(None)
                        else
                            let tp = state.Queue.Dequeue()
                            state.Queue.Enqueue(tp, state.MaxPriority)
                            let state = { state with MaxPriority = state.MaxPriority + 1f }
                            logNewState state

                            logger.LogDebug("Replying with the next TimePoint: {TimePoint}", tp)
                            reply.Reply(tp |> Some)

                        scope.Dispose()
                        return! loop state

                    | Pick reply ->
                        use scope = beginScope (nameof Pick)

                        if state.Queue.Count = 0 then
                            logger.LogDebug("Queue is empty")
                            reply.Reply(None)
                        else
                            let tp = state.Queue.First
                            logger.LogDebug("Replying with the TimePoint: {TimePoint}", tp)
                            reply.Reply(tp |> Some)

                        scope.Dispose()
                        return! loop state
                }

            loop State.Default
        )
        , defaultArg cancellationToken CancellationToken.None
    )

    let isStarted = ref 0

    new(logger: ILogger<TimePointQueue>, cancellationToken: System.Threading.CancellationToken) = new TimePointQueue(Seq.empty, logger, cancellationToken)
    new(logger: ILogger<TimePointQueue>) = new TimePointQueue(Seq.empty, logger)

    member this.Start() =
        if Interlocked.CompareExchange(isStarted, 1, 0) = 1 then
            ()
        else
            _agent.Start()
            if timePoints |> (not << Seq.isEmpty) then
                this.AddMany(timePoints)

    member _.AddMany(timePoints: TimePoint seq) =
        _agent.Post(AddMany (timePoints |> Seq.readonly))

    member this.Reload(timePoints: TimePoint seq) =
        _agent.Post(Reset)
        this.AddMany(timePoints)

    member _.TryPick =
        _agent.PostAndReply(Pick, 1000)

    member _.Scroll(id: Guid) =
        let _ = _agent.PostAndReply(fun reply -> Scroll (id, reply))
        ()

    member _.TryEnqueue =
        _agent.PostAndReply(GetNext, 1000)

    member _.GetTimePointsWithPriority() =
        _agent.PostAndReply(GetTimePointsWithPriority)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                (_agent :> IDisposable).Dispose()

            _isDisposed <- true
                

    interface ITimePointQueue with
        member this.Start() = this.Start()
        member this.TryEnqueue = this.TryEnqueue
        member this.TryPick = this.TryPick
        member this.Scroll(id: Guid) = this.Scroll(id)
        member this.Reload timePoints = this.Reload(timePoints)

    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)

