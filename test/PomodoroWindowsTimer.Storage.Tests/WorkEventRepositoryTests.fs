namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO
open Microsoft.Data.Sqlite

open NUnit.Framework
open FsUnitTyped.TopLevelOperators
open p1eXu5.FSharp.Testing.ShouldExtensions.Helpers

open PomodoroWindowsTimer.Storage


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
                WorkRepository.create System.TimeProvider.System (Helpers.execute conn)

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
                WorkEventRepository.create System.TimeProvider.System (Helpers.execute conn)

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
                WorkEventRepository.create System.TimeProvider.System (Helpers.execute conn)

            let readAll =
                WorkEventRepository.readAll (Helpers.select conn)

            let workEvent = generateWorkEvent ()

            let! _ = create workId workEvent ct
            let! res = readAll ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> shouldContain workEvent
        }
