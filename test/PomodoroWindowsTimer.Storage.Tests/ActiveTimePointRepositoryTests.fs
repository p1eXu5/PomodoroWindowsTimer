namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO

open NUnit.Framework
open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions
open FsUnitTyped
open FsToolkit.ErrorHandling
open Faqt
open Faqt.Operators

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Testing.Fakers


[<Category("DB. ActiveTimePoint")>]
module ActiveTimePointRepositoryTests =
    let private dbFileName = $"active_time_point_test_{Guid.NewGuid()}.db"

    let mutable _repositoryFactory = Unchecked.defaultof<IRepositoryFactory>

    let private workRepository () = _repositoryFactory.GetWorkRepository ()
    let private workEventRepository () = _repositoryFactory.GetWorkEventRepository ()
    let private activeTimePointRepository () = _repositoryFactory.GetActiveTimePointRepository ()

    [<OneTimeSetUp>]
    let Setup () =
        task {
            _repositoryFactory <- repositoryFactory dbFileName
            do! seedDataBase _repositoryFactory
            do applyMigrations dbFileName
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
            rows |> shouldContain (atp |> ActiveTimePoint.withNoRemainingTimeSpan)
        }
        |> TaskResult.runTest

    [<Test>]
    let ``03: InsertIfNotExistsAsync test`` () =
        taskResult {
            let atpRepo = activeTimePointRepository ()

            let atp = ActiveTimePoint.generate ()
            let! _ = atpRepo.InsertIfNotExistsAsync atp ct
            let! _ = atpRepo.InsertIfNotExistsAsync atp ct

            let! dbAtp = (atpRepo :?> ActiveTimePointRepository).FindByIdAsync atp.Id ct

            %dbAtp.IsSome.Should().BeTrue()
            %dbAtp.Value.Should().Be(atp |> ActiveTimePoint.withNoRemainingTimeSpan)

            return ()
        }
        |> TaskResult.runTest

