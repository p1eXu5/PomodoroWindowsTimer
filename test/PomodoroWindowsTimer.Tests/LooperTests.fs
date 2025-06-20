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
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"); Name = "1"; TimeSpan = TimeSpan.FromSeconds(1L); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"); Name = "2"; TimeSpan = TimeSpan.FromSeconds(1L); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"); Name = "3"; TimeSpan = TimeSpan.FromSeconds(1L); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"); Name = "4"; TimeSpan = TimeSpan.FromSeconds(1L); Kind = Work; KindAlias = Work |> Kind.alias }
    ]

let timeProvider () =
    System.TimeProvider.System

let testLooper (timePoints: TimePoint list) (setupTime: System.TimeProvider -> System.TimeProvider) =
    let cts = new CancellationTokenSource(TimeSpan.FromSeconds(60L))

    let tpQueue = new TimePointQueue(TestLogger<TimePointQueue>(TestContextWriters.GetInstance<TestContext>(), LogOut.All) :> ILogger<TimePointQueue>, -1<ms>, cts.Token)
    tpQueue.Start()
    tpQueue.AddMany(timePoints)

    let timeProvider = Substitute.For<System.TimeProvider>()

    let looper = new Looper(tpQueue, setupTime timeProvider, 200<ms>, TestLogger<Looper>(TestContextWriters.GetInstance<TestContext>(), LogOut.All) :> ILogger<Looper>, cts.Token)
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
    SpinWait.SpinUntil((fun () -> eventQueue.Count = count), TimeSpan.FromSeconds(5L))
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

    // act
    do looper.PreloadTimePoint()
    do eventQueue |> waitEventCount 1

    // assert
    %eventQueue.Should().ContainExactlyOneItemMatching(fun ev -> 
        match ev with
        | LooperEvent.TimePointStarted args ->
            args.NewActiveTimePoint.OriginalId = testTimePoints[0].Id && args.OldActiveTimePoint = None
        | _ -> false
    )

[<Test>]
let ``PreloadTimePoint, Next -> raises TimePointStarted event with None nextTp two times`` () =
    let (looper, eventQueue, disp) =
        testLooper testTimePoints (fun mock ->
            %mock.LocalTimeZone.Returns(TimeZoneInfo.Utc)
            %mock.GetUtcNow().Returns(DateTimeOffset(date, startTime, TimeSpan.Zero))
            mock
        )
    use _ = disp

    do looper.PreloadTimePoint()
    do eventQueue |> waitEventCount 1

    // act
    do looper.Next()
    do eventQueue |> waitEventCount 2

    // assert
    %eventQueue.Should().AllSatisfy(fun ev -> 
        match ev with
        | LooperEvent.TimePointStarted args ->
            args.NewActiveTimePoint.OriginalId = testTimePoints[0].Id && args.OldActiveTimePoint = None
        | _ -> false
    )

