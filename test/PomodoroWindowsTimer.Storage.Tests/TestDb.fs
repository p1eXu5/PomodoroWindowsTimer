[<AutoOpen>]
module PomodoroWindowsTimer.Storage.Tests.TestDb

open System.Reflection
open System.IO
open System.Threading

let ct = CancellationToken.None

let internal dataSource dbFileName =
    Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        dbFileName
    )
