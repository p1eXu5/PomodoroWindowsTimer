module PomodoroWindowsTimer.Projectors.LinearDiagramProjector

open System
open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open System.Collections.Generic

type private IntermediateProjection =
    {
        WorkEventDurationList : WorkEventDuration list
        DecreaseList : WorkEventDuration list
        IncreaseList : WorkEventDuration list
    }

let rec private firstRunProjection idleThreshold (workEvents: WorkAndEvent list) (res: IntermediateProjection) : IntermediateProjection =
    let idleWorkEventDuration startTime idle =
        {
            Id = 0uL
            Number = "Idle"
            IsWork = System.Nullable()
            StartTime = startTime
            Duration = idle
        }

    let terminalIncreased (lastWorkEvent: WorkAndEvent) (createdAt: DateTimeOffset) isWork =
        match res.WorkEventDurationList with
        | [] ->
            // not finished event -> no duration
            res
           
        | head :: tail ->
            let startTime = TimeOnly.FromDateTime(createdAt.DateTime)
            let duration = head.StartTime - startTime
            { res with
                WorkEventDurationList =
                    {
                        Id = lastWorkEvent.Work.Id
                        Number = lastWorkEvent.Work.Number
                        IsWork = isWork
                        StartTime = startTime
                        Duration = duration
                    }
                    :: head :: tail
            }
            

    match workEvents with
    | [] -> res
    | lastWorkEvent :: other ->
        match lastWorkEvent.Event with
        | WorkEvent.WorkIncreased (createdAt = s; value = v) ->
            { res with
                IncreaseList =
                    {
                        Id = lastWorkEvent.Work.Id
                        Number = lastWorkEvent.Work.Number
                        IsWork = (true |> Nullable.op_Implicit)
                        StartTime = TimeOnly.FromDateTime(s.DateTime)
                        Duration = v
                    }
                    :: res.IncreaseList
            }
            |> firstRunProjection idleThreshold other

        | WorkEvent.BreakIncreased (createdAt = s; value = v) ->
            { res with
                IncreaseList =
                    {
                        Id = lastWorkEvent.Work.Id
                        Number = lastWorkEvent.Work.Number
                        IsWork = (false |> Nullable.op_Implicit)
                        StartTime = TimeOnly.FromDateTime(s.DateTime)
                        Duration = v
                    }
                    :: res.IncreaseList
            }
            |> firstRunProjection idleThreshold other

        | WorkEvent.WorkReduced (createdAt = s; value = v) ->
            { res with
                DecreaseList =
                    {
                        Id = lastWorkEvent.Work.Id
                        Number = lastWorkEvent.Work.Number
                        IsWork = (true |> Nullable.op_Implicit)
                        StartTime = TimeOnly.FromDateTime(s.DateTime)
                        Duration = v
                    }
                    :: res.DecreaseList
            }
            |> firstRunProjection idleThreshold other

        | WorkEvent.BreakReduced (createdAt = s; value = v) ->
            { res with
                DecreaseList =
                    {
                        Id = lastWorkEvent.Work.Id
                        Number = lastWorkEvent.Work.Number
                        IsWork = (false |> Nullable.op_Implicit)
                        StartTime = TimeOnly.FromDateTime(s.DateTime)
                        Duration = v
                    }
                    :: res.DecreaseList
            }

        | WorkEvent.WorkStarted (createdAt = s; ) ->
            terminalIncreased lastWorkEvent s (true |> Nullable.op_Implicit)
            |> firstRunProjection idleThreshold other

        | WorkEvent.BreakStarted (createdAt = s; ) ->
            terminalIncreased lastWorkEvent s (false |> Nullable.op_Implicit)
            |> firstRunProjection idleThreshold other

        | WorkEvent.Stopped (createdAt = s; ) ->
            match res.WorkEventDurationList with
            | [] ->
                // single stopped event
                res
                |> firstRunProjection idleThreshold other

            | head :: tail ->
                let startTime = TimeOnly.FromDateTime(s.DateTime)
                let duration = head.StartTime - startTime
                if duration <= idleThreshold then
                    { res with
                        WorkEventDurationList =
                            { head with StartTime = startTime; Duration = head.Duration.Add(duration) }
                            :: tail
                    }
                else
                    { res with
                        WorkEventDurationList =
                            idleWorkEventDuration startTime duration
                            :: head :: tail
                    }
                |> firstRunProjection idleThreshold other


let projectByPeriod
    (workEventRepository: IWorkEventRepository)
    (idleThreshold: TimeSpan)
    (period: DateOnlyPeriod)
    (cancellationToken: CancellationToken)
    : Task<Result<WorkEventDuration list, string>> =
    task {
        let! res = workEventRepository.FindByPeriodAsync period cancellationToken

        match res with
        | Error err -> return Error $"Failed to obtain work events. {err}"
        | Ok workEventLists ->
            let firstProjection =
                firstRunProjection idleThreshold workEventLists
                    {
                        WorkEventDurationList = []
                        DecreaseList = []
                        IncreaseList = []
                    }

            return Error "Not implemented"
    }

