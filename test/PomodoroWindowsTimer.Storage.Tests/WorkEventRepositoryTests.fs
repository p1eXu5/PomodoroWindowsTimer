namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO
open Microsoft.Extensions.Options

open NUnit.Framework
open FsUnit
open p1eXu5.AspNetCore.Testing.Logging
open p1eXu5.FSharp.Testing.ShouldExtensions
open FsToolkit.ErrorHandling

open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Storage.Configuration
open PomodoroWindowsTimer.Testing.Fakers
open FsUnitTyped
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open p1eXu5.FSharp.Testing.ShouldExtensions



[<Category("DB. WorkEvent")>]
module WorkEventRepositoryTests =

    let private dbFileName = "work_event_test.db"

    let mutable private connectionString = Unchecked.defaultof<string>
    let mutable private workId1 = Unchecked.defaultof<uint64>
    let mutable private workId2 = Unchecked.defaultof<uint64>

    let getConnectionString () =
        if String.IsNullOrWhiteSpace(connectionString) then
            let dataSource = dbFileName |> dataSource
            if File.Exists(dataSource) then
                File.Delete(dataSource)
            connectionString <- $"Data Source=%s{dataSource};"
            connectionString
        else
            connectionString

    let workDbOptions () =
        { new IOptions<WorkDbOptions> with
            member _.Value : WorkDbOptions = 
                { ConnectionString=getConnectionString () }
        }

    let workRepository () =
        new WorkRepository(
            workDbOptions (),
            System.TimeProvider.System,
            TestLogger<WorkRepository>(TestContextWriters.Default)
        )
        :> IWorkRepository
    
    let workEventRepository () =
        new WorkEventRepository(
            workDbOptions (),
            System.TimeProvider.System,
            TestLogger<WorkEventRepository>(TestContextWriters.Default)
        )
        :> IWorkEventRepository

    let private createWork () =
        task {
            let repo = workRepository ()
            return! repo.InsertAsync (generateNumber ()) (generateTitle ()) ct
        }

    [<OneTimeSetUp>]
    let Setup () =
        task {
            match! workRepository () :?> WorkRepository |> _.CreateTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))

            match! workEventRepository () :?> WorkEventRepository |> _.CreateTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))

            let! res = createWork ()
            match res with
            | Ok (id, _) ->
                workId1 <- id
            | Error err -> raise (InvalidOperationException(err))

            let! res = createWork ()
            match res with
            | Ok (id, _) ->
                workId2 <- id
            | Error err ->
                assertionExn err
        }

    [<OneTimeTearDown>]
    let TearDown () =
        task {
            let dataSource = dbFileName |> dataSource
            if File.Exists(dataSource) then
                File.Delete(dataSource)
        }

    
    [<Test>]
    let ``01: InsertAsync test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let! id1 = workEventRepo.InsertAsync workId1 (generateWorkEvent ()) ct
            let! id2 = workEventRepo.InsertAsync workId1 (generateWorkEvent ()) ct

            id1 |> shouldBeGreaterThan 0UL
            id2 |> shouldBeGreaterThan id1
        }
        |> TaskResult.runTest

    [<Test>]
    let ``02: FindByWorkIdAsync test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent = generateWorkEvent ()
            let! _ = workEventRepo.InsertAsync workId1 workEvent ct

            let! rows = workEventRepo.FindByWorkIdAsync workId1 ct

            rows |> shouldContain workEvent
        }
        |> TaskResult.runTest

    [<Test>]
    let ``03: FindByWorkIdByDateAsync test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent1 = generateWorkEvent ()
            let workEvent2 = generateWorkEvent ()
            let workEvent3 = generateWorkEvent ()
            let workEvent4 = generateWorkEvent ()
            let workEvent5 = generateWorkEvent ()

            let! _ = workEventRepo.InsertAsync workId1 workEvent1 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent2 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent3 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent4 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent5 ct

            let! rows = workEventRepo.FindByWorkIdByDateAsync workId1 (workEvent3 |> WorkEvent.dateOnly) ct

            rows |> shouldContain workEvent3
        }
        |> TaskResult.runTest

    [<Test>]
    let ``findByWorkIdByDate date without event test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent1 = generateWorkEvent ()
            let workEvent2 = generateWorkEvent ()
            let workEvent3 = generateWorkEvent ()
            let workEvent4 = generateWorkEvent ()
            let workEvent5 = generateWorkEvent ()

            let! _ = workEventRepo.InsertAsync workId1 workEvent1 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent2 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent3 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent4 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent5 ct

            let date = DateOnly.FromDateTime(System.TimeProvider.System.GetUtcNow().DateTime.AddDays(1))

            let! rows = workEventRepo.FindByWorkIdByDateAsync workId1 date ct

            rows |> should be Empty
        }
        |> TaskResult.runTest

    [<Test>]
    let ``findAllByPeriod period with events test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let work1Events =
                [
                    generateWorkEvent ()
                    generateWorkEvent ()
                    generateWorkEvent ()
                    generateWorkEvent ()
                    generateWorkEvent ()
                ]

            let work2Events =
                [
                    generateWorkEvent ()
                    generateWorkEvent ()
                    generateWorkEvent ()
                    generateWorkEvent ()
                    generateWorkEvent ()
                ]

            for i in 0 .. 4 do
                let! _ = workEventRepo.InsertAsync workId1 work1Events[i] ct
                ()

            for i in 0 .. 4 do
                let! _ = workEventRepo.InsertAsync workId2 work2Events[i] ct
                ()

            let minDate =
                DateOnly.FromDateTime(
                    (work1Events @ work2Events)
                    |> List.map WorkEvent.createdAt
                    |> List.min
                    |> fun dt -> dt.DateTime
                )

            let maxDate =
                DateOnly.FromDateTime(
                    (work1Events @ work2Events)
                    |> List.map WorkEvent.createdAt
                    |> List.max
                    |> fun dt -> dt.DateTime
                )

            let! rows = workEventRepo.FindAllByPeriodAsync ({ Start = minDate; EndInclusive = maxDate }) ct

            let list = rows |> Seq.toList
            list.Length |> should greaterThanOrEqualTo 2
            list[0].Events.Length |> should greaterThanOrEqualTo 5
            list[1].Events.Length |> should greaterThanOrEqualTo 5
        }
        |> TaskResult.runTest

