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

let ct = CancellationToken.None

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
        connectionStrings[dbFileName] <- $"Data Source=%s{dataSource};"
        connectionStrings[dbFileName]


let internal workDbOptions dbFileName =
    { new IOptions<WorkDbOptions> with
        member _.Value : WorkDbOptions = 
            { ConnectionString=getConnectionString dbFileName }
    }

let internal workRepository dbFileName =
    new WorkRepository(
        workDbOptions dbFileName,
        System.TimeProvider.System,
        TestLogger<WorkRepository>(TestContextWriters.DefaultWith(TestContext.Progress, TestContext.Out))
    )
    :> IWorkRepository
    
let internal workEventRepository dbFileName =
    new WorkEventRepository(
        workDbOptions dbFileName,
        System.TimeProvider.System,
        TestLogger<WorkEventRepository>(TestContextWriters.DefaultWith(TestContext.Progress, TestContext.Out))
    )
    :> IWorkEventRepository

let internal activeTimePointRepository dbFileName =
    new ActiveTimePointRepository(
        workDbOptions dbFileName,
        System.TimeProvider.System,
        TestLogger<ActiveTimePointRepository>(TestContextWriters.Default)
    )
    :> IActiveTimePointRepository