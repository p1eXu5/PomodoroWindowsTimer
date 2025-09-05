namespace PomodoroWindowsTimer.Storage

open System
open System.Threading
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Storage.Configuration

module internal CommitRepository =

    open System.Data.Common
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    
    open Dapper
    open IcedTasks
    open FsToolkit.ErrorHandling

    module CommitTable = PomodoroWindowsTimer.Storage.Tables.Commit
    module CommitTagTable = PomodoroWindowsTimer.Storage.Tables.CommitTag

    module Sql =
        open System.Text

        let INSERT_COMMIT =
            $"""
            INSERT INTO {CommitTable.NAME} (
                  {CommitTable.Columns.message}
                , {CommitTable.Columns.work_id}
                , {CommitTable.Columns.created_at}
                , {CommitTable.Columns.updated_at})
            VALUES (@Message, @WorkId, @CreatedAt, @CreatedAt);

            SELECT {CommitTable.Columns.id}
            FROM {CommitTable.NAME}
            ORDER BY {CommitTable.Columns.id} DESC
            LIMIT 1
            ;
            """
            
        let INSERT_COMMIT_TAG =
            $"""
            INSERT INTO {CommitTagTable.NAME} (
                    {CommitTagTable.Columns.commit_id}
                , {CommitTagTable.Columns.tag_id}
                , {CommitTagTable.Columns.created_at})
            VALUES (@CommitId, @TagId, @CreatedAt)
            ;
            """

    type Deps =
        {
            OpenDbConnection: OpenDbConnection
            TimeProvider: System.TimeProvider
            Logger: ILogger
        }

    let insertAsync deps message workId (tags: Tag list) =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.OpenDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let createdAtMs = deps.TimeProvider.GetUtcNow().ToUnixTimeMilliseconds();

            let command =
                CommandDefinition(
                    Sql.INSERT_COMMIT,
                    parameters = {|
                        Message = message
                        WorkId = workId
                        CreatedAt = createdAtMs
                    |},
                    cancellationToken = ct
                )

            try
                let! createdCommitId = dbConnection.ExecuteScalarAsync<uint64>(command)

                if tags.Length = 0 then
                    return (createdCommitId, DateTimeOffset.FromUnixTimeMilliseconds(createdAtMs))
                else
                    let parameters : obj =
                        tags
                        |> List.map (fun tag ->
                            {|
                                CommitId = createdCommitId
                                TagId = tag.Id
                                CreatedAt = createdAtMs
                            |}
                        )
                        |> box

                    let command =
                        CommandDefinition(
                            Sql.INSERT_COMMIT_TAG,
                            parameters = parameters,
                            cancellationToken = ct
                        )

                    let! rowsAffected = dbConnection.ExecuteAsync(command)
                    deps.Logger.LogDebug("CommitTags: {RowsAffected} row(s) inserted", rowsAffected)
                    return (createdCommitId, DateTimeOffset.FromUnixTimeMilliseconds(createdAtMs))
            with ex ->
                deps.Logger.LogError(ex, "Failed to insert commit.")
                return! Error "Failed to insert commit."
        }


type internal CommitRepository(options: IDatabaseSettings, timeProvider: System.TimeProvider, logger: ILogger<CommitRepository>) =

    let getDbConnection = RepositoryBase.openDbConnection options logger
    let deps : CommitRepository.Deps =
        {
            OpenDbConnection = getDbConnection
            TimeProvider = timeProvider
            Logger = logger
        }

    interface ICommitRepository with
        member _.InsertAsync message workId tags cancellationToken =
            CommitRepository.insertAsync deps message workId tags cancellationToken


