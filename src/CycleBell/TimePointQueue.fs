namespace CycleBell.TimePointQueue

open CycleBell.Types
open Priority_Queue
open System.Threading
open System
open FSharp.Control
open CycleBell.Abstrractions

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

                    | GetNext reply ->
                        if state.Queue.Count = 0 then
                            reply.Reply(None)
                        else
                            let tp = state.Queue.Dequeue()
                            state.Queue.Enqueue(tp, state.MaxPriority)
                            reply.Reply(tp |> Some)
                            return! loop { state with MaxPriority = state.MaxPriority + 1f }
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
            

    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)

