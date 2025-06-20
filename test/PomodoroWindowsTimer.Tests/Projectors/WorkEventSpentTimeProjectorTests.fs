namespace PomodoroWindowsTimer.Tests

open System
open System.Threading

open NUnit.Framework
open Faqt
open Faqt.Operators
open p1eXu5.FSharp.Testing.ShouldExtensions
open NSubstitute
open FsToolkit.ErrorHandling

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Testing.Fakers

[<Category("WorkEventSpentTimeProjector")>]
module WorkEventSpentTimeProjectorTests =

    let private timePointId = TimePointId.generate ()
    let private date = DateOnly(2024, 1, 1)
    let private ct = CancellationToken.None
    let private work = Work.generate ()

    let fromMin (v: int64) = TimeSpan.FromMinutes(v)

    let diff (endTime: string) (startTime: string) =
        TimeOnly.ParseExact(endTime, "HH:mm", null) - TimeOnly.ParseExact(startTime, "HH:mm", null)
        |> fun ts -> ts.TotalSeconds * 1.0<sec>

    let mockWorkEventRepo kind notAfter workEventLists =
        let mockWorkEventRepo = Substitute.For<IWorkEventRepository>()
        let _ = 
            (mockWorkEventRepo.FindByActiveTimePointIdByDateAsync timePointId kind notAfter ct)
                .Returns(workEventLists |> Ok)
        mockWorkEventRepo


    [<Test>]
    let ``01: Start-stop work events test`` () =
        taskResult {
            let workEventLists =
                [
                    work, WorkEvent.generateWorkStartedWith date "12:00"
                    work, WorkEvent.generateStoppedWith date "12:05"
                ]
                |> List.rev
            let kind = Kind.Work
            let notAfter = DateTimeOffset(date, TimeOnly(12, 5, 0), TimeSpan.Zero)
            let diff = diff "12:05" "12:00"

            let mockWorkEventRepo = mockWorkEventRepo kind notAfter workEventLists

            let! res = WorkEventSpentTimeProjector.workSpentTimeListTask mockWorkEventRepo timePointId kind notAfter diff ct

            %res.Should().Be([
                {
                    Work = work
                    SpentTime = TimeSpan.FromMinutes(5.0)
                }
            ])

            return ()
        }
        |> TaskResult.runTest

    [<Test>]
    let ``02: Increased work events test`` () =
        taskResult {
            let workEventLists =
                [
                    work, WorkEvent.createWorkIncreasedWith date "12:10" (fromMin 10) None
                ]
            let kind = Kind.Work
            let notAfter = DateTimeOffset(date, TimeOnly(12, 10, 0), TimeSpan.Zero)
            let diff = diff "12:10" "12:00"

            let mockWorkEventRepo = mockWorkEventRepo kind notAfter workEventLists

            let! res = WorkEventSpentTimeProjector.workSpentTimeListTask mockWorkEventRepo timePointId kind notAfter diff ct

            %res.Should().Be([
                {
                    Work = work
                    SpentTime = TimeSpan.FromMinutes(10.0)
                }
            ])

            return ()
        }
        |> TaskResult.runTest

    [<Test>]
    let ``03: Events when playing with increased in pause time test`` () =
        taskResult {
            let workEventLists =
                [
                    work, WorkEvent.generateWorkStartedWith date "12:00"
                    work, WorkEvent.generateStoppedWith date "12:05"
                    work, WorkEvent.createWorkIncreasedWith date "12:10" (fromMin 5) None
                    work, WorkEvent.generateWorkStartedWith date "12:11"
                    work, WorkEvent.generateStoppedWith date "12:16"
                ]
                |> List.rev
            let kind = Kind.Work
            let notAfter = DateTimeOffset(date, TimeOnly(12, 16, 0), TimeSpan.Zero)
            let diff = (diff "12:05" "12:00") + (diff "12:16" "12:11")

            let mockWorkEventRepo = mockWorkEventRepo kind notAfter workEventLists

            let! res = WorkEventSpentTimeProjector.workSpentTimeListTask mockWorkEventRepo timePointId kind notAfter diff ct

            %res.Should().Be([
                {
                    Work = work
                    SpentTime = TimeSpan.FromMinutes(10.0)
                }
            ])

            return ()
        }
        |> TaskResult.runTest

    [<Test>]
    let ``04: Events when playing with increased between time test`` () =
        taskResult {
            let workEventLists =
                [
                    work, WorkEvent.generateWorkStartedWith date "12:00"
                    work, WorkEvent.generateStoppedWith date "12:05"
                    work, WorkEvent.createWorkIncreasedWith date "12:10" (fromMin 5) None
                    work, WorkEvent.generateWorkStartedWith date "12:11"
                    work, WorkEvent.generateStoppedWith date "12:16"
                ]
                |> List.rev
            let kind = Kind.Work
            let notAfter = DateTimeOffset(date, TimeOnly(12, 16, 0), TimeSpan.Zero)
            let diff = (diff "12:16" "12:00")

            let mockWorkEventRepo = mockWorkEventRepo kind notAfter workEventLists

            let! res = WorkEventSpentTimeProjector.workSpentTimeListTask mockWorkEventRepo timePointId kind notAfter diff ct

            %res.Should().Be([
                {
                    Work = work
                    SpentTime = TimeSpan.FromMinutes(15.0)
                }
            ])

            return ()
        }
        |> TaskResult.runTest

    [<Test>]
    let ``06: Events with reduced time test`` () =
        taskResult {
            let workEventLists =
                [
                    work, WorkEvent.generateWorkStartedWith date "12:00"
                    work, WorkEvent.generateStoppedWith date "12:05"
                    work, WorkEvent.createWorkReducedWith date "12:06" (fromMin 5) None

                    work, WorkEvent.generateWorkStartedWith date "12:11"
                    work, WorkEvent.generateStoppedWith date "12:16"
                ]
                |> List.rev
            let kind = Kind.Work
            let notAfter = DateTimeOffset(date, TimeOnly(12, 16, 0), TimeSpan.Zero)
            let diff = (diff "12:05" "12:00") + (diff "12:16" "12:11")

            let mockWorkEventRepo = mockWorkEventRepo kind notAfter workEventLists

            let! res = WorkEventSpentTimeProjector.workSpentTimeListTask mockWorkEventRepo timePointId kind notAfter diff ct

            %res.Should().Be([
                {
                    Work = work
                    SpentTime = TimeSpan.FromMinutes(5.0)
                }
            ])
        }
        |> TaskResult.runTest

    [<Test>]
    let ``07: Single increase event, diff is less, returns diff`` () =
        taskResult {
            let workEventLists =
                [
                    work, WorkEvent.createWorkIncreasedWithNoTimePoint date "12:10" (fromMin 10)
                ]
                |> List.rev
            let kind = Kind.Work
            let notAfter = DateTimeOffset(date, TimeOnly(12, 16, 0), TimeSpan.Zero)
            let diff = (diff "12:10" "12:05")

            let mockWorkEventRepo = mockWorkEventRepo kind notAfter workEventLists

            let! res = WorkEventSpentTimeProjector.workSpentTimeListTask mockWorkEventRepo timePointId kind notAfter diff ct

            %res.Should().Be([
                {
                    Work = work
                    SpentTime = TimeSpan.FromMinutes(5.0)
                }
            ])
        }
        |> TaskResult.runTest


