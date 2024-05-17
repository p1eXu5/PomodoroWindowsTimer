namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO
open Microsoft.Data.Sqlite

open NUnit.Framework
open FsUnitTyped.TopLevelOperators
open p1eXu5.FSharp.Testing.ShouldExtensions.Helpers

open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Testing.Fakers


[<Category("DB. Work")>]
module WorkRepositoryTests =

    let private dbFileName = "work_test.db"

    let mutable private connectionString = Unchecked.defaultof<string>

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

    [<OneTimeSetUp>]
    let Setup () =
        task {
            do!
                Initializer.initdb (getConnectionString())
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
                WorkRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let! res1 = create (generateNumber ()) (generateTitle ()) ct
            let! res2 = create (generateNumber ()) (generateTitle ()) ct

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
                WorkRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let readAll =
                WorkRepository.readAllTask (Helpers.selectTask conn)

            let number = generateNumber ()
            let title = generateTitle ()

            let! _ = create number title ct
            let! res = readAll ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> Seq.map (fun r -> r.Number, r.Title) |> shouldContain (number, title)
        }


    [<Test>]
    let ``find test`` () =
        task {
            let conn = getConnection ()
            let create =
                WorkRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let find =
                WorkRepository.findTask (Helpers.selectTask conn)

            let number = generateNumber ()
            let title = generateTitle ()

            let! _ = create number title ct
            let! res = find title[..3] ct

            match res with
            | Error err -> failAssert err
            | Ok rows ->
                rows |> Seq.map (fun r -> r.Number, r.Title) |> shouldContain (number, title)
        }


    [<Test>]
    let ``update test`` () =
        task {
            let conn = getConnection ()
            let create =
                WorkRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let update =
                WorkRepository.updateTask System.TimeProvider.System (Helpers.update conn)

            let find =
                WorkRepository.findTask (Helpers.selectTask conn)

            let findById =
                WorkRepository.findByIdTask (Helpers.selectTask conn)
        
            match! create (generateNumber ()) (generateTitle ()) ct with
            | Error err -> failAssert err
            | Ok (id, _) ->
                match! findById id ct with
                | Error err -> failAssert err
                | Ok work ->
                    let number = generateNumber ()
                    let title = generateTitle ()

                    let updatedWork =
                        {
                            work.Value with
                                Number = number
                                Title = title
                        }

                    match! update updatedWork ct with
                    | Error err -> failAssert err
                    | Ok _ ->
                        match! find title ct with
                        | Error err -> failAssert err
                        | Ok rows ->
                            rows |> Seq.map (fun r -> r.Number, r.Title) |> shouldContain (number, title)
        }


    [<Test>]
    let ``delete test`` () =
        task {
            let conn = getConnection ()
            let create =
                WorkRepository.createTask System.TimeProvider.System (Helpers.execute conn)

            let delete =
                WorkRepository.deleteTask (Helpers.delete conn)

            let findById =
                WorkRepository.findByIdTask (Helpers.selectTask conn)

            match! create (generateNumber ()) (generateTitle ()) ct with
            | Error err -> failAssert err
            | Ok (id, _) ->
                match! delete id ct with
                | Error err -> failAssert err
                | Ok () ->
                    match! findById id ct with
                    | Error err -> failAssert err
                    | Ok None -> ()
                    | Ok (Some _) -> failAssert "Work has not been deleted"
        }

