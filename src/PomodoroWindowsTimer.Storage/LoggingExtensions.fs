namespace PomodoroWindowsTimer.Storage

open System
open Microsoft.Extensions.Logging
open System.Runtime.CompilerServices
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer

// -----------------------
// 2 - FailedToCreateTable
// -----------------------
[<Extension>]
type FailedToCreateTable() =
    static let failedToCreateTable = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(2, nameof FailedToCreateTable),
        "Failed to create table `{TableName}`.")

    [<Extension>]
    static member FailedToCreateTable(logger: ILogger, tableName: string, ex: Exception) =
        failedToCreateTable.Invoke(logger, tableName, ex)

// -----------------------
// 100 - TableCreated
// -----------------------
[<Extension>]
type TableCreated() =
    static let tableCreated = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(100, nameof TableCreated),
        "Table `{TableName}` has been created.")

    [<Extension>]
    static member TableCreated(logger: ILogger, tableName: string) =
        tableCreated.Invoke(logger, tableName, null)


[<Extension>]
type LoggingExtensions () =

    static let failedToOpenConnection = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, nameof LoggingExtensions.FailedToOpenConnection),
        "Failed to open db connection.")

    static let failedToInsert = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(2, nameof LoggingExtensions.FailedToInsert),
        "Failed to insert `{TableName}`.")

    static let failedToUpdateWork = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(3, nameof LoggingExtensions.FailedToUpdateWork),
        "Failed to update work: {Work}.")

    static let failedToFindWorkEventsByWorkId = LoggerMessage.Define<uint64>(
        LogLevel.Error,
        new EventId(4, nameof LoggingExtensions.FailedToFindWorkEventsByWorkId),
        "Failed to find work events by workId {WorkId}.")

    static let failedToFindWorkEventsByWorkIdByDate = LoggerMessage.Define<uint64, DateOnly>(
        LogLevel.Error,
        new EventId(5, nameof LoggingExtensions.FailedToFindWorkEventsByWorkIdByDate),
        "Failed to find work events by workId {WorkId} and by date {Date}.")

    static let failedToFindWorkEventsByWorkIdByPeriod = LoggerMessage.Define<uint64, DateOnly, DateOnly>(
        LogLevel.Error,
        new EventId(6, nameof LoggingExtensions.FailedToFindWorkEventsByWorkIdByPeriod),
        "Failed to find work events by workId {WorkId} and by period [{StartDate} - {EndDateInclusive}].")

    static let failedToFindWorkEventsByPeriod = LoggerMessage.Define<DateOnly, DateOnly>(
        LogLevel.Error,
        new EventId(7, nameof LoggingExtensions.FailedToFindWorkEventsByPeriod),
        "Failed to find work events by period [{StartDate} - {EndDateInclusive}].")

    static let failedToFindByActiveTimePointIdByDate = LoggerMessage.Define<TimePointId, DateTimeOffset>(
        LogLevel.Error,
        new EventId(8, nameof LoggingExtensions.FailedToFindByActiveTimePointIdByDate),
        "Failed to find work events by active time point id {ActiveTimePointId} and created not after {NotAfterDate}.")

    static let workEventInsertingMessage = LoggerMessage.Define<WorkId, string, int64>(
        LogLevel.Debug,
        new EventId(9, "Work Event Inserting"),
        "'{WorkNumber}' work event '{EventName}' is inserting. Create date: {CreatedAtMs}..."
    )

    static let activeTimePointInsertingMessage = LoggerMessage.Define<Name, Kind>(
        LogLevel.Debug,
        new EventId(10, "Active TimePoint Inserting"),
        "'{TimePointNumber}' active time point of '{Kind}' is inserting..."
    )

    [<Extension>]
    static member LogWorkEventInserting(logger: ILogger, workId: WorkId, workEventName: string, createdAt: int64) =
        workEventInsertingMessage.Invoke(logger, workId, workEventName, createdAt, null)

    [<Extension>]
    static member LogActiveTimePointInserting(logger: ILogger, atp: ActiveTimePoint) =
        activeTimePointInsertingMessage.Invoke(logger, atp.Name, atp.Kind, null)

    // -----------------------
    // TableCreated
    // -----------------------
    [<Extension>]
    static member FailedToOpenConnection(logger: ILogger, ex: Exception) =
        failedToOpenConnection.Invoke(logger, ex)

    [<Extension>]
    static member FailedToInsert(logger: ILogger, tableName: string, ex: Exception) =
        failedToInsert.Invoke(logger, tableName, ex)

    [<Extension>]
    static member FailedToUpdateWork(logger: ILogger, work: Work, ex: Exception) =
        let workJson = JsonHelpers.Serialize(work);
        failedToUpdateWork.Invoke(logger, workJson, ex);

    [<Extension>]
    static member FailedToFindWorkEventsByWorkId(logger: ILogger, workId: uint64, ex: Exception) =
        failedToFindWorkEventsByWorkId.Invoke(logger, workId, ex)

    [<Extension>]
    static member FailedToFindWorkEventsByWorkIdByDate(logger: ILogger, workId: uint64, date: DateOnly, ex: Exception) =
        failedToFindWorkEventsByWorkIdByDate.Invoke(logger, workId, date, ex);

    [<Extension>]
    static member FailedToFindWorkEventsByWorkIdByPeriod(logger: ILogger, workId: uint64, period: DateOnlyPeriod, ex: Exception) =
        failedToFindWorkEventsByWorkIdByPeriod.Invoke(logger, workId, period.Start, period.EndInclusive, ex);

    [<Extension>]
    static member FailedToFindWorkEventsByPeriod(logger: ILogger, period: DateOnlyPeriod, ex: Exception) =
        failedToFindWorkEventsByPeriod.Invoke(logger, period.Start, period.EndInclusive, ex);

    [<Extension>]
    static member FailedToFindByActiveTimePointIdByDate(logger: ILogger, activeTimePointId: TimePointId, notAfter: DateTimeOffset, ex: Exception) =
        failedToFindByActiveTimePointIdByDate.Invoke(logger, activeTimePointId, notAfter, ex);
