module PomodoroWindowsTimer.Storage.Tests.DbFileRevisorTests

open System
open System.IO
open System.Threading
open System.Threading.Tasks

open NUnit.Framework
open NSubstitute
open Faqt
open Faqt.Operators
open p1eXu5.AspNetCore.Testing
open p1eXu5.AspNetCore.Testing.Logging

open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Storage.Migrations
open FsToolkit.ErrorHandling
open p1eXu5.FSharp.Testing.ShouldExtensions

let mutable private _dbSettings = Unchecked.defaultof<IDatabaseSettings>
let mutable private _repositoryFactory = Unchecked.defaultof<IRepositoryFactory>

let private dbFileRevisor () =
    DbFileRevisor.init
        (DbSeeder(_repositoryFactory, TestLogger<DbSeeder>(TestContextWriters.GetInstance<TestContext>())))
        (DbMigrator(TestLogger<DbUp.Engine.UpgradeEngine>(TestContextWriters.GetInstance<TestContext>()), TestLogger<DbMigrator>(TestContextWriters.GetInstance<TestContext>())))

[<OneTimeSetUp>]
let Setup () =
    task {
        _dbSettings <- DatabaseSettingsExtensions.Create($"revisor_test_{Guid.NewGuid()}.db", false)
        _repositoryFactory <- repositoryFactory _dbSettings
    }

[<OneTimeTearDown>]
let TearDown () =
    task {
        let dataSource = _dbSettings.DatabaseFilePath
        if File.Exists(dataSource) then
            File.Delete(dataSource)
    }


[<Test>]
let ``01: TryUpdateDatabaseFile -> file does not exist -> returns ok``() =
    task {
        // Arrange
        let fileRevisor = dbFileRevisor ()

        // Act
        let! result = fileRevisor.TryUpdateDatabaseFileAsync _dbSettings CancellationToken.None

        // Assert
        %result.Should().BeOfCase(Result.Ok)
    }

[<Test>]
let ``02: TryUpdateDatabaseFile -> file does not exist -> applies migrations``() =
    taskResult {
        // Arrange
        let fileRevisor = dbFileRevisor ()

        // Act
        let! result = fileRevisor.TryUpdateDatabaseFileAsync _dbSettings CancellationToken.None

        // Assert
        let! tables = _repositoryFactory.ReadDbTablesAsync()
        %tables.Should().Contain("SchemaVersions")

        let! migrationCount = _repositoryFactory.ReadRowCountAsync("SchemaVersions")
        %migrationCount.Should().BeGreaterThan(0)
    }
    |> TaskResult.runTest

[<Test>]
let ``03: TryUpdateDatabaseFile -> file is empty -> returns ok``() =
    task {
        // Arrange
        let stream = File.Create(_dbSettings.DatabaseFilePath)
        stream.Dispose()

        let fileRevisor = dbFileRevisor ()

        // Act
        let! result = fileRevisor.TryUpdateDatabaseFileAsync _dbSettings CancellationToken.None

        // Assert
        %result.Should().BeOfCase(Result.Ok)
    }

[<Test>]
let ``04: TryUpdateDatabaseFile -> file is empty -> applies migrations``() =
    taskResult {
        // Arrange
        let stream = File.Create(_dbSettings.DatabaseFilePath)
        stream.Dispose()

        let fileRevisor = dbFileRevisor ()

        // Act
        do! fileRevisor.TryUpdateDatabaseFileAsync _dbSettings CancellationToken.None

        // Assert
        let! tables = _repositoryFactory.ReadDbTablesAsync()
        %tables.Should().Contain("SchemaVersions")

        let! migrationCount = _repositoryFactory.ReadRowCountAsync("SchemaVersions")
        %migrationCount.Should().BeGreaterThan(0)
    }
    |> TaskResult.runTest

[<Test>]
let ``05: TryUpdateDatabaseFile -> file with seeded db -> returns ok``() =
    task {
        let stream = File.Create(_dbSettings.DatabaseFilePath)
        stream.Dispose()

        do! seedDataBase(_repositoryFactory)

        let fileRevisor = dbFileRevisor ()

        // Act
        let! result = fileRevisor.TryUpdateDatabaseFileAsync _dbSettings CancellationToken.None

        // Assert
        %result.Should().BeOfCase(Result.Ok)
    }

[<Test>]
let ``06: TryUpdateDatabaseFile -> file with seeded db -> applies migrations``() =
    taskResult {
        let stream = File.Create(_dbSettings.DatabaseFilePath)
        stream.Dispose()

        do! seedDataBase(_repositoryFactory)

        let fileRevisor = dbFileRevisor ()

        // Act
        do! fileRevisor.TryUpdateDatabaseFileAsync _dbSettings CancellationToken.None

        // Assert
        let! tables = _repositoryFactory.ReadDbTablesAsync()
        %tables.Should().Contain("SchemaVersions")

        let! migrationCount = _repositoryFactory.ReadRowCountAsync("SchemaVersions")
        %migrationCount.Should().BeGreaterThan(0)
    }
    |> TaskResult.runTest


[<Test>]
let ``07: TryUpdateDatabaseFile -> file is not empty and not db -> returns error``() =
    task {
        let stream = File.Create(_dbSettings.DatabaseFilePath)
        let streamWriter = new StreamWriter(stream)
        streamWriter.WriteLine("foo_bar")
        streamWriter.Flush()
        streamWriter.Close()
        streamWriter.Dispose()
        stream.Dispose()

        let fileRevisor = dbFileRevisor ()

        // Act
        let! result = fileRevisor.TryUpdateDatabaseFileAsync _dbSettings CancellationToken.None

        // Assert
        %result.Should().BeOfCase(Result.Error)
        TestContext.WriteLine($"Result: %A{result}")
    }
