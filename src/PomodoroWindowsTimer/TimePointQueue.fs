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
    
    let startHandleMessage =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(0b0_0001_0001, "Start Handle TimePointQueue Message"),
            "Start handle {TimePointQueueMsgName}"
        )

    let logStartHandle (scopeName: string) =
        startHandleMessage.Invoke(logger, scopeName, null)

    let messageScope =
        LoggerMessage.DefineScope<string>(
            "Scope of message: {TimePointQueueMsgName}"
        )

    let beginScope (scopeName: string) =
        let scope = messageScope.Invoke(logger, scopeName)
        logStartHandle scopeName
        scope

    let newStateMessage =
        LoggerMessage.Define<string>(
            LogLevel.Trace,
            new EventId(0b0_0001_0010, "New TimePointQueue State"),
            "New State: {NewState}"
        )

    let logNewState (state: State) =
        if logger.IsEnabled(LogLevel.Trace) then
            let stateJson = JsonHelpers.Serialize(state)
            newStateMessage.Invoke(logger, stateJson, null)

    let timePointsMessage =
        LoggerMessage.Define<string>(
            LogLevel.Trace,
            new EventId(0b0_0001_0011, "TimePointQueue TimePoint List"),
            "TimePoint list: {TimePointList}"
        )

    let logTimePoints l =
        if logger.IsEnabled(LogLevel.Trace) then
            let json = JsonHelpers.Serialize(l)
            timePointsMessage.Invoke(logger, json, null)

    let nextTimePointMessage =
        LoggerMessage.Define<string, float32>(
            LogLevel.Trace,
            new EventId(0b0_0001_0100, "Next TimePointQueue TimePoint"),
            "Replying with the next TimePoint: {TimePoint} with priority {Priority}"
        )

    let logNextTimePoint (tp: TimePoint) (priority: float32) =
        if logger.IsEnabled(LogLevel.Trace) then
            let json = JsonHelpers.Serialize(tp)
            nextTimePointMessage.Invoke(logger, json, priority, null)

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
                            let tp = state.Queue.First
                            let priority = state.Queue.GetPriority(tp)
                            let _ = state.Queue.Dequeue()
                            state.Queue.Enqueue(tp, state.MaxPriority)
                            let state = { state with MaxPriority = state.MaxPriority + 1f }
                            logNewState state

                            logNextTimePoint tp priority
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
                            let priority = state.Queue.GetPriority(tp)
                            logNextTimePoint tp priority
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

