namespace PomodoroWindowsTimer.TimePointQueue

open PomodoroWindowsTimer.Types
open Priority_Queue
open System.Threading
open System
open FSharp.Control
open PomodoroWindowsTimer.Abstrractions

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

    new(cancellationToken: System.Threading.CancellationToken) = new TimePointQueue(Seq.empty, cancellationToken)
    new() = new TimePointQueue(Seq.empty)

    member _.ConnectImpulse() = ()

    member _.AddMany(timePoints: TimePoint seq) =
        _agent.Post(AddMany (timePoints |> Seq.readonly))
            
    member _.GetTimePointsWithPriority() =
        _agent.PostAndAsyncReply(GetTimePointsWithPriority)

    member _.GetAsyncSeq() =
        let rec loop () =
            asyncSeq {
                let! nextTp = _agent.PostAndAsyncReply(GetNext, 1000)
                if nextTp |> Option.isSome then
                    yield nextTp |> Option.get
                    yield! loop ()
            }
        loop ()

    member this.Start() =
        _agent.Start()
        if timePoints |> (not << Seq.isEmpty) then
            this.AddMany(timePoints)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                (_agent :> IDisposable).Dispose()

            _isDisposed <- true
                

    interface ITimePointQueue with
        member this.Start() = this.Start()
        member _.Enqueue = _agent.PostAndAsyncReply(GetNext, 1000)
        member _.Pick = _agent.PostAndReply(Pick, 1000)
        member _.Scroll(id: Guid) =
            async {
                let! _ = _agent.PostAndAsyncReply(fun reply -> Scroll (id, reply))
                return ()
            }

    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)

