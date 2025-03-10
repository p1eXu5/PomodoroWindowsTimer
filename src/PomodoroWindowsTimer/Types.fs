namespace PomodoroWindowsTimer.Types

open System
open System.Diagnostics
open PomodoroWindowsTimer

type [<Measure>] ms
type [<Measure>] sec
type [<Measure>] min

type Name = string

[<Struct>]
type DateOnlyPeriod =
    {
        Start: DateOnly
        EndInclusive: DateOnly
    }
    static member Zero =
        {
            Start = DateOnly()
            EndInclusive = DateOnly()
        }

[<Struct>]
type DateTimePeriod =
    {
        Start: DateTime
        EndInclusive: DateTime
    }
    static member Zero =
        {
            Start = DateTime()
            EndInclusive = DateTime()
        }

type Kind =
    | Work
    | Break
    | LongBreak

/// Kind alias, used in patterns.
type Alias = private Alias of string

type TimePointId = Guid

type TimePoint =
    {
        Id: TimePointId
        Name: Name
        TimeSpan: TimeSpan
        Kind: Kind
        KindAlias: Alias
    }

type ActiveTimePoint =
    {
        Id: TimePointId
        OriginalId: TimePointId
        Name: Name
        // TODO: rename to RemaininigTimeSpan
        RemainingTimeSpan: TimeSpan
        TimeSpan: TimeSpan
        Kind: Kind
        KindAlias: Alias
    }

type TimePointPrototype =
    {
        Name: string
        Kind: Kind
        KindAlias: Alias
        TimeSpan: TimeSpan
    }


type LooperEvent =
    | TimePointStarted of TimePointStartedEventArgs
    | TimePointTimeReduced of ActiveTimePoint
and
    TimePointStartedEventArgs =
        {
            NewActiveTimePoint: ActiveTimePoint
            OldActiveTimePoint: ActiveTimePoint option
        }


type Pattern = string

type WorkId = uint64

type Work =
    {
        Id: WorkId
        Number: string
        Title: string
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
        LastEventCreatedAt: DateTimeOffset option
    }

type WorkEvent =
    | WorkStarted of createdAt: DateTimeOffset * timePointName: string * activeTimePointId: TimePointId
    | BreakStarted of createdAt: DateTimeOffset * timePointName: string * activeTimePointId: TimePointId
    | Stopped of createdAt: DateTimeOffset
    | WorkReduced of createdAt: DateTimeOffset * value: TimeSpan * activeTimePointId: TimePointId option
    | WorkIncreased of createdAt: DateTimeOffset * value: TimeSpan * activeTimePointId: TimePointId option
    | BreakReduced of createdAt: DateTimeOffset * value: TimeSpan * activeTimePointId: TimePointId option
    | BreakIncreased of createdAt: DateTimeOffset * value: TimeSpan * activeTimePointId: TimePointId option

type WorkEventList =
    {
        Work: Work
        Events: WorkEvent list
    }

type WorkAndEvent =
    {
        Work: Work
        Event: WorkEvent
    }

type Statistic =
    {
        Period: DateTimePeriod
        WorkTime: TimeSpan
        BreakTime: TimeSpan
        TimePointNameStack: string list
    }

type WorkStatistic =
    {
        Work: Work
        Statistic: Statistic option
    }

/// Value for LinearDiagram
[<Struct>]
type WorkEventDuration =
    {
        Id: WorkId
        Number: string
        IsWork: Nullable<bool>
        StartTime: TimeOnly
        Duration: TimeSpan
    }

type DailyStatistic =
    {
        Date: DateOnly
        WorkStatistic: WorkStatistic list
    }

type WorkEventOffsetTime =
    {
        WorkEvent: WorkEvent
        OffsetTime: TimeSpan option
    }

type WorkEventOffsetTimeList =
    {
        Work: Work
        OffsetTimes: WorkEventOffsetTime list
    }

