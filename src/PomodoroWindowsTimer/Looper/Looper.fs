namespace PomodoroWindowsTimer.Looper

open System
open System.Runtime.CompilerServices
open System.Threading
open Microsoft.Extensions.Logging

open FSharp.Control

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open LoggingExtensions

type private State = 
    {
        ActiveTimePoint: ActiveTimePoint option
        StartTime: DateTime
        Subscribers: (LooperEvent -> unit) list
        IsStopped: bool
    }
    static member Init(startDateTime) =
        {
            ActiveTimePoint = None
            StartTime = startDateTime
            Subscribers = []
            IsStopped = false
        }

type private Msg =
    | PreloadTimePoint
    | Tick
    | Subscribe of (LooperEvent -> unit)
    | SubscribeMany of (LooperEvent -> unit) list * AsyncReplyChannel<unit>
    | Stop
    | Resume
    | Next
    | Shift of float<sec>
    | ShiftAck of float<sec> * AsyncReplyChannel<unit>
    | GetActiveTimePoint of AsyncReplyChannel<ActiveTimePoint option>


/// 1. Start looper
/// 2. TryReceive time point from queue and store to active time point
[<Sealed>]
type Looper(
    timePointQueue: ITimePointQueue,
    timeProvider: System.TimeProvider,
    tickMilliseconds: int<ms>,
    logger: ILogger<Looper>, 
    ?cancellationToken: System.Threading.CancellationToken
) as this =
    let mutable timer : Timer = Unchecked.defaultof<_>
    let mutable _isDisposed = false

    let started = ref 0

    let now () =
        timeProvider.GetLocalNow().DateTime

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
            state.Subscribers
            |> List.iter (fun f -> f event)
        else
            ()

    
    let tryNextTimePoint (state: State) =
        // in first time next time point is equal to preloaded timepoint
        match timePointQueue.TryGetNext(), state.ActiveTimePoint with
        | Some nextTp, Some actTp ->
            if nextTp.Id = actTp.OriginalId && actTp.RemainingTimeSpan = TimeSpan.Zero then
                timePointQueue.TryGetNext()
            else
                nextTp |> Some
        | nextTp, _ -> nextTp


    let initialize (switchinMode: TimePointSwitchingMode) (state: State) =
        let nextTpOpt = state |> tryNextTimePoint

        let dt = now ()

        let newAtpOpt =
            nextTpOpt
            |> Option.map (fun nextTp ->
                let (newAtp, oldAtp) =
                    state.ActiveTimePoint
                    |> Option.filter (fun atp -> atp.OriginalId = nextTp.Id)
                    |> Option.map (fun atp -> (atp, None))
                    |> Option.defaultValue (nextTp |> TimePoint.toActiveTimePoint, state.ActiveTimePoint)

                tryPostEvent state (
                    LooperEvent.TimePointStarted (
                        TimePointStartedEventArgs.init newAtp oldAtp true switchinMode, timeProvider.GetUtcNow()
                    )
                )

                newAtp
            )

        timer.Change(int tickMilliseconds, 0) |> ignore
        { state with ActiveTimePoint = newAtpOpt; StartTime = dt; IsStopped = false }


    let withActiveTimePointTimeSpan (seconds: float<sec>) (state: State) =
        match state.ActiveTimePoint with
        | Some atp ->
            let atp = { atp with RemainingTimeSpan = TimeSpan.FromSeconds(float seconds) }
            tryPostEvent state (LooperEvent.TimePointTimeReduced (TimePointTimeReducedEventArgs.init atp (not state.IsStopped), timeProvider.GetUtcNow()))
            { state with ActiveTimePoint = atp |> Some }
        | None ->
            logger.LogWarning("It's trying to shift not existing active time point.")
            state

    let agent = new MailboxProcessor<Msg>(
        (fun inbox ->
            let rec loop (state: State) =
                let beginScope (looperMsgName: string) =
                    let scope = logger.BeginMessageScope(looperMsgName)
                    logger.LogStartHandleMessage(looperMsgName)
                    scope

                let endScope (scope: IDisposable | null) =
                    match scope with
                    | NonNull s -> s.Dispose()
                    | _ -> ()

                let tryPostEvent = tryPostEvent state

                async {
                    let! msg = inbox.Receive()
                    logger.LogLooperCurrentState(state)

                    match msg with
                    | PreloadTimePoint when state.IsStopped ->
                        use scope = beginScope (nameof PreloadTimePoint)

                        let atpOpt = timePointQueue.TryPick() |> Option.map TimePoint.toActiveTimePoint
                        atpOpt
                        |> Option.iter (fun atp ->
                            tryPostEvent
                            <| LooperEvent.TimePointStarted (
                                TimePointStartedEventArgs.init atp None false TimePointSwitchingMode.Auto, timeProvider.GetUtcNow())
                        )

                        scope |> endScope
                        return! loop { state with ActiveTimePoint = atpOpt }

                    | Resume when state.IsStopped && state.ActiveTimePoint |> Option.isSome ->
                        let dt = now ()
                        do
                            state.ActiveTimePoint
                            |> Option.iter (fun atp ->
                                tryPostEvent
                                <| LooperEvent.TimePointStarted (
                                    TimePointStartedEventArgs.ofActiveTimePoint atp true TimePointSwitchingMode.Manual, timeProvider.GetUtcNow())
                            )

                        timer.Change(int tickMilliseconds, 0) |> ignore
                        return! loop { state with StartTime = dt; IsStopped = false }

                    | Next ->
                        use scope = beginScope (nameof Next)
                        let newState =  state |> initialize TimePointSwitchingMode.Manual
                        scope |> endScope
                        return! loop newState

                    | Tick when not state.IsStopped && state.ActiveTimePoint |> Option.isNone ->
                        return! loop (state |> initialize TimePointSwitchingMode.Auto)

                    | Tick when not state.IsStopped ->
                        let atp = state.ActiveTimePoint |> Option.get
                        let dt = now ()
                        let atp = { atp with RemainingTimeSpan = atp.RemainingTimeSpan - (dt - state.StartTime) }

                        if MathF.Floor(float32 atp.RemainingTimeSpan.TotalSeconds) <= 0f then
                            let tpOpt = timePointQueue.TryGetNext() |> Option.map TimePoint.toActiveTimePoint
                            match tpOpt with
                            | None ->
                                tryPostEvent (
                                    LooperEvent.TimePointTimeReduced (
                                        TimePointTimeReducedEventArgs.init atp true, timeProvider.GetUtcNow()
                                    )
                                )
                                timer.Change(int tickMilliseconds, 0) |> ignore
                                return! loop { state with StartTime = dt; ActiveTimePoint = None }

                            | Some tp ->
                                let newAtp = { tp with RemainingTimeSpan = tp.RemainingTimeSpan + atp.RemainingTimeSpan }
                                tryPostEvent (
                                    LooperEvent.TimePointStarted (
                                        TimePointStartedEventArgs.init newAtp (atp |> Some) true TimePointSwitchingMode.Auto, timeProvider.GetUtcNow()
                                    )
                                )
                                timer.Change(int tickMilliseconds, 0) |> ignore
                                return! loop { state with ActiveTimePoint = newAtp |> Some; StartTime = dt }
                        else
                            tryPostEvent (
                                LooperEvent.TimePointTimeReduced (
                                    TimePointTimeReducedEventArgs.init atp true, timeProvider.GetUtcNow()
                                )
                            )
                            timer.Change(int tickMilliseconds, 0) |> ignore
                            return! loop { state with ActiveTimePoint = atp |> Some; StartTime = dt }

                    | Subscribe subscriber ->
                        return! loop { state with Subscribers = subscriber :: state.Subscribers }

                    | SubscribeMany (subscribers, reply) ->
                        reply.Reply(())
                        return! loop { state with Subscribers = subscribers @ state.Subscribers }

                    | Stop when not state.IsStopped ->
                        do
                            state.ActiveTimePoint
                            |> Option.iter (fun atp ->
                                tryPostEvent (LooperEvent.TimePointStopped (atp, timeProvider.GetUtcNow()))
                            )
                        return! loop { state with IsStopped = true; StartTime = timeProvider.GetLocalNow().DateTime }

                    | Shift seconds when state.IsStopped ->
                        use scope = beginScope (nameof Next)
                        let newState = state |> withActiveTimePointTimeSpan seconds
                        scope |> endScope
                        return! loop newState

                    | ShiftAck (seconds, reply) when state.IsStopped ->
                        use scope = beginScope (nameof Next)
                        let newState = state |> withActiveTimePointTimeSpan seconds
                        reply.Reply ()
                        scope |> endScope
                        return! loop newState

                    | GetActiveTimePoint reply ->
                        reply.Reply(state.ActiveTimePoint)
                        return! loop state

                    | _ -> return! loop state
                }

            loop (State.Init(timeProvider.GetLocalNow().DateTime))
        )
        , defaultArg cancellationToken CancellationToken.None
    )

    let timePointQueueSubscriber =
        timePointQueue.TimePointsChanged.Subscribe(fun _ -> agent.Post(Msg.PreloadTimePoint))

    member val private Subscribers = [] with get, set

    member val TimePointQueue = timePointQueue with get

    /// Starts Looper in Stop state
    member _.Start() =
        let subscribers = this.Subscribers
        this.Subscribers <- []
        
        this.Start(subscribers)

        let subscribers = this.Subscribers
        if subscribers |> (not << List.isEmpty) then
            this.Subscribers <- []
            subscribers
            |> List.iter this.AddSubscriber


    /// Starts Looper in Stop state
    member _.Start(subscribers: (LooperEvent -> unit) list) =
        if Interlocked.CompareExchange(started, 1 ,0) = 0 then
            if box timer = null then
                timer <-
                    new Timer(fun _ ->
                        agent.Post(Tick)
                    )

            // notifierAgent.Start()
            agent.Start()
            if subscribers |> (not << List.isEmpty) then
                agent.PostAndReply(fun reply -> SubscribeMany (subscribers, reply))
            agent.Post(Stop)
            timePointQueue.Start()

    member _.AddSubscriber(subscriber: (LooperEvent -> unit)) =
        ObjectDisposedException.ThrowIf(_isDisposed, this)
        if started.Value = 0 then
            this.Subscribers <- subscriber :: this.Subscribers
        else
            agent.Post(Subscribe subscriber)

    /// Tries to pick TimePoint from queue, if it present
    /// emits TimePointStarted event and sets ActiveTimePoint.
    member _.PreloadTimePoint() =
        this.ThrowIfNotRunOrDisposed()
        agent.Post(PreloadTimePoint)

    member _.Shift(seconds: float<sec>) =
        if seconds < 0.0<sec> then
            logger.LogWarning("It's trying to shift to negative time")
        else
            this.ThrowIfNotRunOrDisposed()
            agent.Post(Shift seconds)

    member _.ShiftAck(seconds: float<sec>) =
        if seconds < 0.0<sec> then
            logger.LogWarning("It's trying to shift to negative time")
        else
            this.ThrowIfNotRunOrDisposed()
            agent.PostAndReply(fun r -> ShiftAck (seconds, r))

    member _.Resume() =
        this.ThrowIfNotRunOrDisposed()
        agent.Post(Resume)

    member _.Next() =
        this.ThrowIfNotRunOrDisposed()
        agent.Post(Next)

    member _.Stop() =
        this.ThrowIfNotRunOrDisposed()
        agent.Post(Stop)

    member _.GetActiveTimePoint() =
        this.ThrowIfNotRunOrDisposed()
        agent.PostAndReply(Msg.GetActiveTimePoint)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                if box timer <> null then
                    timer.Dispose()

                agent.Post(Stop)
                // notifierAgent.Post(() |> Choice2Of2)
                (agent :> IDisposable).Dispose()
                // (notifierAgent :> IDisposable).Dispose()
                timePointQueueSubscriber.Dispose()
                timePointQueue.Dispose()

            _isDisposed <- true

    member private _.ThrowIfNotRunOrDisposed ([<CallerMemberNameAttribute>] ?memberName: string) =
        ObjectDisposedException.ThrowIf(_isDisposed, this)
        if started.Value = 0 then
            let msg =
                memberName
                |> function
                    | Some n -> $"Failed to execute {n}. Looper agent is not running."
                    | None -> "Looper agent is not running."
            raise (new InvalidOperationException(msg))

    interface ILooper with
        /// Starts Looper in Stop state
        member this.Start() = this.Start()
        member this.Stop() = this.Stop()
        member this.Next() = this.Next()
        member this.Shift(seconds: float<sec>) = this.Shift(seconds)
        member this.ShiftAck(seconds: float<sec>) = this.ShiftAck(seconds)
        member this.Resume() = this.Resume()
        member this.AddSubscriber(subscriber: (LooperEvent -> unit)) = this.AddSubscriber(subscriber)
        /// Tries to pick TimePoint from queue, if it present
        /// emits TimePointStarted event and sets ActiveTimePoint.
        member this.PreloadTimePoint() = this.PreloadTimePoint()
        member this.GetActiveTimePoint() = this.GetActiveTimePoint()

    interface IDisposable with
        member this.Dispose() = this.Dispose(true)


