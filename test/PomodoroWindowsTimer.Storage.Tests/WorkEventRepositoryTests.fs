namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO
open Microsoft.Data.Sqlite

open NUnit.Framework
open FsUnit
open FsUnitTyped.TopLevelOperators
open p1eXu5.FSharp.Testing.ShouldExtensions.Helpers

open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Testing.Fakers
open PomodoroWindowsTimer.Types


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

    let getConnection () =
        new SqliteConnection(getConnectionString())

    let private createWork () =
        task {
            use conn = getConnection ()
            let create =
                WorkRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            return! create (generateNumber ()) (generateTitle ()) ct
        }

    [<OneTimeSetUp>]
    let Setup () =
        task {
            do!
                Initializer.initdb (getConnectionString())
            let! res = createWork ()
            match res with
            | Ok (id, _) ->
                workId1 <- id
            | Error err ->
                assertionExn err

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
    let ``create test`` () =
        task {
            use conn = getConnection ()
            let create =
                WorkEventRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let! res1 = create workId1 (generateWorkEvent ()) ct
            let! res2 = create workId1 (generateWorkEvent ()) ct

            match res1, res2 with
            | Error err, _ -> failAssert err
            | _, Error err -> failAssert err
            | Ok (id1, _), Ok (id2, _) ->
                id1 |> shouldBeGreaterThan 0UL
                id2 |> shouldBeGreaterThan id1
        }

    [<Test>]
    let ``findByWorkId test`` () =
        task {
            use conn = getConnection ()
            let create =
                WorkEventRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let findByWorkId =
                WorkEventRepository.findByWorkIdTask (Helpers.selectTask conn)

            let workEvent = generateWorkEvent ()

            let! _ = create workId1 workEvent ct
            let! res = findByWorkId workId1 ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> shouldContain workEvent
        }

    [<Test>]
    let ``findByWorkIdByDate date with event test`` () =
        task {
            use conn = getConnection ()
            let create =
                WorkEventRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let findByWorkIdByDate =
                WorkEventRepository.findByWorkIdByDateTask System.TimeProvider.System (Helpers.selectTask conn)

            let workEvent1 = generateWorkEvent ()
            let workEvent2 = generateWorkEvent ()
            let workEvent3 = generateWorkEvent ()
            let workEvent4 = generateWorkEvent ()
            let workEvent5 = generateWorkEvent ()

            let! _ = create workId1 workEvent1 ct
            let! _ = create workId1 workEvent2 ct
            let! _ = create workId1 workEvent3 ct
            let! _ = create workId1 workEvent4 ct
            let! _ = create workId1 workEvent5 ct

            let! res = findByWorkIdByDate workId1 (workEvent3 |> WorkEvent.dateOnly) ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> shouldContain workEvent3
        }

    [<Test>]
    let ``findByWorkIdByDate date without event test`` () =
        task {
            use conn = getConnection ()
            let create =
                WorkEventRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let findByWorkIdByDate =
                WorkEventRepository.findByWorkIdByDateTask System.TimeProvider.System (Helpers.selectTask conn)

            let workEvent1 = generateWorkEvent ()
            let workEvent2 = generateWorkEvent ()
            let workEvent3 = generateWorkEvent ()
            let workEvent4 = generateWorkEvent ()
            let workEvent5 = generateWorkEvent ()

            let! _ = create workId1 workEvent1 ct
            let! _ = create workId1 workEvent2 ct
            let! _ = create workId1 workEvent3 ct
            let! _ = create workId1 workEvent4 ct
            let! _ = create workId1 workEvent5 ct

            let date = DateOnly.FromDateTime(System.TimeProvider.System.GetUtcNow().DateTime.AddDays(1))

            let! res = findByWorkIdByDate workId1 date ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> should be Empty
        }

    [<Test>]
    let ``findAllByPeriod period with events test`` () =
        task {
            use conn = getConnection ()
            let create =
                WorkEventRepository.createTask System.TimeProvider.System (Helpers.execute conn)

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
                let! _ = create workId1 work1Events[i] ct
                ()

            for i in 0 .. 4 do
                let! _ = create workId2 work2Events[i] ct
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

            let findAllByPeriod =
                WorkEventRepository.findAllByPeriodTask System.TimeProvider.System (Helpers.selectTask2 conn)

            let! res = findAllByPeriod ({ Start = minDate; EndInclusive = maxDate }) ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                let list = rows |> Seq.toList
                list.Length |> should greaterThanOrEqualTo 2
                list[0].Events.Length |> should greaterThanOrEqualTo 5
                list[1].Events.Length |> should greaterThanOrEqualTo 5
        }