[<DebuggerDisplay("Num = {Num}, End = {End}")>]
type IdleExcelRow =
    {
        Num: int
        End: TimeOnly
    }

[<DebuggerDisplay("Num = {Num}, End = {End}, WorkId = {Work.Id}")>]
type WorkExcelRow =
    {
        Num: int
        Work: Work
        End: TimeOnly
    }

type ExcelRow =
    | WorkExcelRow of WorkExcelRow
    | IdleExcelRow of IdleExcelRow

type WorkSpentTime =
    {
        Work: Work
        SpentTime: TimeSpan
    }

type StartRow = int

// ------------------------------- modules

module DateOnlyPeriod =

    let create start endInclusive : DateOnlyPeriod =
        {
            Start = start
            EndInclusive = endInclusive
        }

    let isOneDay (period: DateOnlyPeriod) =
        period.Start = period.EndInclusive

module TimePointStartedEventArgs =
    let init newActiveTimePoint oldActiveTimePoint : TimePointStartedEventArgs =
        {
            NewActiveTimePoint = newActiveTimePoint
            OldActiveTimePoint = oldActiveTimePoint
        }

module ExcelRow =

    let createWorkExcelRow num (work: Work) (endTimeOnly: TimeOnly) =
        {
            Num = num
            Work = work
            End = endTimeOnly
        }
        |> ExcelRow.WorkExcelRow

    let createIdleExcelRow num (endTimeOnly: TimeOnly) =
        {
            Num = num
            End = endTimeOnly
        }
        |> ExcelRow.IdleExcelRow

    let addTime (time: TimeSpan) = function
        | ExcelRow.WorkExcelRow w ->
            { w with End = w.End.Add(time) } |> ExcelRow.WorkExcelRow
        | ExcelRow.IdleExcelRow idle ->
            { idle with End = idle.End.Add(time) } |> ExcelRow.IdleExcelRow

    let subTime (time: TimeSpan) = function
        | ExcelRow.WorkExcelRow w ->
            { w with End = w.End.Add(-time) } |> ExcelRow.WorkExcelRow
        | ExcelRow.IdleExcelRow idle ->
            { idle with End = idle.End.Add(-time) } |> ExcelRow.IdleExcelRow

    let num = function
        | ExcelRow.WorkExcelRow w ->
            w.Num
        | ExcelRow.IdleExcelRow idle ->
            idle.Num

    let endAddTime (time: TimeSpan) = function
        | ExcelRow.WorkExcelRow w ->
            w.End.Add(time)
        | ExcelRow.IdleExcelRow idle ->
            idle.End.Add(time)

    let endTimeOnly = function
        | ExcelRow.WorkExcelRow w ->
            w.End
        | ExcelRow.IdleExcelRow idle ->
            idle.End

module WorkExcelRow =

    let subTime (time: TimeSpan) (row: WorkExcelRow) =
        { row with End = row.End.Add(-time) }

    let addTime (time: TimeSpan) (row: WorkExcelRow) =
        { row with End = row.End.Add(time) }

