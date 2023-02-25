module CycleBell.Looper

open FSharp.Control
open CycleBell.Types
open CycleBell.Abstrractions
open System
open System.Threading

type LooperEvent =
    | TimePointStarted of ``new``: TimePoint * old: TimePoint option 
    | TimePointTimeReduced of TimePoint
    | LoopFinished of TimePoint


type private State = 
    {
        ActiveTimePoint: TimePoint option
        StartTime: DateTime
        Subscribers: (LooperEvent -> Async<unit>) list
        TimeShift: TimeSpan 
    }
    with
        static member Default() =
            {
                ActiveTimePoint = None
                StartTime = DateTime.Now
                Subscribers = []
                TimeShift = TimeSpan.Zero
            }

        static member Reset(subscribers: (LooperEvent -> Async<unit>) list, startTime: DateTime) =
            {
                ActiveTimePoint = None
                StartTime = startTime
                Subscribers = subscribers
                TimeShift = TimeSpan.Zero
            }

        static member Reset(subscribers: (LooperEvent -> Async<unit>) list, startTime: DateTime, timeShift: TimeSpan) =
            {
                ActiveTimePoint = None
                StartTime = DateTime.Now
                Subscribers = subscribers
                TimeShift = timeShift
            }

type private Msg =
    | Tick
    | Subscribe of (LooperEvent -> Async<unit>)
    | SubscribeMany of (LooperEvent -> Async<unit>) list * AsyncReplyChannel<unit>
    | Stop

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
                        return ()
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

                    match msg with
                    | Tick when state.ActiveTimePoint |> Option.isNone ->
                        let! tpOpt = timePointQueue.Enqueue

                        let dt = DateTime.Now

                        match tpOpt with
                        | Some tp ->
                            tryPostEvent (LooperEvent.TimePointStarted (tp, None))
                            timer.Change(tickMilliseconds, 0) |> ignore
                            return! loop { state with ActiveTimePoint = tp |> Some; StartTime = dt; TimeShift = TimeSpan.Zero }

                        | _ ->
                            let! isStopped = isStopMsgQueued
                            if isStopped then
                                return ()
                            else
                                timer.Change(tickMilliseconds, 0) |> ignore
                                return! loop state

                    | Tick ->
                        let atp = state.ActiveTimePoint |> Option.get
                        let dt = DateTime.Now
                        let atp = { atp with TimeSpan = atp.TimeSpan - (dt - state.StartTime) }
                        tryPostEvent (LooperEvent.TimePointTimeReduced atp)

                        if atp.TimeSpan <= TimeSpan.Zero then
                            let! tpOpt = timePointQueue.Enqueue
                            match tpOpt with
                            | None ->
                                timer.Change(tickMilliseconds, 0) |> ignore
                                return! loop (State.Reset(state.Subscribers, dt))
                            | Some tp ->
                                let newAtp = { tp with TimeSpan = tp.TimeSpan + atp.TimeSpan }
                                tryPostEvent (LooperEvent.TimePointStarted (newAtp, atp |> Some))
                                timer.Change(tickMilliseconds, 0) |> ignore
                                return! loop { state with ActiveTimePoint = newAtp |> Some; StartTime = dt }
                        else
                            timer.Change(tickMilliseconds, 0) |> ignore
                            return! loop { state with ActiveTimePoint = atp |> Some; StartTime = dt }

                    | Subscribe subscriber ->
                        return! loop { state with Subscribers = subscriber :: state.Subscribers }

                    | SubscribeMany (subscribers, reply) ->
                        reply.Reply(())
                        return! loop { state with Subscribers = subscribers @ state.Subscribers }

                    | Stop ->
                        return ()
                }

            loop (State.Default())
        )
        , defaultArg cancellationToken CancellationToken.None
    )


    member _.Start(subscribers: (LooperEvent -> Async<unit>) list) =
        timer <-
            new Timer(fun _ ->
                agent.Post(Tick)
            )

        timePointQueue.Start()
        invoker.Start()
        agent.Start()
        do agent.PostAndReply(fun reply -> SubscribeMany (subscribers, reply))
        agent.Post(Tick)


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
        




