﻿module PomodoroWindowsTimer.Looper

open FSharp.Control
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstrractions
open System
open System.Threading

type LooperEvent =
    | TimePointStarted of ``new``: TimePoint * old: TimePoint option 
    | TimePointTimeReduced of TimePoint

type private State = 
    {
        ActiveTimePoint: TimePoint option
        StartTime: DateTime
        Subscribers: (LooperEvent -> Async<unit>) list
        IsStopped: bool
    }
    with
        static member Default() =
            {
                ActiveTimePoint = None
                StartTime = DateTime.Now
                Subscribers = []
                IsStopped = false
            }


type private Msg =
    | PreloadTimePoint
    | Tick
    | Subscribe of (LooperEvent -> Async<unit>)
    | SubscribeMany of (LooperEvent -> Async<unit>) list * AsyncReplyChannel<unit>
    | Stop
    | Resume
    | Next
    | Shift of float

/// 1. Start looper
/// 2. TryReceive time point from queue and store to active time point
type Looper(timePointQueue: ITimePointQueue, tickMilliseconds: int, ?cancellationToken: System.Threading.CancellationToken) =
    let mutable timer : Timer = Unchecked.defaultof<_>
    let mutable _isDisposed = false

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

    let agent = new MailboxProcessor<Msg>(
        (fun inbox ->
            let rec loop state =
                let isStopMsgQueued =
                    async {
                        let! stopMsgOpt = inbox.TryScan(fun m ->
                            match m with
                            | Stop ->
                                async {
                                    return ()
                                }
                                |> Some
                            | _ -> None
                        )

                        return stopMsgOpt |> Option.isSome
                    }

                let tryPostEvent event =
                    if state.Subscribers |> (not << List.isEmpty) then
                        invoker.Post(Choice1Of2 (state.Subscribers |> List.map (fun f -> f event)))

                async {
                    let! msg = inbox.Receive()
                    // printfn "%A: %A\n" msg state

                    let initialize state =
                        async {
                            let! tpOpt = timePointQueue.Enqueue

                            let dt = DateTime.Now

                            match tpOpt with
                            | Some tp ->
                                let (newAtp, oldAtp) =
                                    state.ActiveTimePoint
                                    |> Option.filter (fun t -> t.Id = tp.Id)
                                    |> Option.map (fun t -> (t, None))
                                    |> Option.defaultValue (tp, state.ActiveTimePoint)

                                tryPostEvent (LooperEvent.TimePointStarted (newAtp, oldAtp))
                                timer.Change(tickMilliseconds, 0) |> ignore
                                return! loop { state with ActiveTimePoint = newAtp |> Some; StartTime = dt }

                            | _ ->
                                timer.Change(tickMilliseconds, 0) |> ignore
                                return! loop state
                        }

                    match msg with
                    | PreloadTimePoint when state.ActiveTimePoint |> Option.isNone ->
                        let tpOpt = timePointQueue.Pick
                        tpOpt
                        |> Option.iter (fun atp -> LooperEvent.TimePointStarted (atp, None) |> tryPostEvent)
                        return! loop { state with ActiveTimePoint = tpOpt }

                    | Resume when state.ActiveTimePoint |> Option.isSome && state.IsStopped ->
                        let dt = DateTime.Now
                        timer.Change(tickMilliseconds, 0) |> ignore
                        return! loop { state with StartTime = dt; IsStopped = false }

                    | Next ->
                        return! initialize { state with IsStopped = false; StartTime = DateTime.Now }

                    | Tick when state.ActiveTimePoint |> Option.isNone && not (state.IsStopped) ->
                        return! initialize state

                    | Tick when not (state.IsStopped) ->
                        let atp = state.ActiveTimePoint |> Option.get
                        let dt = DateTime.Now
                        let atp = { atp with TimeSpan = atp.TimeSpan - (dt - state.StartTime) }

                        if MathF.Floor(float32 atp.TimeSpan.TotalSeconds) = 0f then
                            let! tpOpt = timePointQueue.Enqueue
                            match tpOpt with
                            | None ->
                                tryPostEvent (LooperEvent.TimePointTimeReduced atp)
                                timer.Change(tickMilliseconds, 0) |> ignore
                                return! loop { state with StartTime = dt; ActiveTimePoint = None }

                            | Some tp ->
                                let newAtp = { tp with TimeSpan = tp.TimeSpan + atp.TimeSpan }
                                tryPostEvent (LooperEvent.TimePointStarted (newAtp, atp |> Some))
                                timer.Change(tickMilliseconds, 0) |> ignore
                                return! loop { state with ActiveTimePoint = newAtp |> Some; StartTime = dt }
                        else
                            tryPostEvent (LooperEvent.TimePointTimeReduced atp)
                            timer.Change(tickMilliseconds, 0) |> ignore
                            return! loop { state with ActiveTimePoint = atp |> Some; StartTime = dt }

                    | Subscribe subscriber ->
                        return! loop { state with Subscribers = subscriber :: state.Subscribers }

                    | SubscribeMany (subscribers, reply) ->
                        reply.Reply(())
                        return! loop { state with Subscribers = subscribers @ state.Subscribers }

                    | Stop when not state.IsStopped ->
                        return! loop { state with IsStopped = true; StartTime = DateTime.Now }

                    | Shift v when state.ActiveTimePoint |> Option.isSome && state.IsStopped ->
                        let atp = state.ActiveTimePoint |> Option.map (fun atp -> { atp with TimeSpan = TimeSpan.FromSeconds(v) }) |> Option.get
                        tryPostEvent (LooperEvent.TimePointTimeReduced atp)
                        return! loop { state with ActiveTimePoint = atp |> Some }

                    | _ -> return! loop state
                }

            loop (State.Default())
        )
        , defaultArg cancellationToken CancellationToken.None
    )

    member val Subscribers = [] with get, set

    member val TimePointQueue = timePointQueue with get

    member this.Start() =
        this.Start(this.Subscribers)


    member _.Stop() =
        agent.Post(Stop)

    member _.Shift(seconds: float) =
        agent.Post(Shift seconds)

    member _.Resume() =
        agent.Post(Resume)

    member _.Next() =
        agent.Post(Next)

    member _.AddSubscriber(subscriber: (LooperEvent -> Async<unit>)) =
        agent.Post(Subscribe subscriber)

    member _.Start(subscribers: (LooperEvent -> Async<unit>) list) =
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

    member _.PreloadTimePoint() =
        agent.Post(PreloadTimePoint)

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


    interface IDisposable with
        member this.Dispose() = this.Dispose(true)
        




