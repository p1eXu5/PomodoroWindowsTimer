module PomodoroWindowsTimer.Tests.LooperTests

open System
open System.Threading
open System.Collections.Generic

open NUnit.Framework
open FsUnit
open ShouldExtensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper

let private xtp =
    [
        { Id = Guid.NewGuid(); Name = "1"; TimeSpan = TimeSpan.FromSeconds(1); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.NewGuid(); Name = "2"; TimeSpan = TimeSpan.FromSeconds(1); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.NewGuid(); Name = "3"; TimeSpan = TimeSpan.FromSeconds(1); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.NewGuid(); Name = "4"; TimeSpan = TimeSpan.FromSeconds(1); Kind = Work; KindAlias = Work |> Kind.alias }
    ]

let private timePointQueue () =
    let tpQueue = new TimePointQueue()
    tpQueue.Start()
    tpQueue.AddMany(xtp)
    tpQueue

[<Test>]
let ``Next test``() =
    task {
        use tpQueue = timePointQueue ()
        use looper = new Looper(tpQueue, 200<ms>, CancellationToken.None)
        use semaphore = new SemaphoreSlim(0, 1)

        let mutable startedTPStack = Queue<TimePoint>()
        let subscriber looperEvent =
            async {
                if startedTPStack.Count < 8 then
                    match looperEvent with
                    | TimePointStarted (newTimePoint, _) ->
                        startedTPStack.Enqueue(newTimePoint)
                    | _ -> ()
                else
                    semaphore.Release() |> ignore
            }
        looper.Start([ subscriber ])

        //looper.PreloadTimePoint()
        looper.Next()

        let! _ = semaphore.WaitAsync(TimeSpan.FromSeconds(1.0 * 8.0 * 2.0))

        startedTPStack |> Seq.map _.Id |> should equalSeq ((xtp @ xtp) |> Seq.map _.Id)
        // Looper is discrete according to tickMilliseconds parameter. If active taime point time span is less then second, reminder will be added to the next time point
        startedTPStack |> Seq.map _.TimeSpan |> Seq.forall (fun span -> span >= TimeSpan.FromSeconds(1) && span < TimeSpan.FromSeconds(2)) |> should be True
    }