module WorkEvent =

    let createdAt = function
        | WorkEvent.WorkStarted (dt, _, _)
        | WorkEvent.BreakStarted (dt, _, _)
        | WorkEvent.Stopped (dt)
        | WorkEvent.WorkReduced (dt,_, _)
        | WorkEvent.WorkIncreased (dt,_, _) 
        | WorkEvent.BreakReduced (dt,_, _)
        | WorkEvent.BreakIncreased (dt,_, _) -> dt

    let dateOnly = function
        | WorkEvent.WorkStarted (dt, _, _)
        | WorkEvent.BreakStarted (dt, _, _)
        | WorkEvent.Stopped (dt) 
        | WorkEvent.WorkReduced (dt,_, _)
        | WorkEvent.WorkIncreased (dt,_, _) 
        | WorkEvent.BreakReduced (dt,_, _)
        | WorkEvent.BreakIncreased (dt,_, _) ->
            DateOnly.FromDateTime(dt.DateTime)

    let localDateTime = function
        | WorkEvent.WorkStarted (dt, _, _)
        | WorkEvent.BreakStarted (dt, _, _)
        | WorkEvent.Stopped (dt)
        | WorkEvent.WorkReduced (dt,_, _)
        | WorkEvent.WorkIncreased (dt,_, _) 
        | WorkEvent.BreakReduced (dt,_, _)
        | WorkEvent.BreakIncreased (dt,_, _) ->
            dt.LocalDateTime

    let tpName = function
        | WorkEvent.WorkStarted (_, n, _)
        | WorkEvent.BreakStarted (_, n, _) -> n |> Some
        | WorkEvent.Stopped _
        | WorkEvent.WorkReduced _
        | WorkEvent.WorkIncreased _ 
        | WorkEvent.BreakReduced _
        | WorkEvent.BreakIncreased _ -> None

    let isStopped = function
        | WorkEvent.Stopped _ -> true
        | _ -> false

    let activeTimePointId = function
        | WorkEvent.WorkStarted (_, _, id)
        | WorkEvent.BreakStarted (_, _, id) -> id |> Some
        | WorkEvent.Stopped _ -> None
        | WorkEvent.WorkReduced (_, _, id)
        | WorkEvent.WorkIncreased (_, _, id) 
        | WorkEvent.BreakReduced (_, _, id)
        | WorkEvent.BreakIncreased (_, _, id) -> id

    let name = function
        | WorkEvent.WorkStarted _ -> nameof WorkEvent.WorkStarted
        | WorkEvent.BreakStarted _ -> nameof WorkEvent.BreakStarted
        | WorkEvent.Stopped _ -> nameof WorkEvent.Stopped
        | WorkEvent.WorkReduced _ -> nameof WorkEvent.WorkReduced
        | WorkEvent.WorkIncreased _ -> nameof WorkEvent.WorkIncreased
        | WorkEvent.BreakReduced _ -> nameof WorkEvent.BreakReduced
        | WorkEvent.BreakIncreased _ -> nameof WorkEvent.BreakIncreased

    let withActiveTimePointId (activeTimePointId: TimePointId) (workEvent: WorkEvent) =
        match workEvent with
        | WorkEvent.WorkStarted (d, n , _) -> WorkEvent.WorkStarted (d, n, activeTimePointId)
        | WorkEvent.BreakStarted (d, n , _) -> WorkEvent.BreakStarted (d, n, activeTimePointId)
        | WorkEvent.WorkReduced (d, v, _) -> WorkEvent.WorkReduced (d, v, activeTimePointId |> Some)
        | WorkEvent.WorkIncreased (d, v, _) -> WorkEvent.WorkIncreased (d, v, activeTimePointId |> Some)
        | WorkEvent.BreakReduced (d, v, _) -> WorkEvent.BreakReduced (d, v, activeTimePointId |> Some)
        | WorkEvent.BreakIncreased (d, v, _) -> WorkEvent.BreakIncreased (d, v, activeTimePointId |> Some)
        | WorkEvent.Stopped _ -> workEvent

    let withoutActiveTimePointId (workEvent: WorkEvent) =
        match workEvent with
        | WorkEvent.WorkReduced (d, v, _) -> WorkEvent.WorkReduced (d, v, None)
        | WorkEvent.WorkIncreased (d, v, _) -> WorkEvent.WorkIncreased (d, v, None)
        | WorkEvent.BreakReduced (d, v, _) -> WorkEvent.BreakReduced (d, v, None)
        | WorkEvent.BreakIncreased (d, v, _) -> WorkEvent.BreakIncreased (d, v, None)
        | WorkEvent.WorkStarted _
        | WorkEvent.BreakStarted _
        | WorkEvent.Stopped _ -> workEvent

    let filterBreakStartStopped = function
        | WorkEvent.BreakStarted _ 
        | WorkEvent.Stopped _ -> true
        | _ -> false

    let filterBreakIncreasedReduced = function
        | WorkEvent.BreakIncreased _ 
        | WorkEvent.BreakReduced _ -> true
        | _ -> false

    let filterWorkStartStopped = function
        | WorkEvent.WorkStarted _ 
        | WorkEvent.Stopped _ -> true
        | _ -> false

    let filterWorkIncreasedReduced = function
        | WorkEvent.WorkIncreased _ 
        | WorkEvent.WorkReduced _ -> true
        | _ -> false

    open Helpers.DateTimeOffset

    let toString (workEvent: WorkEvent) =
        match workEvent with
        | WorkEvent.WorkStarted (d, n, tpId) -> $"WorkEvent.{nameof WorkEvent.WorkStarted} (DateTimeOffset.ParseExact(\"{d.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture), \"{n}\", Guid.Parse(\"{tpId.ToString()}\"))"
        | WorkEvent.BreakStarted (d, n, tpId) -> $"WorkEvent.{nameof WorkEvent.BreakStarted} (DateTimeOffset.ParseExact(\"{d.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture), \"{n}\", Guid.Parse(\"{tpId.ToString()}\"))"
        | WorkEvent.WorkReduced (d, v, tpId) -> $"WorkEvent.{nameof WorkEvent.WorkReduced} (DateTimeOffset.ParseExact(\"{d.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture), TimeSpan.FromMilliseconds({v.Milliseconds}), Guid.Parse(\"{tpId.ToString()}\"))"
        | WorkEvent.WorkIncreased (d, v, tpId) -> $"WorkEvent.{nameof WorkEvent.WorkIncreased} (DateTimeOffset.ParseExact(\"{d.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture), TimeSpan.FromMilliseconds({v.Milliseconds}), Guid.Parse(\"{tpId.ToString()}\"))"
        | WorkEvent.BreakReduced (d, v, tpId) -> $"WorkEvent.{nameof WorkEvent.BreakReduced} (DateTimeOffset.ParseExact(\"{d.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture), TimeSpan.FromMilliseconds({v.Milliseconds}), Guid.Parse(\"{tpId.ToString()}\"))"
        | WorkEvent.BreakIncreased (d, v, tpId) -> $"WorkEvent.{nameof WorkEvent.BreakIncreased} (DateTimeOffset.ParseExact(\"{d.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture), TimeSpan.FromMilliseconds({v.Milliseconds}), Guid.Parse(\"{tpId.ToString()}\"))"
        | WorkEvent.Stopped d -> $"WorkEvent.{nameof WorkEvent.Stopped} (DateTimeOffset.ParseExact(\"{d.ToString(defaultFormat)}\", defaultFormat, CultureInfo.InvariantCulture))"

