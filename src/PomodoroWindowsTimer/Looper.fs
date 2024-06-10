module PomodoroWindowsTimer.Looper

open FSharp.Control
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open System
open System.Threading
open Microsoft.Extensions.Logging


type private State = 
    {
        ActiveTimePoint: TimePoint option
        StartTime: DateTime
        Subscribers: (LooperEvent -> Async<unit>) list
        IsStopped: bool
    }

type private Msg =
    | PreloadTimePoint
    | Tick
    | Subscribe of (LooperEvent -> Async<unit>)
    | SubscribeMany of (LooperEvent -> Async<unit>) list * AsyncReplyChannel<unit>
    | Stop
    | Resume
    | Next
    | Shift of float<sec>
    | ShiftAck of float<sec> * AsyncReplyChannel<unit>


/// 1. Start looper
/// 2. TryReceive time point from queue and store to active time point
type Looper(
    timePointQueue: ITimePointQueue,
    timeProvider: System.TimeProvider,
    tickMilliseconds: int<ms>,
    logger: ILogger<Looper>, 
    ?cancellationToken: System.Threading.CancellationToken
) =
    let mutable timer : Timer = Unchecked.defaultof<_>
    let mutable _isDisposed = false

    let started = ref 0

    let now () =
        timeProvider.GetLocalNow().DateTime

    let defaultState () =
        {
            ActiveTimePoint = None
            StartTime = now ()
            Subscribers = []
            IsStopped = false
        }

    let startHandleMessage =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(0b0_0010_0001, "Start Handle Looper Message"),
            "Start handle Looper message: {LooperMsgName}"
        )

    let logStartHandle (scopeName: string) =
        startHandleMessage.Invoke(logger, scopeName, null)

    let messageScope =
        LoggerMessage.DefineScope<string>(
            "Scope of Looper message: {LooperMsgName}"
        )

    let beginScope (scopeName: string) =
        let scope = messageScope.Invoke(logger, scopeName)
        logStartHandle scopeName
        scope

    let newStateMessage =
        LoggerMessage.Define<string>(
            LogLevel.Trace,
            new EventId(0b0_0001_0010, "New Looper State"),
            "New Looper State: {NewLooperState}"
        )

    let logNewState (state: State) =
        if logger.IsEnabled(LogLevel.Trace) then
            let stateJson = JsonHelpers.Serialize(state)
            newStateMessage.Invoke(logger, stateJson, null)

    let invoker = new MailboxProcessor<Choice<Async<unit> list, unit>>(
        (fun inbox ->
            let rec loop () =
                async {
                    let! msg = inbox.Receive()
                    match msg with
                    | Choice1Of2 subscribers ->
                        do!
                            subscribers
                            |> Async.Parallel
                            |> Async.Ignore

                        return! loop ()
                    | _ ->
                        return! loop ()
                }

            loop ()
        )
        , defaultArg cancellationToken CancellationToken.None
    )

    let tryPostEvent (state: State) event =
        if state.Subscribers |> (not << List.isEmpty) then
            invoker.Post(Choice1Of2 (state.Subscribers |> List.map (fun f -> f event)))

    let initialize (state: State) =
        let nextTpOpt =
            // in first time next time point is equal to preloaded timepoint
            match timePointQueue.TryGetNext(), state.ActiveTimePoint with
            | Some nextTp, Some actTp ->
                if actTp.Id = nextTp.Id && actTp.TimeSpan = TimeSpan.Zero then
                    timePointQueue.TryGetNext()
                else
                    nextTp |> Some
            | Some nextTp, None ->
                nextTp |> Some
            | None, _ -> None

        let dt = now ()

        match nextTpOpt with
        | Some nextTp ->
            let (newAtp, oldAtp) =
                state.ActiveTimePoint
                |> Option.filter (fun t -> t.Id = nextTp.Id)
                |> Option.map (fun t -> (t, None))
                |> Option.defaultValue (nextTp, state.ActiveTimePoint)

            tryPostEvent state (LooperEvent.TimePointStarted (newAtp, oldAtp))
            timer.Change(int tickMilliseconds, 0) |> ignore
            { state with ActiveTimePoint = newAtp |> Some; StartTime = dt }

        | _ ->
            timer.Change(int tickMilliseconds, 0) |> ignore
            state

    let withActiveTimePointTimeSpan (seconds: float<sec>) (state: State) =
        match state.ActiveTimePoint with
        | Some atp ->
            let atp = { atp with TimeSpan = TimeSpan.FromSeconds(float seconds) }
            tryPostEvent state (LooperEvent.TimePointTimeReduced atp)
            { state with ActiveTimePoint = atp |> Some }
        | None ->
            logger.LogWarning("It's trying to shift not existing active time point.")
            state

    let agent = new MailboxProcessor<Msg>(
        (fun inbox ->
            let rec loop (state: State) =
                let tryPostEvent = tryPostEvent state

                async {
                    let! msg = inbox.Receive()
                    // printfn "%A: %A\n" msg state

                    match msg with
                    | PreloadTimePoint when state.ActiveTimePoint |> Option.isNone ->
                        use scope = beginScope (nameof PreloadTimePoint)

                        let tpOpt = timePointQueue.TryPick()
                        tpOpt
                        |> Option.iter (fun atp -> LooperEvent.TimePointStarted (atp, None) |> tryPostEvent)

                        scope.Dispose()
                        return! loop { state with ActiveTimePoint = tpOpt }

                    | Resume when state.ActiveTimePoint |> Option.isSome && state.IsStopped ->
                        let dt = now ()
                        timer.Change(int tickMilliseconds, 0) |> ignore
                        return! loop { state with StartTime = dt; IsStopped = false }

                    | Next ->
                        use scope = beginScope (nameof Next)
                        let newState = initialize { state with IsStopped = false; StartTime = now () }
                        scope.Dispose()
                        return! loop newState

                    | Tick when state.ActiveTimePoint |> Option.isNone && not (state.IsStopped) ->
                        return! loop (initialize state)

                    | Tick when not (state.IsStopped) ->
                        let atp = state.ActiveTimePoint |> Option.get
                        let dt = now ()
                        let atp = { atp with TimeSpan = atp.TimeSpan - (dt - state.StartTime) }

                        if MathF.Floor(float32 atp.TimeSpan.TotalSeconds) <= 0f then
                            let tpOpt = timePointQueue.TryGetNext()
                            match tpOpt with
                            | None ->
                                tryPostEvent (LooperEvent.TimePointTimeReduced atp)
                                timer.Change(int tickMilliseconds, 0) |> ignore
                                return! loop { state with StartTime = dt; ActiveTimePoint = None }

                            | Some tp ->
                                let newAtp = { tp with TimeSpan = tp.TimeSpan + atp.TimeSpan }
                                tryPostEvent (LooperEvent.TimePointStarted (newAtp, atp |> Some))
                                timer.Change(int tickMilliseconds, 0) |> ignore
                                return! loop { state with ActiveTimePoint = newAtp |> Some; StartTime = dt }
                        else
                            tryPostEvent (LooperEvent.TimePointTimeReduced atp)
                            timer.Change(int tickMilliseconds, 0) |> ignore
                            return! loop { state with ActiveTimePoint = atp |> Some; StartTime = dt }

                    | Subscribe subscriber ->
                        return! loop { state with Subscribers = subscriber :: state.Subscribers }

                    | SubscribeMany (subscribers, reply) ->
                        reply.Reply(())
                        return! loop { state with Subscribers = subscribers @ state.Subscribers }

                    | Stop when not state.IsStopped ->
                        return! loop { state with IsStopped = true; StartTime = timeProvider.GetLocalNow().DateTime }

                    | Shift seconds when state.IsStopped ->
                        use scope = beginScope (nameof Next)
                        let newState = state |> withActiveTimePointTimeSpan seconds
                        scope.Dispose()
                        return! loop newState

                    | ShiftAck (seconds, reply) when state.IsStopped ->
                        use scope = beginScope (nameof Next)
                        let newState = state |> withActiveTimePointTimeSpan seconds
                        reply.Reply ()
                        scope.Dispose()
                        return! loop newState

                    | _ -> return! loop state
                }

            loop (defaultState ())
        )
        , defaultArg cancellationToken CancellationToken.None
    )

    member val Subscribers = [] with get, set

    member val TimePointQueue = timePointQueue with get

    /// Starts Looper in Stop state
    member this.Start() =
        this.Start(this.Subscribers)

    /// Starts Looper in Stop state
    member _.Start(subscribers: (LooperEvent -> Async<unit>) list) =
        if Interlocked.CompareExchange(started, 1 ,0) = 0 then
            if timer = null then
                timer <-
                    new Timer(fun _ ->
                        agent.Post(Tick)
                    )

            timePointQueue.Start()
            invoker.Start()
            agent.Start()
            do agent.PostAndReply(fun reply -> SubscribeMany (subscribers, reply))
            agent.Post(Stop)

    member _.AddSubscriber(subscriber: (LooperEvent -> Async<unit>)) =
        agent.Post(Subscribe subscriber)

    /// Tryes to pick TimePoint from queue, if it present
    /// emits TimePointStarted event and sets ActiveTimePoint.
    member _.PreloadTimePoint() =
        agent.Post(PreloadTimePoint)

    member _.Shift(seconds: float<sec>) =
        if seconds < 0.0<sec> then
            logger.LogWarning("It's trying to shift to negative time")
        else
            agent.Post(Shift seconds)

    member _.ShiftAck(seconds: float<sec>) =
        if seconds < 0.0<sec> then
            logger.LogWarning("It's trying to shift to negative time")
        else
            agent.PostAndReply(fun r -> ShiftAck (seconds, r))

    member _.Resume() =
        agent.Post(Resume)

    member _.Next() =
        agent.Post(Next)

    member _.Stop() =
        agent.Post(Stop)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                timer.Dispose()
                agent.Post(Stop)
                invoker.Post(() |> Choice2Of2)
                (agent :> IDisposable).Dispose()
                (invoker :> IDisposable).Dispose()
                timePointQueue.Dispose()

            _isDisposed <- true

    interface ILooper with
        /// Starts Looper in Stop state
        member this.Start() = this.Start()
        member this.Stop() = this.Stop()
        member this.Next() = this.Next()
        member this.Shift(seconds: float<sec>) = this.Shift(seconds)
        member this.ShiftAck(seconds: float<sec>) = this.ShiftAck(seconds)
        member this.Resume() = this.Resume()
        member this.AddSubscriber(subscriber: (LooperEvent -> Async<unit>)) = this.AddSubscriber(subscriber)
        member this.PreloadTimePoint() = this.PreloadTimePoint()

    interface IDisposable with
        member this.Dispose() = this.Dispose(true)
        




