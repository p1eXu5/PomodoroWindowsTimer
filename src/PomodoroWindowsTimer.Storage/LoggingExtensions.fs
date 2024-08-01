namespace PomodoroWindowsTimer.Storage

open System
open Microsoft.Extensions.Logging
open System.Runtime.CompilerServices
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer

[<Extension>]
type LoggingExtensions() =

    static let failedToOpenConnection = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, nameof LoggingExtensions.FailedToOpenConnection),
        "Failed to open db connection.")
    
    static let failedToCreateTable = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(2, nameof LoggingExtensions.FailedToCreateTable),
        "Failed to create table {TableName}.")

    static let failedToInsert = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(3, nameof LoggingExtensions.FailedToInsert),
        "Failed to insert {TableName}.")

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
        new EventId(5, nameof LoggingExtensions.FailedToFindWorkEventsByWorkIdByPeriod),
        "Failed to find work events by workId {WorkId} and by period [{StartDate} - {EndDateInclusive}].")

    static let failedToFindWorkEventsByPeriod = LoggerMessage.Define<DateOnly, DateOnly>(
        LogLevel.Error,
        new EventId(5, nameof LoggingExtensions.FailedToFindWorkEventsByPeriod),
        "Failed to find work events by period [{StartDate} - {EndDateInclusive}].")

    static let failedToFindByActiveTimePointIdByDate = LoggerMessage.Define<TimePointId, DateTimeOffset>(
        LogLevel.Error,
        new EventId(6, nameof LoggingExtensions.FailedToFindByActiveTimePointIdByDate),
        "Failed to find work events by active time point id {ActiveTimePointId} and created not after {NotAfterDate}.")

    [<Extension>]
    static member FailedToOpenConnection(logger: ILogger, ex: Exception) =
        failedToOpenConnection.Invoke(logger, ex)

    [<Extension>]
    static member FailedToCreateTable(logger: ILogger, tableName: string, ex: Exception) =
        failedToCreateTable.Invoke(logger, tableName, ex)

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