module WorkEventList =
    module List =
        let groupByDay (workEvents: WorkEventList list) =
            workEvents
            |> List.map (fun wel ->
                wel.Events
                |> List.groupBy (WorkEvent.dateOnly)
                |> List.map (fun (day, events) ->
                    (
                        day,
                        {
                            Work = wel.Work
                            Events = events
                        }
                    )
                )
            )
            |> List.concat
            |> List.groupBy fst
            |> List.map (fun (day, wel) ->
                (day, wel |> List.map snd)
            )
            |> List.sortBy fst

module Statistic =
    /// 14 * 25 min pomodoro + 1 * 15 min pomodoro + 11 * 5 min break + 3 * 20 min long break
    [<Literal>]
    let OVERALL_MINUTES_PER_DAY_MAX = 8.0 * 60.0

    [<Literal>]
    let WORK_MINUTES_PER_DAY_MAX = 25. * 4. * 3. + 25. + 25. + 15.

    [<Literal>]
    let BREAK_MINUTES_PER_DAY_MAX =
        OVERALL_MINUTES_PER_DAY_MAX - WORK_MINUTES_PER_DAY_MAX

    let breakMinutesPerDayMax = TimeSpan.FromMinutes(BREAK_MINUTES_PER_DAY_MAX)
    let workMinutesPerDayMax = TimeSpan.FromMinutes(WORK_MINUTES_PER_DAY_MAX)
    let overallMinutesPerDayMax = TimeSpan.FromMinutes(OVERALL_MINUTES_PER_DAY_MAX)

    let total (statistic: Statistic) =
        statistic.BreakTime + statistic.WorkTime

    /// Returns difference
    let chooseNotCompleted (statistic: Statistic) =
        let total = statistic.BreakTime + statistic.WorkTime
        if total < overallMinutesPerDayMax then
            overallMinutesPerDayMax - total |> Some
        else
            None


