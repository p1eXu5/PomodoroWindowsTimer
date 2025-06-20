namespace PomodoroWindowsTimer.Tests

open System
open System.Collections

open NUnit.Framework
open Faqt
open Faqt.Operators

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Testing.Fakers

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

    let start23 = ``23:00:00``

    let testCases : IEnumerable =
        seq {
            TestCaseData(
                [
                    (``23:00:00``, "Work 1", TimePointId.generate ()) |> WorkEvent.WorkStarted
                ],
                {
                    Period =
                        {
                            Start = ``23:00:00``.LocalDateTime
                            EndInclusive = ``23:00:00``.LocalDateTime
                        }
                    WorkTime = TimeSpan.Zero
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = ["Work 1"]
                } |> Some
            ).SetName("01: WorkStarted only")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1", TimePointId.generate ()) |> WorkEvent.WorkStarted
                    (``23:10:00``, "Break 1", TimePointId.generate ()) |> WorkEvent.BreakStarted
                ],
                {
                    Period =
                        {
                            Start = ``23:00:00``.LocalDateTime
                            EndInclusive = ``23:10:00``.LocalDateTime
                        }
                    WorkTime = TimeSpan.FromMinutes(10L)
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = ["Work 1"; "Break 1"]
                } |> Some
            ).SetName("02: WorkStarted -> Break Started")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1", TimePointId.generate ()) |> WorkEvent.WorkStarted
                    (``23:10:00``, "Break 1", TimePointId.generate ()) |> WorkEvent.BreakStarted
                    (``23:20:00``, "Work 2", TimePointId.generate ()) |> WorkEvent.WorkStarted
                ],
                {
                    Period =
                        {
                            Start = ``23:00:00``.LocalDateTime
                            EndInclusive = ``23:20:00``.LocalDateTime
                        }
                    WorkTime = TimeSpan.FromMinutes(10L)
                    BreakTime = TimeSpan.FromMinutes(10L)
                    TimePointNameStack = ["Work 1"; "Break 1"; "Work 2"]
                } |> Some
            ).SetName("03: WorkStarted -> Break Started -> WorkStarted")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1", TimePointId.generate ()) |> WorkEvent.WorkStarted
                    (``23:10:00``, "Break 1", TimePointId.generate ()) |> WorkEvent.BreakStarted
                    (``23:20:00``, "Work 2", TimePointId.generate ()) |> WorkEvent.WorkStarted
                    (``23:30:00``, "Long Break", TimePointId.generate ()) |> WorkEvent.BreakStarted
                ],
                {
                    Period =
                        {
                            Start = ``23:00:00``.LocalDateTime
                            EndInclusive = ``23:30:00``.LocalDateTime
                        }
                    WorkTime = TimeSpan.FromMinutes(20L)
                    BreakTime = TimeSpan.FromMinutes(10L)
                    TimePointNameStack = ["Work 1"; "Break 1"; "Work 2"; "Long Break"]
                } |> Some
            ).SetName("04: WorkStarted -> Break Started -> Work Started -> Long Break Started")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1", TimePointId.generate ()) |> WorkEvent.WorkStarted
                    (``23:10:00``) |> WorkEvent.Stopped
                ],
                {
                    Period =
                        {
                            Start = ``23:00:00``.LocalDateTime
                            EndInclusive = ``23:10:00``.LocalDateTime
                        }
                    WorkTime = TimeSpan.FromMinutes(10L)
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = ["Work 1";]
                } |> Some
            ).SetName("05: Work Started -> Stopped")

            TestCaseData(
                [
                    (``23:50:00``, "Work 1", TimePointId.generate ()) |> WorkEvent.WorkStarted
                    (``00:00:00``) |> WorkEvent.Stopped
                ],
                {
                    Period =
                        {
                            Start = ``23:50:00``.LocalDateTime
                            EndInclusive = ``00:00:00``.LocalDateTime
                        }
                    WorkTime = TimeSpan.FromMinutes(10L)
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = ["Work 1";]
                } |> Some
            ).SetName("06: Work Started -> Stopped in next day")

            TestCaseData(
                [
                    (``23:00:00``, "Work 1", TimePointId.generate ()) |> WorkEvent.WorkStarted
                    (``23:10:00``)           |> WorkEvent.Stopped
                    (``23:20:00``, "Work 1", TimePointId.generate ()) |> WorkEvent.WorkStarted
                    (``23:30:00``, "Break 1", TimePointId.generate ()) |> WorkEvent.BreakStarted
                    (``23:40:00``) |> WorkEvent.Stopped
                ],
                {
                    Period =
                        {
                            Start = ``23:00:00``.LocalDateTime
                            EndInclusive = ``23:40:00``.LocalDateTime
                        }
                    WorkTime = TimeSpan.FromMinutes(20L)
                    BreakTime = TimeSpan.FromMinutes(10L)
                    TimePointNameStack = ["Work 1"; "Break 1"]
                } |> Some
            ).SetName("07: Work Started -> Stopped -> Work Started -> Break Started -> Stopped")

            TestCaseData(
                [
                    WorkEvent.WorkStarted   (start23,               "W1", TimePointId.generate ())
                    WorkEvent.WorkIncreased (start23.AddMinutes  5, TimeSpan.FromMinutes 60L, None)
                    WorkEvent.Stopped       (start23.AddMinutes 10)        
                    WorkEvent.WorkStarted   (start23.AddMinutes 20, "W1", TimePointId.generate ())
                    WorkEvent.BreakStarted  (start23.AddMinutes 30, "B1", TimePointId.generate ())
                    WorkEvent.Stopped       (start23.AddMinutes 40)
                ],
                {
                    Period =
                        {
                            Start = start23.LocalDateTime
                            EndInclusive = start23.AddMinutes(40.0).LocalDateTime // by start/stop event created time (not increase/reduce)
                        }
                    WorkTime = TimeSpan.FromMinutes(20. + 60.)
                    BreakTime = TimeSpan.FromMinutes(10L)
                    TimePointNameStack = ["W1"; "B1"]
                } |> Some
            ).SetName("08: Work Started -> WorkIncreased -> Stopped -> Work Started -> Break Started -> Stopped")
        }


    [<TestCaseSource(nameof testCases)>]
    let ``project WorkStarted event test`` (events: WorkEvent list, expectedStat: Statistic option) =
        let stat = WorkEventProjector.projectStatistic events
        %stat.Should().Be(expectedStat)

