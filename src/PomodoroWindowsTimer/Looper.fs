module PomodoroWindowsTimer.Looper

open FSharp.Control
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open System
open System.Threading
open Microsoft.Extensions.Logging
open System.Runtime.CompilerServices


type private State = 
    {
        ActiveTimePoint: ActiveTimePoint option
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
    | GetActiveTimePoint of AsyncReplyChannel<ActiveTimePoint option>


module private State =
    let init (timeProvider: System.TimeProvider) =
        {
            ActiveTimePoint = None
            StartTime = timeProvider.GetLocalNow().DateTime
            Subscribers = []
            IsStopped = false
        }

[<AutoOpen>]
module private BeginMessageScope =
    let private messageScope =
        LoggerMessage.DefineScope<string>(
            "Scope of Looper message: {LooperMsgName}"
        )

    type LoggerExtensions () =
        [<Extension>]
        static member BeginMessageScope(logger: ILogger, looperMsgName: string) =
            messageScope.Invoke(logger, looperMsgName)

[<AutoOpen>]
module private StartHandleMessage =
    let private loggerMessage = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(0b0_0010_0001, "Start Handle Looper Message"),
        "Start handle Looper message: {LooperMsgName}"
    )

    type LoggerExtensions () =
        [<Extension>]
        static member LogStartHandleMessage(logger: ILogger, looperMsgName: string) =
            loggerMessage.Invoke(logger, looperMsgName, null)

[<AutoOpen>]
module private LooperStateUpdated =
    let private loggerMessageTrace = LoggerMessage.Define<string>(
        LogLevel.Trace,
        new EventId(0b0_0001_0010, "Looper State Updated"),
        "Looper state updated: {NewLooperState}"
    )

    let private loggerMessage = LoggerMessage.Define(
        LogLevel.Trace,
        new EventId(0b0_0001_0010, "Looper State Updated"),
        "Looper state updated"
    )

    type LoggerExtensions () =
        [<Extension>]
        static member LogLooperStateUpdated(logger: ILogger, state: State) =
            if logger.IsEnabled(LogLevel.Trace) then
                loggerMessageTrace.Invoke(
                    logger,
                    (JsonHelpers.Serialize state),
                    null
                )
            else
                loggerMessage.Invoke(logger, null)


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

    let beginScope (looperMsgName: string) =
        let scope = logger.BeginMessageScope(looperMsgName)
        logger.LogStartHandleMessage(looperMsgName)
        scope

    let notifierAgent = new MailboxProcessor<Choice<Async<unit> list, unit>>(
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
            notifierAgent.Post(Choice1Of2 (state.Subscribers |> List.map (fun f -> f event)))

    let initialize (state: State) =
        let nextTpOpt =
            // in first time next time point is equal to preloaded timepoint
            match timePointQueue.TryGetNext(), state.ActiveTimePoint with
            | Some nextTp, Some actTp ->
                if nextTp.Id = actTp.OriginId && actTp.RemainingTimeSpan = TimeSpan.Zero then
                    timePointQueue.TryGetNext()
                else
                    nextTp |> Some
            | nextTp, _ -> nextTp

        let dt = now ()

        match nextTpOpt with
        | Some nextTp ->
            let (newAtp, oldAtp) =
                state.ActiveTimePoint
                |> Option.filter (fun t -> t.OriginId = nextTp.Id)
                |> Option.map (fun t -> (t, None))
                |> Option.defaultValue (nextTp |> TimePoint.toActiveTimePoint, state.ActiveTimePoint)

            tryPostEvent state (LooperEvent.TimePointStarted (TimePointStartedEventArgs.init newAtp oldAtp))
            timer.Change(int tickMilliseconds, 0) |> ignore
            { state with ActiveTimePoint = newAtp |> Some; StartTime = dt }

        | _ ->
            timer.Change(int tickMilliseconds, 0) |> ignore
            state

    let withActiveTimePointTimeSpan (seconds: float<sec>) (state: State) =
        match state.ActiveTimePoint with
        | Some atp ->
            let atp = { atp with RemainingTimeSpan = TimeSpan.FromSeconds(float seconds) }
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

                        let atpOpt = timePointQueue.TryPick() |> Option.map TimePoint.toActiveTimePoint
                        atpOpt
                        |> Option.iter (fun atp -> LooperEvent.TimePointStarted (TimePointStartedEventArgs.init atp None) |> tryPostEvent)

                        scope.Dispose()
                        return! loop { state with ActiveTimePoint = atpOpt }

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
                        let atp = { atp with RemainingTimeSpan = atp.RemainingTimeSpan - (dt - state.StartTime) }

                        if MathF.Floor(float32 atp.RemainingTimeSpan.TotalSeconds) <= 0f then
                            let tpOpt = timePointQueue.TryGetNext() |> Option.map TimePoint.toActiveTimePoint
                            match tpOpt with
                            | None ->
                                tryPostEvent (LooperEvent.TimePointTimeReduced atp)
                                timer.Change(int tickMilliseconds, 0) |> ignore
                                return! loop { state with StartTime = dt; ActiveTimePoint = None }

                            | Some tp ->
                                let newAtp = { tp with RemainingTimeSpan = tp.RemainingTimeSpan + atp.RemainingTimeSpan }
                                tryPostEvent (LooperEvent.TimePointStarted (TimePointStartedEventArgs.init newAtp (atp |> Some)))
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

                    | GetActiveTimePoint reply ->
                        reply.Reply(state.ActiveTimePoint)
                        return! loop state

                    | _ -> return! loop state
                }

            loop (State.init timeProvider)
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
            notifierAgent.Start()
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

    member _.GetActiveTimePoint() =
        agent.PostAndReply(Msg.GetActiveTimePoint)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                timer.Dispose()
                agent.Post(Stop)
                notifierAgent.Post(() |> Choice2Of2)
                (agent :> IDisposable).Dispose()
                (notifierAgent :> IDisposable).Dispose()
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
        /// Tryes to pick TimePoint from queue, if it present
        /// emits TimePointStarted event and sets ActiveTimePoint.
        member this.PreloadTimePoint() = this.PreloadTimePoint()
        member this.GetActiveTimePoint() = this.GetActiveTimePoint()

    interface IDisposable with
        member this.Dispose() = this.Dispose(true)
        




