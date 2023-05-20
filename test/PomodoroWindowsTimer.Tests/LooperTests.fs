module PomodoroWindowsTimer.Tests.LooperTests

open NUnit.Framework
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.TimePointQueue
open System
open FsUnit.TopLevelOperators
open ShouldExtensions.FsUnit
open FSharp.Control

[<Test>]
let ``get test``() =
    async {
        use looper = new TimePointQueue()
        looper.Start()
        let xtp =
            [
                { Id = Guid.NewGuid(); Name = "1"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
                { Id = Guid.NewGuid(); Name = "2"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
                { Id = Guid.NewGuid(); Name = "3"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
                { Id = Guid.NewGuid(); Name = "4"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
            ]
        looper.AddMany(xtp)
        let! storredTp = looper.GetTimePointsWithPriority()

        storredTp |> Seq.map fst |> should equalSeq  xtp
        storredTp
        |> Seq.map snd
        |> Seq.zip [ 0f; 1f; 2f; 3f ]
        |> Seq.iter (fun (expected, actual) -> actual |> shouldL (equalWithin 0.1f) expected $"Actual %A{storredTp |> Seq.map fst}")
    }
    |> toTask

[<Test>]
let ``asyncSeq test``() =
    async {
        use looper = new TimePointQueue()
        looper.Start()
        let xtp =
            [
                { Id = Guid.NewGuid(); Name = "1"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
                { Id = Guid.NewGuid(); Name = "2"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
                { Id = Guid.NewGuid(); Name = "3"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
                { Id = Guid.NewGuid(); Name = "4"; TimeSpan = TimeSpan.FromMilliseconds(100); Kind = Work; KindAlias = Work |> Kind.alias }
            ]
        looper.AddMany(xtp)

        let! first =
            looper.GetAsyncSeq ()
            |> AsyncSeq.tryFirst

        first |> should be (ofCase <@ Option<TimePoint>.Some @>)

        first |> Option.get |> should equal (xtp |> List.head)

        let! storredTp = looper.GetTimePointsWithPriority()

        storredTp |> Seq.map fst |> should equivalent xtp
        storredTp |> Seq.map fst |> should not' (equalSeq xtp)

        storredTp
        |> Seq.map snd
        |> Seq.sort
        |> Seq.zip [ 1f; 2f; 3f; 4f ]
        |> Seq.iter (fun (expected, actual) -> actual |> shouldL (equalWithin 0.1f) expected $"Actual %A{storredTp |> Seq.map fst}")
    }
    |> toTask