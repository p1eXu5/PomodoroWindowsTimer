module PomodoroWindowsTimer.Tests.LooperTests

open System
open System.Threading
open System.Collections.Generic

open NUnit.Framework
open FsUnit
open NSubstitute
open p1eXu5.FSharp.Testing.ShouldExtensions
open Faqt
open Faqt.Operators

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper
open p1eXu5.AspNetCore.Testing.Logging
open Microsoft.Extensions.Logging


let private testTimePoints =
    [
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"); Name = "1"; TimeSpan = TimeSpan.FromSeconds(1); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"); Name = "2"; TimeSpan = TimeSpan.FromSeconds(1); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"); Name = "3"; TimeSpan = TimeSpan.FromSeconds(1); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"); Name = "4"; TimeSpan = TimeSpan.FromSeconds(1); Kind = Work; KindAlias = Work |> Kind.alias }
    ]

let timeProvider () =
    System.TimeProvider.System

let testLooper (timePoints: TimePoint list) (setupTime: System.TimeProvider -> System.TimeProvider) =
    let cts = new CancellationTokenSource(TimeSpan.FromSeconds(60))

    let tpQueue = new TimePointQueue(TestLogger<TimePointQueue>(tcw, LogOut.Out ||| LogOut.Progress) :> ILogger<TimePointQueue>, -1<ms>, cts.Token)
    tpQueue.Start()
    tpQueue.AddMany(timePoints)

    let timeProvider = Substitute.For<System.TimeProvider>()

    let looper = new Looper(tpQueue, setupTime timeProvider, 200<ms>, TestLogger<Looper>(tcw, LogOut.Out ||| LogOut.Progress) :> ILogger<Looper>, cts.Token)
    let eventQueue = Queue<LooperEvent>()
    let subscriber looperEvent =
        async {
            eventQueue.Enqueue(looperEvent)
        }
    looper.Start([ subscriber ])
    (
        looper,
        eventQueue,
        { new IDisposable with 
            member _.Dispose() =
                (looper :> IDisposable).Dispose()
                cts.Dispose()
        }
    )

let waitEventCount count (eventQueue: IReadOnlyCollection<LooperEvent>) =
    SpinWait.SpinUntil((fun () -> eventQueue.Count = count), TimeSpan.FromSeconds(5))
    |> ignore

let date = DateOnly(2024, 01, 01)
let startTime = TimeOnly(8, 0, 0)

[<Test>]
let ``PreloadTimePoint -> raises TimePointStarted event with None nextTp`` () =
    let (looper, eventQueue, disp) =
        testLooper testTimePoints (fun mock ->
            %mock.LocalTimeZone.Returns(TimeZoneInfo.Utc)
            %mock.GetUtcNow().Returns(DateTimeOffset(date, startTime, TimeSpan.Zero))
            mock
        )
    use _ = disp

    do looper.PreloadTimePoint()
    do eventQueue |> waitEventCount 1

    %eventQueue.Should().Contain(LooperEvent.TimePointStarted (TimePointStartedEventArgs.init testTimePoints[0] testTimePoints[0] None))


//[<Test>]
//let ``Next test``() =
//    task {
//        use tpQueue = timePointQueue ()
//        use looper = new Looper(tpQueue, timeProvider (), 200<ms>, TestLogger<Looper>(tcw, LogOut.Out ||| LogOut.Progress) :> ILogger<Looper>, CancellationToken.None)
//        use semaphore = new SemaphoreSlim(0, 1)

//        let mutable startedTPStack = Queue<TimePoint>()
//        let subscriber looperEvent =
//            async {
//                if startedTPStack.Count < 8 then
//                    match looperEvent with
//                    | TimePointStarted (newTimePoint, _) ->
//                        startedTPStack.Enqueue(newTimePoint)
//                    | _ -> ()
//                else
//                    semaphore.Release() |> ignore
//            }
//        looper.Start([ subscriber ])

//        //looper.PreloadTimePoint()
//        looper.Next()

//        let! _ = semaphore.WaitAsync(TimeSpan.FromSeconds(1.0 * 8.0 * 2.0))

//        startedTPStack |> Seq.map _.Id |> should equalSeq ((testTimePoints @ testTimePoints) |> Seq.map _.Id)
//        // Looper is discrete according to tickMilliseconds parameter. If active taime point time span is less then second, reminder will be added to the next time point
//        startedTPStack |> Seq.map _.TimeSpan |> Seq.forall (fun span -> span >= TimeSpan.FromSeconds(1) && span < TimeSpan.FromSeconds(2)) |> should be True
//    }
