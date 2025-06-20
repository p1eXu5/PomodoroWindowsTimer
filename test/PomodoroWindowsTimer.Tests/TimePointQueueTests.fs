module PomodoroWindowsTimer.Tests.TimePointQueueTests

open System

open NUnit.Framework
open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions

open System.Threading
open Microsoft.Extensions.Logging

open p1eXu5.AspNetCore.Testing.Logging

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.TimePointQueue


let private testTimePoints =
    [
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"); Name = "1"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"); Name = "2"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"); Name = "3"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
        { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"); Name = "4"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
    ]

let private timePointQueue () =
    let cts = new CancellationTokenSource(TimeSpan.FromSeconds(60L))
    let tpQueue = new TimePointQueue(TestLogger<TimePointQueue>(TestContextWriters.GetInstance<TestContext>(), LogOut.All) :> ILogger<TimePointQueue>, -1<ms>, cts.Token)
    tpQueue.Start()
    tpQueue

[<Test>]
let ``01: AddMany -> GetAllWithPriority test``() =
    use tpQueue = timePointQueue ()

    tpQueue.AddMany(testTimePoints)

    // action:
    let storredTp = tpQueue.GetAllWithPriority()

    // assert:
    storredTp |> Seq.map fst |> should equalSeq testTimePoints
    storredTp |> Seq.map (snd >> int) |> should equalSeq [ 0; 1; 2; 3 ]

[<Test>]
let ``02-1: AddMany -> TryGetNext test``() =
    use tpQueue = timePointQueue ()
    tpQueue.AddMany(testTimePoints)

    // action:
    let nextTp = tpQueue.TryGetNext()

    nextTp |> should equal (Some testTimePoints[0])

    let storredTp = tpQueue.GetAllWithPriority() |> Seq.toList
    storredTp |> List.map fst |> shouldL equalSeq [ testTimePoints[1];  testTimePoints[2]; testTimePoints[3]; testTimePoints[0]; ] $"Actual is: %A{storredTp |> JsonHelpers.Serialize}"
    let priorities = storredTp |> List.map (snd >> int)
    priorities |> shouldL equalSeq [ 1; 2; 3; 4 ] $"Actual is: %A{priorities}"

[<Test>]
let ``02-2: AddMany -> multiple TryGetNext test``() =
    use tpQueue = timePointQueue ()
    tpQueue.AddMany(testTimePoints)

    // action:
    let outQueue =
        seq { 1 .. 12 }
        |> Seq.map (fun _ -> tpQueue.TryGetNext())
        |> Seq.choose id

    // assert:
    outQueue |> should equalSeq (testTimePoints @ testTimePoints @ testTimePoints)

[<Test>]
let ``03: AddMany -> ScrollTo test``() =
    use tpQueue = timePointQueue ()
    tpQueue.AddMany(testTimePoints)

    // action:
    tpQueue.ScrollTo(testTimePoints[2].Id)

    let storredTp = tpQueue.GetAllWithPriority() |> Seq.toList
    storredTp |> List.map fst |> shouldL equalSeq [ testTimePoints[2];  testTimePoints[3]; testTimePoints[0]; testTimePoints[1]; ] $"Actual is: %A{storredTp |> JsonHelpers.Serialize}"
    let priorities = storredTp |> List.map (snd >> int)
    priorities |> shouldL equalSeq [ -4; -3; 0; 1 ] $"Actual is: %A{priorities}"

[<Test>]
let ``04: AddMany -> ScrollTo -> TryGetNext test``() =
    use tpQueue = timePointQueue ()
    tpQueue.AddMany(testTimePoints)

    // action:
    tpQueue.ScrollTo(testTimePoints[2].Id)

    let nextTp = tpQueue.TryGetNext()

    nextTp |> should equal (Some testTimePoints[2])

[<Test>]
let ``05: AddMany -> TryGetNext -> ScrollTo -> TryGetNext test``() =
    use tpQueue = timePointQueue ()
    tpQueue.AddMany(testTimePoints)

    // action:
    tpQueue.TryGetNext() |> ignore
    tpQueue.ScrollTo(testTimePoints[2].Id)

    let nextTp = tpQueue.TryGetNext()

    nextTp |> should equal (Some testTimePoints[2])

    let storredTp = tpQueue.GetAllWithPriority() |> Seq.toList
    storredTp |> List.map fst |> shouldL equalSeq [ testTimePoints[3];  testTimePoints[0]; testTimePoints[1]; testTimePoints[2]; ] $"Actual is: %A{storredTp |> JsonHelpers.Serialize}"
    let priorities = storredTp |> List.map (snd >> int)
    priorities |> shouldL equalSeq [ -3; -2; 0; 1 ] $"Actual is: %A{priorities}"

[<Test>]
let ``06: AddMany -> multiple TryGetNext and ScrollTo test``() =
    use tpQueue = timePointQueue ()
    tpQueue.AddMany(testTimePoints)

    // action:
    do
        seq { 1 .. 13 }
        |> Seq.iter (fun _ -> tpQueue.TryGetNext() |> ignore)

    tpQueue.ScrollTo(testTimePoints[2].Id)

    let nextTp = tpQueue.TryGetNext()
    nextTp |> should equal (Some testTimePoints[2])

    tpQueue.ScrollTo(testTimePoints[0].Id)

    let nextTp = tpQueue.TryGetNext()
    nextTp |> should equal (Some testTimePoints[0])

    tpQueue.ScrollTo(testTimePoints[3].Id)

    let nextTp = tpQueue.TryGetNext()
    nextTp |> should equal (Some testTimePoints[3])

    let storredTp = tpQueue.GetAllWithPriority() |> Seq.toList
    storredTp |> List.map fst |> shouldL equalSeq testTimePoints $"Actual is: %A{storredTp |> JsonHelpers.Serialize}"
    let priorities = storredTp |> List.map (snd >> int)
    priorities |> shouldL equalSeq [ -3; 0; 1; 2 ] $"Actual is: %A{priorities}"

