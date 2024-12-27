namespace PomodoroWindowsTimer.Storage

open System.Threading
open Microsoft.Extensions.Logging

open PomodoroWindowsTimer.Abstractions


module internal RepositoryFactory =

    open System.Data.Common

    open Dapper
    open IcedTasks
    open FsToolkit.ErrorHandling


    module Sql =
        let SELECT_TABLES = """
            SELECT name FROM sqlite_master WHERE type='table';
            """

        let SELECT_COUNT tableName = $"""
            SELECT COUNT(*) FROM %s{tableName};
            """

    type Deps =
        {
            OpenDbConnection: OpenDbConnection
            Logger: ILogger
        }

    let readDbTablesAsync deps =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.SELECT_TABLES,
                    cancellationToken = ct
                )

            try
                let! rows = dbConnection.QueryAsync<string>(command)
                return rows |> Seq.toList
            with ex ->
                deps.Logger.LogError(ex, "Failed to read db tables.")
                return! Error (ex.Format("Failed to read db tables."))
        }

    let readRowCount deps tableName =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.SELECT_COUNT tableName,
                    cancellationToken = ct
                )

            try
                let! rowCount = dbConnection.ExecuteScalarAsync<int>(command)
                return rowCount
            with ex ->
                deps.Logger.LogError(ex, "Failed to read row count.")
                return! Error (ex.Format("Failed to read row count."))
        }


type internal RepositoryFactory(
    options: IDatabaseSettings,
    timeProvider: System.TimeProvider,
    loggerFactory: ILoggerFactory,
    logger: ILogger<RepositoryFactory>
) =
    let getDbConnection ct = RepositoryBase.openDbConnection options logger ct
    let deps : RepositoryFactory.Deps =
        {
            OpenDbConnection = getDbConnection
            Logger = logger
        }

    member _.Replicate(dbSettings: IDatabaseSettings) =
        RepositoryFactory(dbSettings, timeProvider, loggerFactory, logger)

    member _.ReadDbTablesAsync (?cancellationToken) =
        let ct = defaultArg cancellationToken CancellationToken.None
        RepositoryFactory.readDbTablesAsync deps ct

    member _.ReadRowCountAsync (tableName, ?cancellationToken) =
        let ct = defaultArg cancellationToken CancellationToken.None
        RepositoryFactory.readRowCount deps tableName ct


    interface IRepositoryFactory with
        member this.ReadDbTablesAsync (?cancellationToken) =
            let ct = defaultArg cancellationToken CancellationToken.None
            this.ReadDbTablesAsync(ct)

        member this.Replicate(dbSettings: IDatabaseSettings) =
            this.Replicate(dbSettings)

        member this.ReadRowCountAsync (tableName, ?cancellationToken) =
            let ct = defaultArg cancellationToken CancellationToken.None
            this.ReadRowCountAsync(tableName, ct)


        member _.GetWorkRepository () : IWorkRepository = 
            WorkRepository(options, timeProvider, loggerFactory.CreateLogger<WorkRepository>()) :> IWorkRepository

        member _.GetWorkEventRepository () : IWorkEventRepository = 
            WorkEventRepository(options, timeProvider, loggerFactory.CreateLogger<WorkEventRepository>()) :> IWorkEventRepository

        member _.GetActiveTimePointRepository () : IActiveTimePointRepository = 
            ActiveTimePointRepository(options, timeProvider, loggerFactory.CreateLogger<ActiveTimePointRepository>()) :> IActiveTimePointRepository
