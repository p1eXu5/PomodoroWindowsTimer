namespace PomodoroWindowsTimer.Tests
open System
open System.Globalization

open FsToolkit.ErrorHandling
open NUnit.Framework
open p1eXu5.FSharp.Testing.ShouldExtensions
open PomodoroWindowsTimer.Testing
open Faqt
open Faqt.Operators

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Helpers.DateTimeOffset

[<Category("TempExcelRows Tests")>]
module Day24102024Tests =
    let private ``5 min threshold`` = TimeSpan.FromMinutes(5L)

    [<Test>]
    let Work1_1Test () =
        let work1_1 =
            {
                Id = uint64 1
                Number = "KARTA-5097"
                Title = "UnitTests. Сервис для работы с ПФР"
                CreatedAt = DateTimeOffset.ParseExact("14.10.2024 08:31:59.81500 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("14.10.2024 08:31:59.81500 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_1 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:37:27.76207 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("982ce860-9fe9-41af-97a3-358824ffd9d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:38:03.32288 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let workEventOffsetTimes =
            {
                Work = work1_1
                Events = events1_1
            }
            |> List.singleton

        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %act.Should().NotThrow()

        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %res.Should().BeOk()

    [<Test>]
    let Work1_2Test () =
        let work1_2 =
            {
                Id = uint64 2
                Number = "KARTA-5090"
                Title = "UnitTests. Сервис ЕСИА"
                CreatedAt = DateTimeOffset.ParseExact("15.10.2024 06:02:35.52400 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("15.10.2024 06:15:19.59100 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_2 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:13:48.82119 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("982ce860-9fe9-41af-97a3-358824ffd9d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:36:50.78841 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 08:43:27.60927 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("16bb44da-1fec-46fc-a484-07a889f0cc96"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 08:57:13.18212 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let workEventOffsetTimes =
            {
                Work = work1_2
                Events = events1_2
            }
            |> List.singleton

        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %act.Should().NotThrow()

        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %res.Should().BeOk()

    [<Test>]
    let Work1_3Test () =
        let work1_3 =
            {
                Id = uint64 3
                Number = "KARTA-5597"
                Title = "Списание времени на церемонии"
                CreatedAt = DateTimeOffset.ParseExact("16.10.2024 09:28:02.47600 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("16.10.2024 09:28:02.47600 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_3 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:58:53.10362 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("990dda41-6a2c-4244-a68f-b72f0e8b08d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:58:56.36474 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:58:56.37842 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("2663b4e7-bcd9-4fa6-b35e-1f56b3ef1ca1"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 07:16:03.83298 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let workEventOffsetTimes =
            {
                Work = work1_3
                Events = events1_3
            }
            |> List.singleton

        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %act.Should().NotThrow()

        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %res.Should().BeOk()

    [<Test>]
    let Work1_4Test () =
        let work1_4 =
            {
                Id = uint64 4
                Number = "KARTA-5730"
                Title = "UniversalApi. Рефакторинг обработчиков сообщений льгот и услуг"
                CreatedAt = DateTimeOffset.ParseExact("16.10.2024 14:57:00.14000 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("16.10.2024 14:57:00.14000 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_4 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:36:50.78941 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("982ce860-9fe9-41af-97a3-358824ffd9d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:37:27.76107 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let workEventOffsetTimes =
            {
                Work = work1_4
                Events = events1_4
            }
            |> List.singleton

        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %act.Should().NotThrow()

        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %res.Should().BeOk()

    [<Test>]
    let Work1_5Test () =
        let work1_5 =
            {
                Id = uint64 6
                Number = "KARTA-5734"
                Title = "BFR. Рефакторинг обработчиков сообщений льгот и услуг"
                CreatedAt = DateTimeOffset.ParseExact("22.10.2024 11:01:22.76300 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("22.10.2024 11:01:22.76300 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_5 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:06:43.93898 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("b42917bd-b490-4ddf-98e4-77d766403acf"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:38:03.32388 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("982ce860-9fe9-41af-97a3-358824ffd9d5"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 06:38:47.93777 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("0246ff3d-43af-47bb-b05c-4d2f700929da"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:43:47.87498 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("990dda41-6a2c-4244-a68f-b72f0e8b08d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:58:53.10262 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 07:16:03.83398 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("2663b4e7-bcd9-4fa6-b35e-1f56b3ef1ca1"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 07:23:55.44690 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 3", Guid.Parse("412657cc-5a22-4cf5-a426-e616a9381ca9"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 07:23:57.82442 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 4", Guid.Parse("2ad9b21f-ce86-4f22-87ba-c45ed0be52ac"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 07:48:56.85680 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Long Break", Guid.Parse("2eab48fd-174e-4bea-b1df-08ddfca0d0e4"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 07:48:59.34113 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("35a2014b-203c-40be-bb53-906744613686"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 08:06:36.23129 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 08:06:36.23983 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("943b1fe7-c782-41e9-8e05-1351d719dd7b"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 08:11:35.27655 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("6575d971-9609-4659-8042-6d9e86ff0a16"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 08:36:35.42245 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 2", Guid.Parse("19b1d3bf-e67e-42b1-8f22-8a11dcb6f967"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 08:41:35.26569 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("16bb44da-1fec-46fc-a484-07a889f0cc96"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 08:43:27.60827 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 08:57:13.18312 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("16bb44da-1fec-46fc-a484-07a889f0cc96"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 09:06:35.35017 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 3", Guid.Parse("504f4384-e8b0-4b4e-8298-dd01a401d0b1"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 09:06:38.64244 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 4", Guid.Parse("d89027f7-0d14-45ad-804b-ee01cda40944"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 09:31:37.69785 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Long Break", Guid.Parse("106442b7-5d1d-45cf-a5ca-438cdd56d4b7"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 09:31:40.02409 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("1bb201ec-7e51-4d4b-94f3-114a893173af"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 09:56:39.11462 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("74be4e9b-9678-401e-b185-caeaef3b865c"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 09:56:41.44757 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("8fcc925d-8ed9-4676-8bf4-fbdf9898ba4b"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 10:21:40.47599 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 2", Guid.Parse("39d7a803-423c-4846-8d77-bde839413d97"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 10:21:48.26502 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("dd7e3301-6088-4c6c-bb51-3d79ddd4a874"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 10:43:59.20356 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let workEventOffsetTimes =
            {
                Work = work1_5
                Events = events1_5
            }
            |> List.singleton

        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %act.Should().NotThrow()

        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %res.Should().BeOk()

    [<Test>]
    let Work1_6Test () =
        let work1_6 =
            {
                Id = uint64 7
                Number = "KARTA-5748 "
                Title = "UnitTests. Сервис BFR"
                CreatedAt = DateTimeOffset.ParseExact("24.10.2024 10:43:59.14800 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("24.10.2024 10:43:59.14800 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_6 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 10:43:59.20456 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("dd7e3301-6088-4c6c-bb51-3d79ddd4a874"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 10:46:47.39002 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 3", Guid.Parse("81917072-cd38-40c1-b24c-15d507093f5b"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 10:51:47.35443 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 4", Guid.Parse("c998b1e2-26e1-4cac-93d8-6a3624c9b3db"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 11:16:47.27381 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Long Break", Guid.Parse("3f85420a-fea0-4d1d-9484-fd56261edec0"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 11:16:49.29016 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("307ca2e6-8a78-480b-ba51-f609ca49c092"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 11:41:48.50255 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("51ec782b-5107-4a8c-94e2-3360e1b50291"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 11:41:51.28150 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("78798e2b-ee4d-45f9-a069-9a44b7bb32cf"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 12:06:50.31550 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 2", Guid.Parse("369488e6-61e8-4ade-a613-1edb7e48cfc1"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 12:06:52.82500 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("9ede3f28-f9e3-4efc-b050-b6d1043586e7"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 12:31:51.96346 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 3", Guid.Parse("4ab3d918-0178-44fb-ad1b-ebffc0a6cee6"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 12:31:54.10918 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 4", Guid.Parse("75d081fe-eb14-4b1c-b588-8373e4f8f83f"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 12:36:38.80783 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 12:36:38.82282 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Long Break", Guid.Parse("32edb217-49cf-40c9-b406-68620dfd6b60"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 12:56:37.94581 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("d8b545b0-839c-40a4-811a-735e3f5e2234"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 13:21:37.98803 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("e47a8b5e-37c6-43a0-88a6-21b144e2db32"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 13:21:45.80320 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let workEventOffsetTimes =
            {
                Work = work1_6
                Events = events1_6
            }
            |> List.singleton

        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %act.Should().NotThrow()

        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %res.Should().BeOk()

    [<Test>]
    let Work1_7Test () =
        let work1_7 =
            {
                Id = uint64 8
                Number = "KARTA-5807"
                Title = "Auth: Ошибки при добавлении ребенка, добавленного из АС Банка"
                CreatedAt = DateTimeOffset.ParseExact("24.10.2024 13:21:45.78400 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("24.10.2024 13:21:45.78400 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_7 =
            [
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 13:21:45.80420 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("e47a8b5e-37c6-43a0-88a6-21b144e2db32"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 13:25:21.76562 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 13:25:22.31172 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("e47a8b5e-37c6-43a0-88a6-21b144e2db32"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 13:30:21.35847 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("a7d4c76b-5bb0-4f0f-91b8-6a5ebcad6f63"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 13:55:21.32955 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 2", Guid.Parse("4f0918ee-ccf8-4d6e-abbc-52d436422fbf"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 13:55:24.41146 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("4a87fd23-4042-430d-8b20-84ced31c0e25"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 14:11:59.24843 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let workEventOffsetTimes =
            {
                Work = work1_7
                Events = events1_7
            }
            |> List.singleton

        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %act.Should().NotThrow()

        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %res.Should().BeOk()


    [<Test>]
    let AllDay24102024Test () =
        let work1_1 =
            {
                Id = uint64 1
                Number = "KARTA-5097"
                Title = "UnitTests. Сервис для работы с ПФР"
                CreatedAt = DateTimeOffset.ParseExact("14.10.2024 08:31:59.81500 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("14.10.2024 08:31:59.81500 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_1 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:37:27.76207 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("982ce860-9fe9-41af-97a3-358824ffd9d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:38:03.32288 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let work1_2 =
            {
                Id = uint64 2
                Number = "KARTA-5090"
                Title = "UnitTests. Сервис ЕСИА"
                CreatedAt = DateTimeOffset.ParseExact("15.10.2024 06:02:35.52400 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("15.10.2024 06:15:19.59100 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_2 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:13:48.82119 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("982ce860-9fe9-41af-97a3-358824ffd9d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:36:50.78841 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 08:43:27.60927 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("16bb44da-1fec-46fc-a484-07a889f0cc96"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 08:57:13.18212 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let work1_3 =
            {
                Id = uint64 3
                Number = "KARTA-5597"
                Title = "Списание времени на церемонии"
                CreatedAt = DateTimeOffset.ParseExact("16.10.2024 09:28:02.47600 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("16.10.2024 09:28:02.47600 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_3 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:58:53.10362 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("990dda41-6a2c-4244-a68f-b72f0e8b08d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:58:56.36474 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:58:56.37842 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("2663b4e7-bcd9-4fa6-b35e-1f56b3ef1ca1"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 07:16:03.83298 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let work1_4 =
            {
                Id = uint64 4
                Number = "KARTA-5730"
                Title = "UniversalApi. Рефакторинг обработчиков сообщений льгот и услуг"
                CreatedAt = DateTimeOffset.ParseExact("16.10.2024 14:57:00.14000 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("16.10.2024 14:57:00.14000 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_4 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:36:50.78941 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("982ce860-9fe9-41af-97a3-358824ffd9d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:37:27.76107 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let work1_5 =
            {
                Id = uint64 6
                Number = "KARTA-5734"
                Title = "BFR. Рефакторинг обработчиков сообщений льгот и услуг"
                CreatedAt = DateTimeOffset.ParseExact("22.10.2024 11:01:22.76300 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("22.10.2024 11:01:22.76300 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_5 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:06:43.93898 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("b42917bd-b490-4ddf-98e4-77d766403acf"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:38:03.32388 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("982ce860-9fe9-41af-97a3-358824ffd9d5"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 06:38:47.93777 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("0246ff3d-43af-47bb-b05c-4d2f700929da"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 06:43:47.87498 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("990dda41-6a2c-4244-a68f-b72f0e8b08d5"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 06:58:53.10262 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 07:16:03.83398 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("2663b4e7-bcd9-4fa6-b35e-1f56b3ef1ca1"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 07:23:55.44690 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 3", Guid.Parse("412657cc-5a22-4cf5-a426-e616a9381ca9"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 07:23:57.82442 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 4", Guid.Parse("2ad9b21f-ce86-4f22-87ba-c45ed0be52ac"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 07:48:56.85680 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Long Break", Guid.Parse("2eab48fd-174e-4bea-b1df-08ddfca0d0e4"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 07:48:59.34113 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("35a2014b-203c-40be-bb53-906744613686"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 08:06:36.23129 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 08:06:36.23983 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("943b1fe7-c782-41e9-8e05-1351d719dd7b"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 08:11:35.27655 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("6575d971-9609-4659-8042-6d9e86ff0a16"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 08:36:35.42245 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 2", Guid.Parse("19b1d3bf-e67e-42b1-8f22-8a11dcb6f967"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 08:41:35.26569 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("16bb44da-1fec-46fc-a484-07a889f0cc96"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 08:43:27.60827 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 08:57:13.18312 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("16bb44da-1fec-46fc-a484-07a889f0cc96"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 09:06:35.35017 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 3", Guid.Parse("504f4384-e8b0-4b4e-8298-dd01a401d0b1"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 09:06:38.64244 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 4", Guid.Parse("d89027f7-0d14-45ad-804b-ee01cda40944"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 09:31:37.69785 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Long Break", Guid.Parse("106442b7-5d1d-45cf-a5ca-438cdd56d4b7"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 09:31:40.02409 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("1bb201ec-7e51-4d4b-94f3-114a893173af"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 09:56:39.11462 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("74be4e9b-9678-401e-b185-caeaef3b865c"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 09:56:41.44757 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("8fcc925d-8ed9-4676-8bf4-fbdf9898ba4b"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 10:21:40.47599 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 2", Guid.Parse("39d7a803-423c-4846-8d77-bde839413d97"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 10:21:48.26502 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("dd7e3301-6088-4c6c-bb51-3d79ddd4a874"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 10:43:59.20356 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let work1_6 =
            {
                Id = uint64 7
                Number = "KARTA-5748 "
                Title = "UnitTests. Сервис BFR"
                CreatedAt = DateTimeOffset.ParseExact("24.10.2024 10:43:59.14800 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("24.10.2024 10:43:59.14800 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_6 =
            [
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 10:43:59.20456 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("dd7e3301-6088-4c6c-bb51-3d79ddd4a874"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 10:46:47.39002 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 3", Guid.Parse("81917072-cd38-40c1-b24c-15d507093f5b"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 10:51:47.35443 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 4", Guid.Parse("c998b1e2-26e1-4cac-93d8-6a3624c9b3db"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 11:16:47.27381 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Long Break", Guid.Parse("3f85420a-fea0-4d1d-9484-fd56261edec0"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 11:16:49.29016 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("307ca2e6-8a78-480b-ba51-f609ca49c092"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 11:41:48.50255 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("51ec782b-5107-4a8c-94e2-3360e1b50291"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 11:41:51.28150 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("78798e2b-ee4d-45f9-a069-9a44b7bb32cf"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 12:06:50.31550 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 2", Guid.Parse("369488e6-61e8-4ade-a613-1edb7e48cfc1"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 12:06:52.82500 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("9ede3f28-f9e3-4efc-b050-b6d1043586e7"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 12:31:51.96346 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 3", Guid.Parse("4ab3d918-0178-44fb-ad1b-ebffc0a6cee6"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 12:31:54.10918 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 4", Guid.Parse("75d081fe-eb14-4b1c-b588-8373e4f8f83f"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 12:36:38.80783 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 12:36:38.82282 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Long Break", Guid.Parse("32edb217-49cf-40c9-b406-68620dfd6b60"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 12:56:37.94581 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 1", Guid.Parse("d8b545b0-839c-40a4-811a-735e3f5e2234"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 13:21:37.98803 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("e47a8b5e-37c6-43a0-88a6-21b144e2db32"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 13:21:45.80320 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let work1_7 =
            {
                Id = uint64 8
                Number = "KARTA-5807"
                Title = "Auth: Ошибки при добавлении ребенка, добавленного из АС Банка"
                CreatedAt = DateTimeOffset.ParseExact("24.10.2024 13:21:45.78400 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                UpdatedAt = DateTimeOffset.ParseExact("24.10.2024 13:21:45.78400 +00:00", defaultFormat, CultureInfo.InvariantCulture)
                LastEventCreatedAt = None
            }

        let events1_7 =
            [
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 13:21:45.80420 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("e47a8b5e-37c6-43a0-88a6-21b144e2db32"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 13:25:21.76562 +00:00", defaultFormat, CultureInfo.InvariantCulture))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 13:25:22.31172 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 1", Guid.Parse("e47a8b5e-37c6-43a0-88a6-21b144e2db32"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 13:30:21.35847 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 2", Guid.Parse("a7d4c76b-5bb0-4f0f-91b8-6a5ebcad6f63"))
                WorkEvent.BreakStarted (DateTimeOffset.ParseExact("24.10.2024 13:55:21.32955 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Break 2", Guid.Parse("4f0918ee-ccf8-4d6e-abbc-52d436422fbf"))
                WorkEvent.WorkStarted (DateTimeOffset.ParseExact("24.10.2024 13:55:24.41146 +00:00", defaultFormat, CultureInfo.InvariantCulture), "Focused Work 3", Guid.Parse("4a87fd23-4042-430d-8b20-84ced31c0e25"))
                WorkEvent.Stopped (DateTimeOffset.ParseExact("24.10.2024 14:11:59.24843 +00:00", defaultFormat, CultureInfo.InvariantCulture))
            ]

        let workEventOffsetTimes =
            [
            {
                Work = work1_1
                Events = events1_1
            }
            {
                Work = work1_2
                Events = events1_2
            }
            {
                Work = work1_3
                Events = events1_3
            }
            {
                Work = work1_4
                Events = events1_4
            }
            {
                Work = work1_5
                Events = events1_5
            }
            {
                Work = work1_6
                Events = events1_6
            }
            {
                Work = work1_7
                Events = events1_7
            }
            ]
        let act = fun () -> workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %act.Should().NotThrow()

        let res = workEventOffsetTimes |> ExcelExporter.excelRows ``5 min threshold``
        do %res.Should().BeOk()
