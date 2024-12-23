[<AutoOpen>]
module PomodoroWindowsTimer.Storage.Tests.TestDb

open System.Reflection
open System.IO
open System.Threading
open System.Collections.Concurrent

open Microsoft.Extensions.Options

open NUnit.Framework
open FsUnit.TopLevelOperators
open FsUnitTyped.TopLevelOperators
open p1eXu5.AspNetCore.Testing.Logging
open p1eXu5.FSharp.Testing.ShouldExtensions
open FsToolkit.ErrorHandling

open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Storage.Configuration
open PomodoroWindowsTimer.Testing.Fakers
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Storage.Migrations
open System

let ct = CancellationToken.None

let tcw = TestContextWriters.Default

let internal dataSource dbFileName =
    Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        dbFileName
    )

let private connectionStrings = ConcurrentDictionary<string, string>()

let internal getConnectionString dbFileName =
    match connectionStrings.TryGetValue(dbFileName) with
    | true, connStr -> connStr
    | false, _ ->
        let dataSource = dbFileName |> dataSource
        if File.Exists(dataSource) then
            File.Delete(dataSource)
        connectionStrings[dbFileName] <- $"Data Source=%s{dataSource};Pooling=false"
        connectionStrings[dbFileName]


let internal databaseSettings dbFileName =
    { new IDatabaseSettings with
        member _.DatabaseFilePath
            with get() = getConnectionString dbFileName
            and set _ = ()
    }

let internal workRepository dbFileName =
    new WorkRepository(
        databaseSettings dbFileName,
        System.TimeProvider.System,
        TestLogger<WorkRepository>(tcw)
    )
    :> IWorkRepository
    
let internal workEventRepository dbFileName =
    new WorkEventRepository(
        databaseSettings dbFileName,
        System.TimeProvider.System,
        TestLogger<WorkEventRepository>(tcw)
    )
    :> IWorkEventRepository

let internal activeTimePointRepository dbFileName =
    new ActiveTimePointRepository(
        databaseSettings dbFileName,
        System.TimeProvider.System,
        TestLogger<ActiveTimePointRepository>(tcw)
    )
    :> IActiveTimePointRepository

let internal repositoryFactory dbFileName =
    let options = databaseSettings dbFileName
    RepositoryFactory(
        options,
        System.TimeProvider.System,
        TestLoggerFactory.CreateWith(TestContext.Progress, TestContext.Out),
        TestLogger<RepositoryFactory>(tcw)
    )

let internal seedDataBase (repositoryFactory: IRepositoryFactory) =
    task {
        let seeder = DbSeeder(repositoryFactory, TestLogger<DbSeeder>(TestContextWriters.Default))
        match! seeder.SeedDatabaseAsync(CancellationToken.None) with
        | Ok _ -> return ()
        | Error err ->
            raise (InvalidOperationException(err))
    }

let internal applyMigrations dbFileName =
    let migrator = DbMigrator(
        TestLogger<DbUp.Engine.UpgradeEngine>(tcw),
        TestLogger<DbMigrator>(tcw)
    )

    match migrator.ApplyMigrations (getConnectionString dbFileName) with
    | Ok () -> ()
    | Error err ->
        raise (InvalidOperationException(err))


SqlMapper.LastEventCreatedAtHandler.Register()