module PomodoroWindowsTimer.Tests.TimePointQueueTests

open System

open NUnit.Framework
open FsUnit
open ShouldExtensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.TimePointQueue
open p1eXu5.AspNetCore.Testing.Logging
open Microsoft.Extensions.Logging

[<Test>]
let ``AddMany -> GetTimePointsWithPriority test``() =
    use tpQueue = new TimePointQueue(TestLogger<TimePointQueue>(TestContextWriters.Default, LogOut.All) :> ILogger<TimePointQueue>)
    tpQueue.Start()
    let xtp =
        [
            { Id = Guid.NewGuid(); Name = "1"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "2"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "3"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "4"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
        ]
    tpQueue.AddMany(xtp)
    let storredTp = tpQueue.GetTimePointsWithPriority()

    storredTp |> Seq.map fst |> should equalSeq  xtp
    storredTp
    |> Seq.map snd
    |> Seq.zip [ 0f; 1f; 2f; 3f ]
    |> Seq.iter (fun (expected, actual) -> actual |> shouldL (equalWithin 0.1f) expected $"Actual %A{storredTp |> Seq.map fst}")


[<Test>]
let ``AddMany -> Enqueue test``() =
    use tpQueue = new TimePointQueue(TestLogger<TimePointQueue>(TestContextWriters.Default, LogOut.All) :> ILogger<TimePointQueue>)
    tpQueue.Start()
    let xtp =
        [
            { Id = Guid.NewGuid(); Name = "1"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "2"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "3"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "4"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
        ]
    tpQueue.AddMany(xtp)

    let outQueue =
        seq { 1 .. 12 }
        |> Seq.map (fun _ -> tpQueue.TryEnqueue)
        |> Seq.choose id

    outQueue |> should equalSeq (xtp @ xtp @ xtp)

