module PomodoroWindowsTimer.Storage.Tests.WorkRepositoryTests

open System
open System.IO
open System.Reflection
open Microsoft.Data.Sqlite

open NUnit.Framework
open FsUnitTyped.TopLevelOperators
open Bogus
open p1eXu5.FSharp.Testing.ShouldExtensions.Helpers

open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Storage.WorkRepository

let private dbFileName = "work_test.db"

let faker = Faker()

let mutable private connectionString = Unchecked.defaultof<string>

let getConnection () =
    if String.IsNullOrWhiteSpace(connectionString) then
        let dataSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dbFileName)
        if File.Exists(dataSource) then
            File.Delete(dataSource)
        connectionString <- $"Data Source=%s{dataSource};"
    new SqliteConnection(connectionString)

let generateNumber () : string option =
    let numFactory =
        [|
            fun () -> None
            fun () ->
                sprintf "WORK-%s" (faker.Random.Int(1, 9999).ToString("0000"))
                |> Some
        |]
    (faker.Random.ArrayElement(numFactory)) ()

let generateTitle () : string =
    faker.Commerce.ProductName()


[<OneTimeSetUp>]
let Setup () =
    task {
        let conn = getConnection ()
        do!
            Initializer.initdb conn
    }

[<Test>]
let ``create test`` () =
    task {
        let conn = getConnection ()
        let create =
            create System.TimeProvider.System (Helpers.execute conn)

        let! res1 = create (generateNumber ()) (generateTitle ())
        let! res2 = create (generateNumber ()) (generateTitle ())

        match res1, res2 with
        | Error err, _ -> failAssert err
        | _, Error err -> failAssert err
        | Ok id1, Ok id2 ->
            id1 |> shouldBeGreaterThan 0
            id2 |> shouldBeGreaterThan id1
    }

[<Test>]
let ``readAll test`` () =
    task {
        let conn = getConnection ()
        let create =
            create System.TimeProvider.System (Helpers.execute conn)

        let readAll =
            readAll (Helpers.select conn)

        let number = generateNumber ()
        let title = generateTitle ()

        let! _ = create number title
        let! res = readAll ()

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
            create System.TimeProvider.System (Helpers.execute conn)

        let find =
            find (Helpers.select conn)

        let number = generateNumber ()
        let title = generateTitle ()

        let! _ = create number title
        let! res = find title[..3]

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
            create System.TimeProvider.System (Helpers.execute conn)

        let update =
            update System.TimeProvider.System (Helpers.update conn)

        let find =
            find (Helpers.select conn)

        let findById =
            findById (Helpers.select conn)
        
        match! create (generateNumber ()) (generateTitle ()) with
        | Error err -> failAssert err
        | Ok id ->
            match! findById id with
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

                match! update updatedWork with
                | Error err -> failAssert err
                | Ok () ->
                    match! find title with
                    | Error err -> failAssert err
                    | Ok rows ->
                        rows |> Seq.map (fun r -> r.Number, r.Title) |> shouldContain (number, title)
    }


[<Test>]
let ``delete test`` () =
    task {
        let conn = getConnection ()
        let create =
            create System.TimeProvider.System (Helpers.execute conn)

        let delete =
            delete (Helpers.delete conn)

        let findById =
            findById (Helpers.select conn)

        match! create (generateNumber ()) (generateTitle ()) with
        | Error err -> failAssert err
        | Ok id ->
            match! delete id with
            | Error err -> failAssert err
            | Ok () ->
                match! findById id with
                | Error err -> failAssert err
                | Ok None -> ()
                | Ok (Some _) -> failAssert "Work has not been deleted"
    }

