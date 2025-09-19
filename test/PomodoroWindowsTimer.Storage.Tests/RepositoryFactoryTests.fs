namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO

open NUnit.Framework
open Faqt
open Faqt.Operators
open FsToolkit.ErrorHandling
open p1eXu5.FSharp.Testing.ShouldExtensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Testing.Fakers

[<Category("DB.")>]
module RepositoryFactoryTests =

    let mutable private _dbSettings = Unchecked.defaultof<IDatabaseSettings>

    let private repositoryFactory () =
        repositoryFactory _dbSettings

    [<SetUp>]
    let SetUp () =
        _dbSettings <- DatabaseSettingsExtensions.Create($"repository_factory_test_{Guid.NewGuid()}.db", false)

    [<TearDown>]
    let TearDown () =
        let dataSource = _dbSettings.DatabaseFilePath
        if File.Exists(dataSource) then
            File.Delete(dataSource)
        

    [<Test>]
    let ``01: ReadDbTablesAsync -> db file is empty -> returns empty`` () =
        taskResult {
            let repositoryFactory = repositoryFactory ()

            let! tables = repositoryFactory.ReadDbTablesAsync()

            %tables.Should().BeEmpty()
        }
        |> TaskResult.runTest

    [<Test>]
    let ``02: ReadDbTablesAsync -> migations are not applied -> returns work and work_event tables`` () =
        taskResult {
            let repositoryFactory = repositoryFactory ()
            do! seedDataBase repositoryFactory

            let! tables = repositoryFactory.ReadDbTablesAsync()

            %tables.Should().HaveSameItemsAs([| "work"; "work_event"; "sqlite_sequence" |])
        }
        |> TaskResult.runTest


