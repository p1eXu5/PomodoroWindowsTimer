namespace PomodoroWindowsTimer.TimePointQueue

open PomodoroWindowsTimer.Types
open Priority_Queue
open System.Threading
open System
open FSharp.Control
open PomodoroWindowsTimer.Abstractions

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


type TimePointQueue(timePoints: TimePoint seq, ?cancellationToken: System.Threading.CancellationToken) =
    let mutable _isDisposed = false
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
                        return! loop State.Default

                    | AddMany timePoints ->
                        let maxPriority' = enqueue timePoints (state.MaxPriority)
                        return! loop { state with MaxPriority = maxPriority' }


                    | GetTimePointsWithPriority reply ->
                        let xtp =
                            seq {
                                for tp in state.Queue do
                                    yield (tp, state.Queue.GetPriority(tp))
                            }
                        reply.Reply xtp

                    | Scroll (id, reply) ->
                        if state.Queue.Count = 0 then
                            reply.Reply(())
                            return! loop state
                        else
                            while state.Queue.First.Id <> id do
                                let priority = state.Queue.GetPriority(state.Queue.First)
                                let tp = state.Queue.Dequeue()
                                state.Queue.Enqueue(tp, priority + state.MaxPriority)

                            reply.Reply(())
                            return! loop { state with MaxPriority = state.MaxPriority + (float32 state.Queue.Count) }


                    | GetNext reply ->
                        if state.Queue.Count = 0 then
                            reply.Reply(None)
                        else
                            let tp = state.Queue.Dequeue()
                            state.Queue.Enqueue(tp, state.MaxPriority)
                            reply.Reply(tp |> Some)
                            return! loop { state with MaxPriority = state.MaxPriority + 1f }

                    | Pick reply ->
                        if state.Queue.Count = 0 then
                            reply.Reply(None)
                        else
                            let tp = state.Queue.First
                            reply.Reply(tp |> Some)
                            return! loop state
                }

            loop State.Default
        )
        , defaultArg cancellationToken CancellationToken.None
    )

    let isStarted = ref 0

    new(cancellationToken: System.Threading.CancellationToken) = new TimePointQueue(Seq.empty, cancellationToken)
    new() = new TimePointQueue(Seq.empty)

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

