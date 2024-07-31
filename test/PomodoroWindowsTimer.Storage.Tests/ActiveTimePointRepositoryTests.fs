namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO

open NUnit.Framework
open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions
open FsUnitTyped
open FsToolkit.ErrorHandling

open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Testing.Fakers
open PomodoroWindowsTimer.Types


[<Category("DB. ActiveTimePoint")>]
module ActiveTimePointRepositoryTests =
    let private dbFileName = "active_time_point_test.db"

    let private workRepository () = TestDb.workRepository dbFileName
    let private workEventRepository () = TestDb.workEventRepository dbFileName
    let private activeTimePointRepository () = TestDb.activeTimePointRepository dbFileName

    [<OneTimeSetUp>]
    let Setup () =
        task {
            match! workRepository () :?> WorkRepository |> _.CreateTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))

            match! activeTimePointRepository () :?> ActiveTimePointRepository |> _.CreateTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))

            match! workEventRepository () :?> WorkEventRepository |> _.CreateTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
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
            let atpRepo = activeTimePointRepository ()

            let! _ = atpRepo.InsertAsync (ActiveTimePoint.generate ()) ct
            let! _ = atpRepo.InsertAsync (ActiveTimePoint.generate ()) ct

            return ()
        }
        |> TaskResult.runTest

    [<Test>]
    let ``02: ReadAllAsync test`` () =
        taskResult {
            let atpRepo = activeTimePointRepository ()

            let atp = ActiveTimePoint.generate ()
            let! _ = atpRepo.InsertAsync atp ct

            // action
            let! rows = atpRepo.ReadAllAsync(ct)

            // assert
            rows |> shouldContain ({ atp with RemainingTimeSpan = TimeSpan.Zero })
        }
        |> TaskResult.runTest