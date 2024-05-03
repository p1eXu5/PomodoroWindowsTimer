namespace PomodoroWindowsTimer.Tests

open System
open System.Collections

open NUnit.Framework
open FsUnit
open PomodoroWindowsTimer.Testing.Fakers

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.WorkEventProjector

[<Category("Projector")>]
module WorkEventProjectorTests =

    module Date =
        let ``January, 1st`` = DateOnly(2024, 1, 1)
        let ``January, 2st`` = DateOnly(2024, 1, 2)

    let ``23:00:00`` = DateTimeOffset(Date.``January, 1st``, TimeOnly(23, 00, 0), TimeSpan.Zero)
    let ``23:10:00`` = DateTimeOffset(Date.``January, 1st``, TimeOnly(23, 10, 0), TimeSpan.Zero)
    let ``23:20:00`` = DateTimeOffset(Date.``January, 1st``, TimeOnly(23, 20, 0), TimeSpan.Zero)
    let ``23:30:00`` = DateTimeOffset(Date.``January, 1st``, TimeOnly(23, 30, 0), TimeSpan.Zero)
    let ``23:40:00`` = DateTimeOffset(Date.``January, 1st``, TimeOnly(23, 40, 0), TimeSpan.Zero)
    let ``23:50:00`` = DateTimeOffset(Date.``January, 1st``, TimeOnly(23, 50, 0), TimeSpan.Zero)
    let ``00:00:00`` = DateTimeOffset(Date.``January, 2st``, TimeOnly(00, 00, 0), TimeSpan.Zero)
    let ``00:10:00`` = DateTimeOffset(Date.``January, 2st``, TimeOnly(00, 10, 0), TimeSpan.Zero)
    let ``00:20:00`` = DateTimeOffset(Date.``January, 2st``, TimeOnly(00, 20, 0), TimeSpan.Zero)
    let ``00:30:00`` = DateTimeOffset(Date.``January, 2st``, TimeOnly(00, 30, 0), TimeSpan.Zero)


    let testCases : IEnumerable =
        seq {
            TestCaseData(
                [
                    (``23:00:00``, "Work 1") |> WorkEvent.WorkStarted
                ],
                {
                    Period =
                        {
                            Start = Date.``January, 1st``
                            EndInclusive = Date.``January, 1st``
                        }
                    WorkTime = TimeSpan.Zero
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = ["Work 1"]
                } |> Some
            ).SetName("01: WorkStarted only")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1") |> WorkEvent.WorkStarted
                    (``23:10:00``, "Break 1") |> WorkEvent.BreakStarted
                ],
                {
                    Period =
                        {
                            Start = Date.``January, 1st``
                            EndInclusive = Date.``January, 1st``
                        }
                    WorkTime = TimeSpan.FromMinutes(10)
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = ["Work 1"; "Break 1"]
                } |> Some
            ).SetName("02: WorkStarted -> Break Started")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1") |> WorkEvent.WorkStarted
                    (``23:10:00``, "Break 1") |> WorkEvent.BreakStarted
                    (``23:20:00``, "Work 2") |> WorkEvent.WorkStarted
                ],
                {
                    Period =
                        {
                            Start = Date.``January, 1st``
                            EndInclusive = Date.``January, 1st``
                        }
                    WorkTime = TimeSpan.FromMinutes(10)
                    BreakTime = TimeSpan.FromMinutes(10)
                    TimePointNameStack = ["Work 1"; "Break 1"; "Work 2"]
                } |> Some
            ).SetName("03: WorkStarted -> Break Started -> WorkStarted")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1") |> WorkEvent.WorkStarted
                    (``23:10:00``, "Break 1") |> WorkEvent.BreakStarted
                    (``23:20:00``, "Work 2") |> WorkEvent.WorkStarted
                    (``23:30:00``, "Long Break") |> WorkEvent.BreakStarted
                ],
                {
                    Period =
                        {
                            Start = Date.``January, 1st``
                            EndInclusive = Date.``January, 1st``
                        }
                    WorkTime = TimeSpan.FromMinutes(20)
                    BreakTime = TimeSpan.FromMinutes(10)
                    TimePointNameStack = ["Work 1"; "Break 1"; "Work 2"; "Long Break"]
                } |> Some
            ).SetName("04: WorkStarted -> Break Started -> Work Started -> Long Break Started")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1") |> WorkEvent.WorkStarted
                    (``23:10:00``) |> WorkEvent.Stopped
                ],
                {
                    Period =
                        {
                            Start = Date.``January, 1st``
                            EndInclusive = Date.``January, 1st``
                        }
                    WorkTime = TimeSpan.FromMinutes(10)
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = ["Work 1";]
                } |> Some
            ).SetName("05: Work Started -> Stopped")

            TestCaseData(
                [
                    (``23:50:00``, "Work 1") |> WorkEvent.WorkStarted
                    (``00:00:00``) |> WorkEvent.Stopped
                ],
                {
                    Period =
                        {
                            Start = Date.``January, 1st``
                            EndInclusive = Date.``January, 2st``
                        }
                    WorkTime = TimeSpan.FromMinutes(10)
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = ["Work 1";]
                } |> Some
            ).SetName("06: Work Started -> Stopped in next day")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1") |> WorkEvent.WorkStarted
                    (``23:10:00``) |> WorkEvent.Stopped
                    (``23:20:00``, "Work 1") |> WorkEvent.WorkStarted
                    (``23:30:00``, "Break 1") |> WorkEvent.BreakStarted
                    (``23:40:00``) |> WorkEvent.Stopped
                ],
                {
                    Period =
                        {
                            Start = Date.``January, 1st``
                            EndInclusive = Date.``January, 1st``
                        }
                    WorkTime = TimeSpan.FromMinutes(20)
                    BreakTime = TimeSpan.FromMinutes(10)
                    TimePointNameStack = ["Work 1"; "Break 1"]
                } |> Some
            ).SetName("07: Work Started -> Stopped -> Work Started -> Break Started -> Stopped")
        }


    [<TestCaseSource(nameof testCases)>]
    let ``project WorkStarted event test`` (events: WorkEvent list, expectedStat: Statistic option) =
        let stat = WorkEventProjector.project events
        stat |> should equal expectedStat

