[<AutoOpen>]
module internal PomodoroWindowsTimer.Storage.RepositoryBase

open System.Threading
open Microsoft.Extensions.Options
open Microsoft.Extensions.Logging
open Microsoft.Data.Sqlite
open IcedTasks

open System.Data
open System.Data.Common
open PomodoroWindowsTimer.Abstractions

type OpenDbConnection = CancellableTask<Result<DbConnection, string>>

let openDbConnection (options: IDatabaseSettings) (logger: ILogger) : OpenDbConnection =
        cancellableTask {
            let dbConnection = new SqliteConnection(options.GetConnectionString())
            try
                do! dbConnection.OpenAsync
                return dbConnection :> DbConnection |> Ok
            with ex ->
                do! dbConnection.DisposeAsync()
                logger.FailedToOpenConnection(ex)
                return Error (ex.Format("Failed to open db connectionn."))
        }

