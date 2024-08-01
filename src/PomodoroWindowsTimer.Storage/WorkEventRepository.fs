namespace PomodoroWindowsTimer.Storage

open System
open System.Threading
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Storage.Configuration

module internal WorkEventRepository =

    open System.Data.Common
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    
    open Dapper
    open IcedTasks
    open FsToolkit.ErrorHandling

    module Table = PomodoroWindowsTimer.Storage.Tables.WorkEvent
    module WorkTable = PomodoroWindowsTimer.Storage.Tables.Work
    module ActiveTimePointTable = PomodoroWindowsTimer.Storage.Tables.ActiveTimePoint

    module Sql =
        let CREATE_TABLE = $"""
            CREATE TABLE IF NOT EXISTS {Table.NAME} (
                  {Table.Columns.id} INTEGER PRIMARY KEY AUTOINCREMENT
                , {Table.Columns.work_id} INTEGER NOT NULL
                , {Table.Columns.event_json} TEXT NOT NULL
                , {Table.Columns.created_at} INTEGER NOT NULL
                , {Table.Columns.active_time_point_id} TEXT
                , {Table.Columns.event_name} TEXT NOT NULL

                , FOREIGN KEY ({Table.Columns.work_id})
                    REFERENCES {WorkTable.NAME} ({WorkTable.Columns.id})
                    ON DELETE CASCADE 
                    ON UPDATE NO ACTION

                , FOREIGN KEY ({Table.Columns.active_time_point_id})
                    REFERENCES {ActiveTimePointTable.NAME} ({ActiveTimePointTable.Columns.id})
                    ON DELETE SET NULL 
                    ON UPDATE NO ACTION
            );
            """

        /// Parameters: WorkId, EventJson, CreatedAt, @ActiveTimePointId, @EventName.
        let INSERT = $"""
            INSERT INTO {Table.NAME} ({Table.Columns.work_id}, {Table.Columns.event_json}, {Table.Columns.created_at}, {Table.Columns.active_time_point_id}, {Table.Columns.event_name})
            VALUES (@WorkId, @EventJson, @CreatedAt, @ActiveTimePointId, @EventName)
            ;

            SELECT {Table.Columns.id}
            FROM {Table.NAME}
            ORDER BY {Table.Columns.id} DESC
            LIMIT 1
            ;
            """

        /// Parameters: WorkId.
        let SELECT_BY_WORK_ID = $"""
            SELECT *
            FROM {Table.NAME}
            WHERE {Table.Columns.work_id} = @WorkId
            ORDER BY {Table.Columns.created_at} ASC
            """

        /// Parameters: WorkId, DateMin, DateMax.
        let SELECT_BY_WORK_ID_BY_PERIOD = $"""
            SELECT *
            FROM {Table.NAME}
            WHERE
                {Table.Columns.work_id} = @WorkId
                AND {Table.Columns.created_at} >= @DateMin
                AND {Table.Columns.created_at} < @DateMax
            ORDER BY {Table.Columns.created_at} ASC
            ;
            """

        /// Parameters: DateMin, DateMax.
        let SELECT_JOIN_WORK_BY_WORK_ID_BY_PERIOD = $"""
            SELECT e.*, '' AS split, w.*
            FROM {Table.NAME} e
                INNER JOIN {WorkTable.NAME} w ON w.{WorkTable.Columns.id} = e.{Table.Columns.work_id} 
            WHERE
                e.{Table.Columns.created_at} >= @DateMin
                AND e.{Table.Columns.created_at} < @DateMax
            ORDER BY
                  e.{Table.Columns.work_id}
                , e.{Table.Columns.created_at} ASC
            ;
            """

        /// Parameters: CreatedAtMax, AtpId, EventNames.
        let SELECT_JOIN_WORK_BY_ATP_ID_BY_MAX_DATE = $"""
            WITH first_event_created_at ({Table.Columns.created_at}) AS (
                SELECT e1.{Table.Columns.created_at}
                FROM {Table.NAME} e1
                WHERE e1.{Table.Columns.active_time_point_id} = @AtpId
                ORDER BY e1.{Table.Columns.created_at}
                LIMIT 1
            )
            SELECT e.*, '' AS split, w.*
            FROM {Table.NAME} e
                INNER JOIN {WorkTable.NAME} w ON w.{WorkTable.Columns.id} = e.{Table.Columns.work_id} 
            WHERE
                e.{Table.Columns.created_at} >= (SELECT {Table.Columns.created_at} FROM first_event_created_at LIMIT 1)
                AND e.{Table.Columns.created_at} <= @CreatedAtMax
                AND (e.{Table.Columns.active_time_point_id} = @AtpId OR e.{Table.Columns.active_time_point_id} IS NULL)
                AND e.{Table.Columns.event_name} IN @EventNames
            ORDER BY
                  e.{Table.Columns.work_id}
                , e.{Table.Columns.created_at} ASC
            ;
            """

    type Deps =
        {
            GetDbConnection: OpenDbConnection
            TimeProvider: System.TimeProvider
            Logger: ILogger
        }

    let createTableAsync deps =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.GetDbConnection
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

    let insertAsync deps workId workEvent =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.GetDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.INSERT,
                    parameters = {|
                        WorkId = workId
                        EventJson = JsonHelpers.Serialize(workEvent)
                        CreatedAt = WorkEvent.createdAt(workEvent).ToUnixTimeMilliseconds()
                        ActiveTimePointId = workEvent |> WorkEvent.activeTimePointId |> Option.map _.ToString() |> Option.defaultValue null
                        EventName = workEvent |> WorkEvent.name
                    |},
                    cancellationToken = ct
                )

            try
                return! dbConnection.ExecuteScalarAsync<uint64>(command)
            with ex ->
                deps.Logger.FailedToInsert(Table.NAME, ex)
                return! Error (ex.Format($"Failed to insert {Table.NAME}."))
        }

    let findByWorkIdAsync deps workId =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.GetDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.SELECT_BY_WORK_ID,
                    parameters = {| WorkId = workId |},
                    cancellationToken = ct
                )

            try
                let! rows = dbConnection.QueryAsync<Table.Row>(command)
                return rows |> Seq.map (fun r -> JsonHelpers.Deserialize<WorkEvent>(r.event_json)) |> Seq.toList
            with ex ->
                deps.Logger.FailedToFindWorkEventsByWorkId(workId, ex)
                return! Error (ex.Format($"Failed to find work events by workId {workId}."))
        }

    let findByWorkIdByDateAsync deps (workId: WorkId) (date: DateOnly) =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.GetDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let dateMin = new DateTimeOffset(date, new TimeOnly(0, 0, 0), deps.TimeProvider.LocalTimeZone.BaseUtcOffset)
            let dateMax = new DateTimeOffset(date.AddDays(1), new TimeOnly(0, 0, 0), deps.TimeProvider.LocalTimeZone.BaseUtcOffset)

            let command =
                CommandDefinition(
                    Sql.SELECT_BY_WORK_ID_BY_PERIOD,
                    parameters = {|
                        WorkId = workId
                        DateMin = dateMin.ToUnixTimeMilliseconds()
                        DateMax = dateMax.ToUnixTimeMilliseconds()
                    |},
                    cancellationToken = ct
                )

            try
                let! rows = dbConnection.QueryAsync<Table.Row>(command)
                return rows |> Seq.map (fun r -> JsonHelpers.Deserialize<WorkEvent>(r.event_json)) |> Seq.toList
            with ex ->
                deps.Logger.FailedToFindWorkEventsByWorkIdByDate(workId, date, ex)
                return! Error (ex.Format($"Failed to find work events by workId {workId} and date {date}."))
        }

    let findByWorkIdByPeriodAsync deps (workId: WorkId) (period: DateOnlyPeriod) =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.GetDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let dateMin = DateTimeOffset(period.Start, TimeOnly(0, 0, 0), deps.TimeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()
            let dateMax = DateTimeOffset(period.EndInclusive.AddDays(1), TimeOnly(0, 0, 0), deps.TimeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()

            let command =
                CommandDefinition(
                    Sql.SELECT_BY_WORK_ID_BY_PERIOD,
                    parameters = {|
                        WorkId = workId
                        DateMin = dateMin
                        DateMax = dateMax
                    |},
                    cancellationToken = ct
                )

            try
                let! rows = dbConnection.QueryAsync<Table.Row>(command)
                return rows |> Seq.map (fun r -> JsonHelpers.Deserialize<WorkEvent>(r.event_json)) |> Seq.toList
            with ex ->
                deps.Logger.FailedToFindWorkEventsByWorkIdByPeriod(workId, period, ex)
                return! Error (ex.Format($"Failed to find work events by workId {WorkId} and by period [{period.Start} - {period.EndInclusive}]."))
        }

    let toWorkEventLists (rows: System.Collections.Generic.IEnumerable<Table.Row * WorkTable.Row>) =
        rows
        |> Seq.map (fun (r, w) ->
            let work = w |> WorkTable.Row.ToWork
            let ev = JsonHelpers.Deserialize<WorkEvent>(r.event_json)
            (work, ev)
        )
        |> Seq.groupBy fst
        |> Seq.map (fun (w, evs) ->
            {
                Work = w
                Events = evs |> Seq.map snd |> Seq.toList
            }
        )
        |> Seq.toList

    let findAllByPeriodAsync deps (period: DateOnlyPeriod) =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.GetDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let dateMin = DateTimeOffset(period.Start, TimeOnly(0, 0, 0), deps.TimeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()
            let dateMax = DateTimeOffset(period.EndInclusive.AddDays(1), TimeOnly(0, 0, 0), deps.TimeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()

            let command =
                CommandDefinition(
                    Sql.SELECT_JOIN_WORK_BY_WORK_ID_BY_PERIOD,
                    parameters = {|
                        DateMin = dateMin
                        DateMax = dateMax
                    |},
                    cancellationToken = ct
                )

            try
                let! rows =
                    dbConnection.QueryAsync<Table.Row, WorkTable.Row, Table.Row * WorkTable.Row>(
                        command,
                        Func<Table.Row, WorkTable.Row, Table.Row * WorkTable.Row>(
                            fun f s ->
                                (f, s)
                        ),
                        "split"
                    )
                return
                    rows
                    |> toWorkEventLists
            with ex ->
                deps.Logger.FailedToFindWorkEventsByPeriod(period, ex)
                return! Error (ex.Format($"Failed to find work events by period [{period.Start} - {period.EndInclusive}]."))
        }

    let findByActiveTimePointIdByDateAsync deps (activeTimePointId: TimePointId) (notAfter: DateTimeOffset) =
        cancellableTaskResult {
            let! (dbConnection: DbConnection) = deps.GetDbConnection
            use _ = dbConnection

            let! ct = CancellableTask.getCancellationToken ()

            let command =
                CommandDefinition(
                    Sql.SELECT_JOIN_WORK_BY_ATP_ID_BY_MAX_DATE,
                    parameters = {|
                        CreatedAtMax = notAfter.ToUnixTimeMilliseconds()
                        AtpId = activeTimePointId.ToString()
                        EventNames = [| nameof WorkEvent.WorkStarted; nameof WorkEvent.BreakStarted; nameof WorkEvent.Stopped |]
                    |},
                    cancellationToken = ct
                )

            try
                let! rows =
                    dbConnection.QueryAsync<Table.Row, WorkTable.Row, Table.Row * WorkTable.Row>(
                        command,
                        Func<Table.Row, WorkTable.Row, Table.Row * WorkTable.Row>(
                            fun f s ->
                                (f, s)
                        ),
                        "split"
                    )
                return
                    rows
                    |> toWorkEventLists
            with ex ->
                deps.Logger.FailedToFindByActiveTimePointIdByDate(activeTimePointId, notAfter, ex)
                return! Error (ex.Format($"Failed to find work events by active time point id {activeTimePointId} and created not after {notAfter}."))
        }


type WorkEventRepository(options: IOptions<WorkDbOptions>, timeProvider: System.TimeProvider, logger: ILogger<WorkEventRepository>) =

    let getDbConnection = RepositoryBase.openDbConnection options logger
    let deps : WorkEventRepository.Deps =
        {
            GetDbConnection = getDbConnection
            TimeProvider = timeProvider
            Logger = logger
        }

    member _.CreateTableAsync(?cancellationToken) =
        let ct = defaultArg cancellationToken CancellationToken.None
        WorkEventRepository.createTableAsync deps ct

    interface IWorkEventRepository with
        member _.InsertAsync workId workEvent cancellationToken =
            WorkEventRepository.insertAsync deps workId workEvent cancellationToken

        member _.FindByWorkIdAsync workId cancellationToken =
            WorkEventRepository.findByWorkIdAsync deps workId cancellationToken

        member _.FindByWorkIdByDateAsync workId date cancellationToken =
            WorkEventRepository.findByWorkIdByDateAsync deps workId date cancellationToken

        member _.FindByWorkIdByPeriodAsync workId period cancellationToken =
            WorkEventRepository.findByWorkIdByPeriodAsync deps workId period cancellationToken

        member _.FindAllByPeriodAsync period cancellationToken =
            WorkEventRepository.findAllByPeriodAsync deps period cancellationToken

        member _.FindLastByWorkIdByDateAsync workId date cancellationToken =
            raise (NotImplementedException())

        member _.FindByActiveTimePointIdByDateAsync activeTimePointId notAfter cancellationToken =
            WorkEventRepository.findByActiveTimePointIdByDateAsync deps activeTimePointId notAfter cancellationToken

