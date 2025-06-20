namespace PomodoroWindowsTimer.Tests

open System
open System.Collections

open FsToolkit.ErrorHandling

open NUnit.Framework
open p1eXu5.FSharp.Testing.ShouldExtensions
open PomodoroWindowsTimer.Testing
open Faqt
open Faqt.Operators

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types


module ExcelExporterTests =

    let private ``5 min threshold`` = TimeSpan.FromMinutes(5L)

    [<Test>]
    let ``01: single work events with idle time, threshold is less then idle time`` () =
        result {
            let work = Fakers.Work.generate ()

            let startDt = DateTimeOffset(DateOnly(2024, 1, 1), TimeOnly(8, 0, 0), TimeSpan.Zero)

            let events =
                [
                    WorkEvent.WorkStarted  (startDt, "W1", TimePointId.generate ())
                    WorkEvent.BreakStarted (startDt.AddMinutes 10, "B1", TimePointId.generate ())
                    WorkEvent.Stopped      (startDt.AddMinutes 20)
                    WorkEvent.WorkStarted  (startDt.AddMinutes 60, "W2", TimePointId.generate ())
                    WorkEvent.Stopped      (startDt.AddMinutes 70)
                ]

            let workEventOffsetTimes =
                {
                    Work = work
                    Events = events
                }
                |> List.singleton

            let dt = TimeOnly.FromDateTime(startDt.LocalDateTime)

            let expectedRows =
                [
                    ExcelRow.createWorkExcelRow 1 work (dt.AddMinutes 20)
                    ExcelRow.createIdleExcelRow 2      (dt.AddMinutes 60)
                    ExcelRow.createWorkExcelRow 3 work (dt.AddMinutes 70)
                ]

            let! (_, rows) = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
            do %rows.Should().SequenceEqual(expectedRows)
        }
        |> Result.runTest

    [<Test>]
    let ``02: multiple work events with idle time, threshold is less then idle time`` () =
        result {
            let work1 = Fakers.Work.generate ()
            let work2 = Fakers.Work.generate ()

            let startDt = DateTimeOffset(DateOnly(2024, 1, 1), TimeOnly(8, 0, 0), TimeSpan.Zero)

            let events1 =
                [
                    WorkEvent.WorkStarted  (startDt, "W1", TimePointId.generate ())
                    WorkEvent.BreakStarted (startDt.AddMinutes 10, "B1", TimePointId.generate ())
                    WorkEvent.Stopped      (startDt.AddMinutes 20)
                ]

            let events2 =
                [
                    WorkEvent.WorkStarted  (startDt.AddMinutes 60, "W2", TimePointId.generate ())
                    WorkEvent.Stopped      (startDt.AddMinutes 70)
                ]

            let workEventOffsetTimes =
                [
                    {
                        Work = work1
                        Events = events1
                    }
                    {
                        Work = work2
                        Events = events2
                    }
                ]

            let dt = TimeOnly.FromDateTime(startDt.LocalDateTime)

            let expectedRows =
                [
                    ExcelRow.createWorkExcelRow 1 work1 (dt.AddMinutes 20)
                    ExcelRow.createIdleExcelRow 2       (dt.AddMinutes 60)
                    ExcelRow.createWorkExcelRow 3 work2 (dt.AddMinutes 70)
                ]

            let! (_, rows) = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
            do %rows.Should().SequenceEqual(expectedRows)
        }
        |> Result.runTest


    [<Test>]
    let ``03: single work events with reduce-increase time in the middle`` () =
        result {
            let work = Fakers.Work.generate ()

            let startDt = DateTimeOffset(DateOnly(2024, 1, 1), TimeOnly(8, 0, 0), TimeSpan.Zero)

            let events =
                [
                    WorkEvent.WorkStarted    (startDt, "W1", TimePointId.generate ())
                    WorkEvent.Stopped        (startDt.AddMinutes 20)
                    WorkEvent.WorkReduced    (startDt.AddMinutes 21, TimeSpan.FromMinutes(20L), None)
                    WorkEvent.BreakIncreased (startDt.AddMinutes 22, TimeSpan.FromMinutes(20L), None)
                    WorkEvent.WorkStarted    (startDt.AddMinutes 23, "W1", TimePointId.generate ())
                ]

            let workEventOffsetTimes =
                {
                    Work = work
                    Events = events
                }
                |> List.singleton

            let dt = TimeOnly.FromDateTime(startDt.LocalDateTime)

            let expectedRows =
                [
                    ExcelRow.createWorkExcelRow 1 work (dt.AddMinutes(25)) // minutes rounded to the nearest 5
                ]

            let! (_, rows) = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
            do %rows.Should().SequenceEqual(expectedRows)
        }
        |> Result.runTest

    [<Test>]
    let ``04: single work events with increase time in the middle`` () =
        result {
            let work = Fakers.Work.generate ()

            let startDt = DateTimeOffset(DateOnly(2024, 1, 1), TimeOnly(8, 0, 0), TimeSpan.Zero)

            let events =
                [
                    WorkEvent.WorkStarted    (startDt, "W1", TimePointId.generate ())
                    WorkEvent.WorkIncreased  (startDt.AddMinutes 10, TimeSpan.FromMinutes 60L, None)
                    WorkEvent.Stopped        (startDt.AddMinutes 20)
                    WorkEvent.BreakStarted   (startDt.AddMinutes 20, "B1", TimePointId.generate ())
                    WorkEvent.Stopped        (startDt.AddMinutes 30)
                ]

            let workEventOffsetTimes =
                {
                    Work = work
                    Events = events
                }
                |> List.singleton

            let dt = TimeOnly.FromDateTime(startDt.LocalDateTime)

            let expectedRows =
                [
                    ExcelRow.createWorkExcelRow 1 work (dt.AddMinutes(90))
                ]

            let! (_, rows) = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
            do %rows.Should().SequenceEqual(expectedRows)
        }
        |> Result.runTest

    [<Test>]
    let ``05: multiple work events with increase time in the middle`` () =
        result {
            let work1 = Fakers.Work.generate ()
            let work2 = Fakers.Work.generate ()

            let startDt = DateTimeOffset(DateOnly(2024, 1, 1), TimeOnly(8, 0, 0), TimeSpan.Zero)

            let events1 =
                [
                    WorkEvent.WorkStarted    (startDt, "W1", TimePointId.generate ())
                    WorkEvent.WorkIncreased  (startDt.AddMinutes 10, TimeSpan.FromMinutes 60L, None)
                    WorkEvent.Stopped        (startDt.AddMinutes 20)
                ]

            let events2 =
                [
                    WorkEvent.BreakStarted   (startDt.AddMinutes 20, "B1", TimePointId.generate ())
                    WorkEvent.Stopped        (startDt.AddMinutes 30)
                ]

            let workEventOffsetTimes =
                [
                    { Work = work1; Events = events1 }
                    { Work = work2; Events = events2 }
                ]

            let dt = TimeOnly.FromDateTime(startDt.LocalDateTime)

            let expectedRows =
                [
                    ExcelRow.createWorkExcelRow 1 work1 (dt.AddMinutes(80))
                    ExcelRow.createWorkExcelRow 2 work2 (dt.AddMinutes(90))
                ]

            let! (_, rows) = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
            do %rows.Should().SequenceEqual(expectedRows)
        }
        |> Result.runTest


    [<Test>]
    let ``06: threshold test`` () =
        result {
            let work = Fakers.Work.generate ()

            let startDt = DateTimeOffset(DateOnly(2024, 1, 1), TimeOnly(8, 0, 0), TimeSpan.Zero)

            let events =
                [
                    WorkEvent.WorkStarted  (startDt, "W1", TimePointId.generate ())
                    WorkEvent.Stopped      (startDt.AddMinutes 20)
                    WorkEvent.WorkStarted  (startDt.AddMinutes 24, "W2", TimePointId.generate ())
                    WorkEvent.Stopped      (startDt.AddMinutes 30)
                ]

            let workEventOffsetTimes =
                {
                    Work = work
                    Events = events
                }
                |> List.singleton

            let dt = TimeOnly.FromDateTime(startDt.LocalDateTime)

            let expectedRows =
                [
                    ExcelRow.createWorkExcelRow 1 work (dt.AddMinutes 30)
                ]

            let! (_, rows) = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
            do %rows.Should().SequenceEqual(expectedRows)
        }
        |> Result.runTest

