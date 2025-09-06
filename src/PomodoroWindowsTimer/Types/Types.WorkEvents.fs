namespace PomodoroWindowsTimer.Types

open System
open System.Diagnostics
open PomodoroWindowsTimer

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

type WorkSpentTime =
    {
        Work: Work
        SpentTime: TimeSpan
    }

// ------------------------------- modules

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
