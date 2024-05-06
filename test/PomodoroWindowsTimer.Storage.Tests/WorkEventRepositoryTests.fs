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
    let mutable private workId = Unchecked.defaultof<uint64>

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
                workId <- id
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

            let! res1 = create workId (generateWorkEvent ()) ct
            let! res2 = create workId (generateWorkEvent ()) ct

            match res1, res2 with
            | Error err, _ -> failAssert err
            | _, Error err -> failAssert err
            | Ok (id1, _), Ok (id2, _) ->
                id1 |> shouldBeGreaterThan 0UL
                id2 |> shouldBeGreaterThan id1
        }

    [<Test>]
    let ``readAll test`` () =
        task {
            use conn = getConnection ()
            let create =
                WorkEventRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let readAll =
                WorkEventRepository.findByWorkIdTask (Helpers.selectTask conn)

            let workEvent = generateWorkEvent ()

            let! _ = create workId workEvent ct
            let! res = readAll workId ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> shouldContain workEvent
        }

    [<Test>]
    let ``findByDate date with event test`` () =
        task {
            use conn = getConnection ()
            let create =
                WorkEventRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let readAll =
                WorkEventRepository.findByWorkIdTask (Helpers.selectTask conn)

            let findByDate =
                WorkEventRepository.findByWorkIdByDateTask System.TimeProvider.System (Helpers.selectTask conn)

            let workEvent1 = generateWorkEvent ()
            let workEvent2 = generateWorkEvent ()
            let workEvent3 = generateWorkEvent ()
            let workEvent4 = generateWorkEvent ()
            let workEvent5 = generateWorkEvent ()

            let! _ = create workId workEvent1 ct
            let! _ = create workId workEvent2 ct
            let! _ = create workId workEvent3 ct
            let! _ = create workId workEvent4 ct
            let! _ = create workId workEvent5 ct

            let! res = findByDate workId (workEvent3 |> WorkEvent.dateOnly) ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> shouldContain workEvent3
        }

    [<Test>]
    let ``findByDate date without event test`` () =
        task {
            use conn = getConnection ()
            let create =
                WorkEventRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let findByDate =
                WorkEventRepository.findByWorkIdByDateTask System.TimeProvider.System (Helpers.selectTask conn)

            let workEvent1 = generateWorkEvent ()
            let workEvent2 = generateWorkEvent ()
            let workEvent3 = generateWorkEvent ()
            let workEvent4 = generateWorkEvent ()
            let workEvent5 = generateWorkEvent ()

            let! _ = create workId workEvent1 ct
            let! _ = create workId workEvent2 ct
            let! _ = create workId workEvent3 ct
            let! _ = create workId workEvent4 ct
            let! _ = create workId workEvent5 ct

            let date = DateOnly.FromDateTime(System.TimeProvider.System.GetUtcNow().DateTime.AddDays(1))

            let! res = findByDate workId date ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> should be Empty
        }
