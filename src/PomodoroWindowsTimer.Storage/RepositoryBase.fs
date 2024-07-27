[<AutoOpen>]
module internal PomodoroWindowsTimer.Storage.RepositoryBase

open System.Threading
open Microsoft.Extensions.Options
open Microsoft.Extensions.Logging
open Microsoft.Data.Sqlite
open IcedTasks

open PomodoroWindowsTimer.Storage.Configuration
open System.Data
open System.Data.Common

type OpenDbConnection = CancellableTask<Result<DbConnection, string>>

let openDbConnection (options: WorkDbOptions) (logger: ILogger) : OpenDbConnection =
        cancellableTask {
            let dbConnection = new SqliteConnection(options.ConnectionString)
            try
                do! dbConnection.OpenAsync
                return dbConnection :> DbConnection |> Ok
            with ex ->
                do! dbConnection.DisposeAsync()
                logger.FailedToOpenConnection(ex)
                return Error (ex.Format("Failed to open db connectionn."))
        }

