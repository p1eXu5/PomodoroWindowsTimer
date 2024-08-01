namespace PomodoroWindowsTimer.Storage

open System
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Storage.Configuration
open System.Threading

module internal WorkRepository =

    open System.Data.Common
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    
    open Dapper
    open IcedTasks
    open FsToolkit.ErrorHandling

    module Table = PomodoroWindowsTimer.Storage.Tables.Work
    module WorkEventTable = PomodoroWindowsTimer.Storage.Tables.WorkEvent

    module Sql =
        let CREATE_TABLE = $"""
            CREATE TABLE IF NOT EXISTS {Table.NAME} (
                      {Table.Columns.id} INTEGER PRIMARY KEY AUTOINCREMENT
                    , {Table.Columns.number} TEXT NOT NULL
                    , {Table.Columns.title} TEXT NOT NULL
                    , {Table.Columns.created_at} INTEGER NOT NULL
                    , {Table.Columns.updated_at} INTEGER NOT NULL

                    , CONSTRAINT title_number_uct UNIQUE({Table.Columns.number}, {Table.Columns.title}) ON CONFLICT FAIL
                );
            """

        /// Parameters: WorkNumber, WorkTitle, WorkCreatedAt
        let INSERT = $"""
            INSERT INTO {Table.NAME} ({Table.Columns.number}, {Table.Columns.title}, {Table.Columns.created_at}, {Table.Columns.updated_at})
            VALUES (@WorkNumber, @WorkTitle, @WorkCreatedAt, @WorkCreatedAt);

            SELECT {Table.Columns.id}
            FROM {Table.NAME}
            ORDER BY {Table.Columns.id} DESC
            LIMIT 1
            ;
            """

        let SELECT_CLAUSE = $"""
            WITH
                work_last_event({WorkEventTable.Columns.work_id}, {Table.Columns.last_event_created_at}) AS (
                    SELECT
                          {WorkEventTable.Columns.work_id}
                        , MAX({WorkEventTable.Columns.created_at}) AS {Table.Columns.last_event_created_at}
                    FROM {WorkEventTable.NAME}
                    GROUP BY work_id
                )
            SELECT
                  w.*
                , e.{Table.Columns.last_event_created_at}
            FROM {Table.NAME} AS w
            LEFT JOIN work_last_event AS e ON e.{WorkEventTable.Columns.work_id} = w.{Table.Columns.id}
            """

        let ORDER_CLAUSE = $"""
            ORDER BY {Table.Columns.last_event_created_at} DESC
            """

        let SELECT_ALL = $"""
            {SELECT_CLAUSE}
            {ORDER_CLAUSE}
            ;
            """

        /// Parameters: Text
        let SELECT_BY_TEXT = $"""
            {SELECT_CLAUSE}
            WHERE w.{Table.Columns.number} LIKE @Text
                OR w.{Table.Columns.title} LIKE @Text
            {ORDER_CLAUSE}
            ;
            """

        /// Parameters: WorkId
        let SELECT_BY_WORK_ID = $"""
            {SELECT_CLAUSE}
            WHERE w.{Table.Columns.id} = @WorkId
            {ORDER_CLAUSE}
            ;
            """

        /// Parameters: Number, Title, NewUpdatedAt, WorkId, OldUpdatedAt
        let UPDATE = $"""
            UPDATE {Table.NAME}
            SET
                  {Table.Columns.number} = @Number
                , {Table.Columns.title} = @Title
                , {Table.Columns.updated_at} = @NewUpdatedAt
            WHERE {Table.Columns.id} = @WorkId AND {Table.Columns.updated_at} = @OldUpdatedAt
            """

        /// Parameters: WorkId, UpdatedAt
        let DELETE = $"""
            DELETE FROM {Table.NAME}
            WHERE {Table.Columns.id} = @WorkId AND {Table.Columns.updated_at} = @UpdatedAt
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

    let insertAsync deps number title =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let createdAtMs = deps.TimeProvider.GetUtcNow().ToUnixTimeMilliseconds();

            let command =
                CommandDefinition(
                    Sql.INSERT,
                    parameters = {|
                        WorkNumber = number
                        WorkTitle = title
                        WorkCreatedAt = createdAtMs
                    |},
                    cancellationToken = ct
                )

            try
                let! createdWorkId = dbConnection.ExecuteScalarAsync<uint64>(command)
                return (createdWorkId, DateTimeOffset.FromUnixTimeMilliseconds(createdAtMs))
            with ex ->
                deps.Logger.FailedToCreateTable(Table.NAME, ex)
                return! Error (ex.Format($"Failed to create table {Table.NAME}."))
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
                return rows |> Seq.map Table.Row.ToWork |> Seq.toList
            with ex ->
                deps.Logger.LogError(ex, "Failed to read db works.")
                return! Error (ex.Format("Failed to read db works."))
        }

    let searchByNumberOrTitleAsync deps text =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.SELECT_BY_TEXT,
                    parameters = {| Text = $"%%{text}%%" |},
                    cancellationToken = ct
                )

            try
                let! rows = dbConnection.QueryAsync<Table.Row>(command)
                return rows |> Seq.map Table.Row.ToWork |> Seq.toList
            with ex ->
                deps.Logger.LogError(ex, "Failed to read db works.")
                return! Error (ex.Format("Failed to read db works."))
        }

    let findByIdAsync deps (workId: WorkId) =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.SELECT_BY_WORK_ID,
                    parameters = {| WorkId = workId |},
                    cancellationToken = ct
                )

            try
                let! dbWork = dbConnection.QueryFirstOrDefaultAsync<Table.Row>(command)

                match box dbWork with
                | null -> return None
                | _ -> return dbWork |> Table.Row.ToWork |> Some
            with ex ->
                deps.Logger.LogError(ex, "Failed to obtain db work by id {WorkId}.", workId)
                return! Error (ex.Format($"Failed to obtain db work by id {workId}."))
        }

    let findByIdOrCreateAsync deps (work: Work) =
        cancellableTaskResult {
            match! findByIdAsync deps work.Id with
            | Some workFromDb -> return workFromDb
            | None ->
                let! (newId, createdAt) = insertAsync deps work.Number work.Title
                return
                    { work with
                        Id = newId
                        CreatedAt = createdAt
                        UpdatedAt = createdAt
                        LastEventCreatedAt = None
                    }
        }

    let updateAsync deps work =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let updatedAt = deps.TimeProvider.GetUtcNow()
            let updatedAtMs = updatedAt.ToUnixTimeMilliseconds()

            let command =
                CommandDefinition(
                    Sql.UPDATE,
                    parameters = {|
                        Number = work.Number
                        Title = work.Title
                        NewUpdatedAt = updatedAtMs
                        WorkId = work.Id
                        OldUpdatedAt = work.UpdatedAt.ToUnixTimeMilliseconds()
                    |},
                    cancellationToken = ct
                )

            try
                let! _ = dbConnection.ExecuteAsync(command)
                return updatedAt
            with ex ->
                deps.Logger.FailedToUpdateWork(work, ex)
                return! Error (ex.Format($"Failed to update work {work.Id}."))
        }

    let deleteAsync deps work =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.DELETE,
                    parameters = {|
                        WorkId = work.Id
                        UpdatedAt = work.UpdatedAt.ToUnixTimeMilliseconds()
                    |},
                    cancellationToken = ct
                )

            try
                let! _ = dbConnection.ExecuteAsync(command)
                return ()
            with ex ->
                deps.Logger.LogError(ex, "Failed to delete work.")
                return! Error (ex.Format($"Failed to delete work {work.Id}."))
        }

type WorkRepository(options: IOptions<WorkDbOptions>, timeProvider: System.TimeProvider, logger: ILogger<WorkRepository>) =

    let getDbConnection = RepositoryBase.openDbConnection options logger
    let deps : WorkRepository.Deps =
        {
            OpenDbConnection = getDbConnection
            TimeProvider = timeProvider
            Logger = logger
        }

    member _.CreateTableAsync(?cancellationToken) =
        let ct = defaultArg cancellationToken CancellationToken.None
        WorkRepository.createTableAsync deps ct

    interface IWorkRepository with
        member _.InsertAsync number title cancellationToken =
            WorkRepository.insertAsync deps number title cancellationToken

        member _.ReadAllAsync cancellationToken = 
            WorkRepository.readAllAsync deps cancellationToken

        member _.SearchByNumberOrTitleAsync text cancellationToken = 
            WorkRepository.searchByNumberOrTitleAsync deps text cancellationToken

        member _.FindByIdAsync workId cancellationToken = 
            WorkRepository.findByIdAsync deps workId cancellationToken

        member _.FindByIdOrCreateAsync work cancellationToken = 
            WorkRepository.findByIdOrCreateAsync deps work cancellationToken

        member _.UpdateAsync work cancellationToken = 
            WorkRepository.updateAsync deps work cancellationToken

        member _.DeleteAsync work cancellationToken = 
            WorkRepository.deleteAsync deps work cancellationToken