module Alias =
    let create str =
        let maxLen = 10
        if String.IsNullOrEmpty(str) then
            Error "Alias must not be null or empty"
        elif str.Length > maxLen then
            let msg = sprintf "Alias must not be more than %i chars" maxLen 
            Error msg
        elif str |> Seq.exists (fun ch -> (not <| Char.IsLetter(ch)) || (not <| Char.IsLower(ch))) then
            Error "Alias must contain only lower letters"
        else
            Ok (Alias str)

    let orThrow = function
        | Ok alias -> alias
        | Error err -> failwith err

    let createOrThrow str =
        str |> create |> orThrow

    let value (Alias v) = v


module Kind =
    let displayString = function
        | Work -> "WORK"
        | Break -> "BREAK"
        | LongBreak -> "LONG BREAK"

    let alias = 
        (function
            | Work -> "w"
            | Break -> "b"
            | LongBreak -> "lb")
        >> Alias.createOrThrow

    [<CompiledName("ToShortString")>]
    let toShortString = function
        | Work -> "W"
        | Break -> "BR"
        | LongBreak -> "LB"

    [<CompiledName("IsBreak")>]
    let isBreak = function
        | Work -> false
        | Break
        | LongBreak -> true

    [<CompiledName("IsWork")>]
    let isWork = isBreak >> not



module TimePointPrototype =

    let defaults =
        [
            { Name = "Focused work"; Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(25) }
            { Name = "Break"; Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(5) }
            { Name = "Long break"; Kind = Kind.LongBreak; KindAlias = Kind.LongBreak |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(20) }
        ]

    let toTimePoint ind prototype =
        {
            Id = Guid.NewGuid()
            Name = sprintf "%s %i" prototype.Name ind
            TimeSpan = prototype.TimeSpan
            Kind = prototype.Kind
            KindAlias = prototype.KindAlias
        }


module TimePoint =

    [<CompiledName("Defaults")>]
    let defaults =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 3"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 3"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 4"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Long Break"; TimeSpan = TimeSpan.FromMinutes(20); Kind = Kind.LongBreak; KindAlias = Kind.LongBreak |> Kind.alias }
        ]

    let testDefaults =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromSeconds(4); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromSeconds(4); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
        ]

    [<CompiledName("ToActiveTimePointWith")>]
    let toActiveTimePointWith (runningTimeSpan: TimeSpan) (timePoint: TimePoint) =
        {
            Id = Guid.NewGuid()
            OriginalId = timePoint.Id
            Name = timePoint.Name
            RemainingTimeSpan = runningTimeSpan
            TimeSpan = timePoint.TimeSpan
            Kind = timePoint.Kind
            KindAlias = timePoint.KindAlias
        }

    let toActiveTimePointWithSec (runningSeconds: float<sec>) (timePoint: TimePoint) =
        toActiveTimePointWith (TimeSpan.FromSeconds(float runningSeconds)) timePoint

    [<CompiledName("ToActiveTimePoint")>]
    let toActiveTimePoint (timePoint: TimePoint) =
        toActiveTimePointWith timePoint.TimeSpan timePoint

module Pattern =

    let defaults =
        [
            "(w-b)3-w-lb"
        ]