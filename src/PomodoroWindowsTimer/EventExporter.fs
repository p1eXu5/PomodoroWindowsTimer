module PomodoroWindowsTimer.EventExporter

open System.IO

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Helpers.DateTimeOffset

let export (fileName: string) (workEvents: WorkEventList list) =
    task {
        let dailyRows =
            workEvents
            |> WorkEventList.List.groupByDay
            |> List.map (fun (day, wel) ->
                wel
                |> List.map (fun w -> (w.Work, w.Events |> List.head |> WorkEvent.createdAt, w.Events |> List.map WorkEvent.toString))
                |> fun t -> (day, t)
            )

        use sw = File.CreateText(fileName)
        do! sw.WriteLineAsync("namespace PomodoroWindowsTimer.Tests")
        do! sw.WriteLineAsync("open System")
        do! sw.WriteLineAsync("open System.Globalization")
        do! sw.WriteLineAsync("")
        do! sw.WriteLineAsync("open FsToolkit.ErrorHandling")
        do! sw.WriteLineAsync("open NUnit.Framework")
        do! sw.WriteLineAsync("open p1eXu5.FSharp.Testing.ShouldExtensions")
        do! sw.WriteLineAsync("open PomodoroWindowsTimer.Testing")
        do! sw.WriteLineAsync("open Faqt")
        do! sw.WriteLineAsync("open Faqt.Operators")
        do! sw.WriteLineAsync("")
        do! sw.WriteLineAsync("open PomodoroWindowsTimer")
        do! sw.WriteLineAsync("open PomodoroWindowsTimer.Types")
        do! sw.WriteLineAsync("open PomodoroWindowsTimer.Helpers.DateTimeOffset")
        do! sw.WriteLineAsync("")

        let mutable i = 1
        for (day, workEvents) in dailyRows do
            do! sw.WriteLineAsync("[<Category(\"TempExcelRows Tests\")>]")
            do! sw.WriteLineAsync($"module Day{day.Day}{day.Month}{day.Year}Tests =")
            do! sw.WriteLineAsync($"    let private ``5 min threshold`` = TimeSpan.FromMinutes(5)")
            do! sw.WriteLineAsync("")
            let mutable j = 1
            for (work, start, events) in workEvents do
                do! sw.WriteLineAsync($"    [<Test>]")
                do! sw.WriteLineAsync($"    let Work{i}_{j}Test () =")
                do! sw.WriteLineAsync($"        let work{i}_{j} =")
                do! sw.WriteLineAsync("            {")
                do! sw.WriteLineAsync($"                Id = uint64 {work.Id.ToString()}")
                do! sw.WriteLineAsync($"                Number = \"{work.Number}\"")
                do! sw.WriteLineAsync($"                Title = \"{work.Title}\"")
                do! sw.WriteLineAsync($"                CreatedAt = DateTimeOffset.ParseExact(\"{work.CreatedAt.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture)")
                do! sw.WriteLineAsync($"                UpdatedAt = DateTimeOffset.ParseExact(\"{work.UpdatedAt.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture)")
                let lastEventCreatedAt =
                    match work.LastEventCreatedAt with
                    | Some dt -> $"DateTimeOffset.Parse(\"{dt.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture) |> Some"
                    | None -> "None"
                do! sw.WriteLineAsync($"                LastEventCreatedAt = {lastEventCreatedAt}")
                do! sw.WriteLineAsync("            }")
                do! sw.WriteLineAsync("")
                do! sw.WriteLineAsync($"        let events{i}_{j} =")
                do! sw.WriteLineAsync("            [")
                for ev in events do
                    do! sw.WriteLineAsync($"                {ev}")
                do! sw.WriteLineAsync("            ]")
                do! sw.WriteLineAsync("")
                do! sw.WriteLineAsync("        let workEventOffsetTimes =")
                do! sw.WriteLineAsync("            {")
                do! sw.WriteLineAsync($"                Work = work{i}_{j}")
                do! sw.WriteLineAsync($"                Events = events{i}_{j}")
                do! sw.WriteLineAsync("            }")
                do! sw.WriteLineAsync("            |> List.singleton")
                do! sw.WriteLineAsync("")
                do! sw.WriteLineAsync("        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``")
                do! sw.WriteLineAsync("        do %act.Should().NotThrow()")
                do! sw.WriteLineAsync("")
                do! sw.WriteLineAsync("        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``")
                do! sw.WriteLineAsync("        do %res.Should().BeOk()")
                do! sw.WriteLineAsync("")
                j <- j + 1
            i <- i + 1
        do! sw.WriteLineAsync("")

        i <- 1
        for (day, workEvents) in dailyRows do
            do! sw.WriteLineAsync($"    [<Test>]")
            do! sw.WriteLineAsync($"    let AllDay{day.Day}{day.Month}{day.Year}Test () =")
            let mutable j = 1
            for (work, start, events) in workEvents do
                do! sw.WriteLineAsync($"        let work{i}_{j} =")
                do! sw.WriteLineAsync("            {")
                do! sw.WriteLineAsync($"                Id = uint64 {work.Id.ToString()}")
                do! sw.WriteLineAsync($"                Number = \"{work.Number}\"")
                do! sw.WriteLineAsync($"                Title = \"{work.Title}\"")
                do! sw.WriteLineAsync($"                CreatedAt = DateTimeOffset.ParseExact(\"{work.CreatedAt.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture)")
                do! sw.WriteLineAsync($"                UpdatedAt = DateTimeOffset.ParseExact(\"{work.UpdatedAt.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture)")
                let lastEventCreatedAt =
                    match work.LastEventCreatedAt with
                    | Some dt -> $"DateTimeOffset.Parse(\"{dt.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture) |> Some"
                    | None -> "None"
                do! sw.WriteLineAsync($"                LastEventCreatedAt = {lastEventCreatedAt}")
                do! sw.WriteLineAsync("            }")
                do! sw.WriteLineAsync("")
                do! sw.WriteLineAsync($"        let events{i}_{j} =")
                do! sw.WriteLineAsync("            [")
                for ev in events do
                    do! sw.WriteLineAsync($"                {ev}")
                do! sw.WriteLineAsync("            ]")
                do! sw.WriteLineAsync("")
                j <- j + 1

            j <- 1
            do! sw.WriteLineAsync("        let workEventOffsetTimes =")
            do! sw.WriteLineAsync("            [")
            for (work, start, events) in workEvents do
                do! sw.WriteLineAsync("            {")
                do! sw.WriteLineAsync($"                Work = work{i}_{j}")
                do! sw.WriteLineAsync($"                Events = events{i}_{j}")
                do! sw.WriteLineAsync("            }")
                j <- j + 1
            do! sw.WriteLineAsync("            ]")
            do! sw.WriteLineAsync("        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``")
            do! sw.WriteLineAsync("        do %act.Should().NotThrow()")
            do! sw.WriteLineAsync("")
            do! sw.WriteLineAsync("        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``")
            do! sw.WriteLineAsync("        do %res.Should().BeOk()")
            i <- i + 1

        return Ok ()
    }