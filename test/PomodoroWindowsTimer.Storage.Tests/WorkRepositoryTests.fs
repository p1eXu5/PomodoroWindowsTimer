module PomodoroWindowsTimer.Storage.Tests.WorkRepositoryTests

open System
open System.Threading
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

let ct = CancellationToken.None

let mutable private connectionString = Unchecked.defaultof<string>

let getConnectionString () =
    if String.IsNullOrWhiteSpace(connectionString) then
        let dataSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dbFileName)
        if File.Exists(dataSource) then
            File.Delete(dataSource)
        connectionString <- $"Data Source=%s{dataSource};"
        connectionString
    else
        connectionString

let getConnection () =
    new SqliteConnection(getConnectionString())

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
        do!
            Initializer.initdb (getConnectionString())
    }

[<Test>]
let ``create test`` () =
    task {
        let conn = getConnection ()
        let create =
            create System.TimeProvider.System (Helpers.execute conn)

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
        let conn = getConnection ()
        let create =
            create System.TimeProvider.System (Helpers.execute conn)

        let readAll =
            readAll (Helpers.select conn)

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
            create System.TimeProvider.System (Helpers.execute conn)

        let find =
            find (Helpers.select conn)

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
            create System.TimeProvider.System (Helpers.execute conn)

        let update =
            update System.TimeProvider.System (Helpers.update conn)

        let find =
            find (Helpers.select conn)

        let findById =
            findById (Helpers.select conn)
        
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
            create System.TimeProvider.System (Helpers.execute conn)

        let delete =
            delete (Helpers.delete conn)

        let findById =
            findById (Helpers.select conn)

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

