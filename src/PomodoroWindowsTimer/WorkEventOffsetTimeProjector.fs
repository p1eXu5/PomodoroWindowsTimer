module PomodoroWindowsTimer.WorkEventOffsetTimeProjector

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions


let internal project (workEvents: WorkEvent list) =
    match workEvents with
    | [] -> []
    | head::tail ->
        tail
        |> List.fold (fun (acc, lastEvent) ev ->
            // we suppose that first stopped event is not preceeded to started
            // TODO: add this case processing into store
            match ev, lastEvent with
            | WorkEvent.Stopped endDt, WorkEvent.WorkStarted (createdAt = startDt)
            | WorkEvent.Stopped endDt, WorkEvent.BreakStarted (createdAt = startDt) ->
                let offsetTime =
                    {
                        WorkEvent = ev
                        OffsetTime = (endDt - startDt) |> Some
                    }
                (offsetTime :: acc, ev)

            | WorkEvent.WorkStarted (createdAt = endDt), WorkEvent.WorkStarted (createdAt = startDt)
            | WorkEvent.WorkStarted (createdAt = endDt), WorkEvent.BreakStarted (createdAt = startDt)
            | WorkEvent.BreakStarted (createdAt = endDt), WorkEvent.BreakStarted (createdAt = startDt)
            | WorkEvent.BreakStarted (createdAt = endDt), WorkEvent.WorkStarted (createdAt = startDt)
                ->
                let offsetTime0 =
                    {
                        WorkEvent = ev
                        OffsetTime = None
                    }
                let offsetTime1 =
                    {
                        WorkEvent = WorkEvent.Stopped endDt
                        OffsetTime = (endDt - startDt) |> Some
                    }
                (offsetTime0 :: offsetTime1 :: acc, ev)
            | WorkEvent.WorkStarted _, WorkEvent.Stopped _
            | WorkEvent.BreakStarted _, WorkEvent.Stopped _
                ->
                let offsetTime =
                    {
                        WorkEvent = ev
                        OffsetTime = None
                    }
                (offsetTime :: acc, ev)
            | WorkEvent.WorkIncreased (value = v), _
            | WorkEvent.BreakIncreased (value = v), _ ->
                let offsetTime =
                    {
                        WorkEvent = ev
                        OffsetTime = v |> Some
                    }
                (offsetTime :: acc, lastEvent)
            | WorkEvent.WorkReduced (value = v), _
            | WorkEvent.WorkReduced (value = v), _ ->
                let offsetTime =
                    {
                        WorkEvent = ev
                        OffsetTime = -v |> Some
                    }
                (offsetTime :: acc, lastEvent)

            | _ -> raise (ArgumentException($"Unpredictable event order. Current: {ev}, previous: {lastEvent}"))

        ) ([{ WorkEvent = head; OffsetTime = None }], head)
        |> fun (acc, _) -> acc |> List.rev


let projectByWorkIdByPeriod (workEventRepo: IWorkEventRepository) (workId: uint64) (period: DateOnlyPeriod) ct =
    task {
        let! res = workEventRepo.FindByWorkIdByPeriodAsync workId period ct
        return res |> Result.map project
    }

let projectByPeriod (workEventRepo: IWorkEventRepository) (period: DateOnlyPeriod) ct =
    task {
        let! res = workEventRepo.FindAllByPeriodAsync period ct
        return
            res
            |> Result.map (
                List.map (fun wel ->
                    {
                        Work = wel.Work
                        OffsetTimes = wel.Events |> project
                    }
                )
            )
    }
