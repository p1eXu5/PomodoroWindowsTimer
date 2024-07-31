namespace PomodoroWindowsTimer.Storage

open System
open System.Threading
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Storage.Configuration

module internal ActiveTimePointRepository =

    open System.Data.Common
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    
    open Dapper
    open IcedTasks
    open FsToolkit.ErrorHandling

    module Table = PomodoroWindowsTimer.Storage.Tables.ActiveTimePoint

    module Sql =
        let CREATE_TABLE = $"""
            CREATE TABLE IF NOT EXISTS {Table.NAME} (
                  {Table.Columns.id} TEXT PRIMARY KEY
                , {Table.Columns.original_id} TEXT
                , {Table.Columns.name} TEXT NOT NULL
                , {Table.Columns.time_span} TEXT NOT NULL
                , {Table.Columns.kind} TEXT NOT NULL
                , {Table.Columns.kind_alias} TEXT NOT NULL
                , {Table.Columns.created_at} INTEGER NOT NULL
            );
            """

        let INSERT = $"""
            INSERT INTO {Table.NAME} (
                  {Table.Columns.id}
                , {Table.Columns.original_id}
                , {Table.Columns.name}
                , {Table.Columns.time_span}
                , {Table.Columns.kind}
                , {Table.Columns.kind_alias}
                , {Table.Columns.created_at}
            )
            VALUES (@Id, @OriginalId, @Name, @TimeSpan, @Kind, @KindAlias, @CreatedAt)
            ;
            """

        let SELECT_ALL = $"""
            SELECT * FROM {Table.NAME}
            ORDER BY {Table.Columns.created_at}
            ;
            """

    type Deps =
        {
            OpenDbConnection: OpenDbConnection
            TimeProvider: System.TimeProvider
            Logger: ILogger
        }

    let createTableAsync deps =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.CREATE_TABLE,
                    cancellationToken = ct
                )

            try
                let! _ = dbConnection.ExecuteAsync(command)
                return ()
            with ex ->
                deps.Logger.FailedToCreateTable(Table.NAME, ex)
                return! Error (ex.Format($"Failed to create table {Table.NAME}."))
        }

    let insertAsync deps (activeTimePoint: ActiveTimePoint) =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.INSERT,
                    parameters = {|
                        Id = activeTimePoint.Id.ToString()
                        OriginalId = activeTimePoint.OriginalId.ToString()
                        Name = activeTimePoint.Name
                        TimeSpan = DateTime().Add(activeTimePoint.TimeSpan).ToString("yyyy-MM-dd HH:mm:ss")
                        Kind = activeTimePoint.Kind |> Kind.displayString
                        KindAlias = activeTimePoint.KindAlias |> Alias.value
                        CreatedAt = deps.TimeProvider.GetUtcNow().ToUnixTimeMilliseconds()
                    |},
                    cancellationToken = ct
                )

            try
                let! _ = dbConnection.ExecuteAsync(command)
                return ()
            with ex ->
                deps.Logger.FailedToInsert(Table.NAME, ex)
                return! Error (ex.Format($"Failed to insert {Table.NAME}."))
        }

    let readAllAsync deps =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.SELECT_ALL,
                    cancellationToken = ct
                )

            try
                let! rows = dbConnection.QueryAsync<Table.Row>(command)
                return rows |> Seq.map Table.Row.ToActiveTimePoint |> Seq.toList
            with ex ->
                deps.Logger.LogError(ex, "Failed to read db works.")
                return! Error (ex.Format("Failed to read db works."))
        }

type ActiveTimePointRepository(options: IOptions<WorkDbOptions>, timeProvider: System.TimeProvider, logger: ILogger<ActiveTimePointRepository>) =

    let getDbConnection = RepositoryBase.openDbConnection options logger
    let deps : ActiveTimePointRepository.Deps =
        {
            OpenDbConnection = getDbConnection
            TimeProvider = timeProvider
            Logger = logger
        }

    member _.CreateTableAsync(?cancellationToken) =
        let ct = defaultArg cancellationToken CancellationToken.None
        ActiveTimePointRepository.createTableAsync deps ct

    interface IActiveTimePointRepository with
        member _.InsertAsync activeTimePoint cancellationToken =
            ActiveTimePointRepository.insertAsync deps activeTimePoint cancellationToken

        member this.ReadAllAsync(cancellationToken) =
            ActiveTimePointRepository.readAllAsync deps cancellationToken

