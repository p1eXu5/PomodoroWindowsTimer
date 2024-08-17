module PomodoroWindowsTimer.WorkEventProjector

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

type private StatisticBuilder =
    | Initialized
    | Calculating of Statistic * lastWorkEvent: WorkEvent


let internal projectStatistic (workEvents: WorkEvent list) =
    workEvents
    |> List.fold (fun builder ev ->
        match builder with
        // we suppose that first stopped event is not preceeded to started
        // TODO: add this case processing into store
        | Initialized ->
            let evDate = ev |> WorkEvent.localDateTime
            let tpName = ev |> WorkEvent.tpName
            let statistic =
                {
                    Period =
                        {
                            Start = evDate
                            EndInclusive = evDate
                        }
                    WorkTime = TimeSpan.Zero
                    BreakTime = TimeSpan.Zero
                    TimePointNameStack = tpName |> Option.map List.singleton |> Option.defaultValue []
                }
            (statistic, ev) |> Calculating

        | Calculating (stat, lastEvent) ->
            let evDate = ev |> WorkEvent.localDateTime
            match ev, lastEvent with
            | WorkEvent.WorkStarted (currDt, n, _), WorkEvent.WorkStarted (createdAt = prevDt) ->
                Calculating (
                    { stat with
                        WorkTime = stat.WorkTime + (currDt - prevDt)
                        TimePointNameStack = n :: stat.TimePointNameStack
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.WorkStarted (currDt, n, _), WorkEvent.BreakStarted (createdAt = prevDt) ->
                Calculating (
                    { stat with
                        BreakTime = stat.BreakTime + (currDt - prevDt)
                        TimePointNameStack = n :: stat.TimePointNameStack
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.WorkStarted (timePointName = n), WorkEvent.Stopped _ ->
                Calculating (
                    { stat with
                        TimePointNameStack = n :: stat.TimePointNameStack
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.BreakStarted (currDt, n, _), WorkEvent.BreakStarted (createdAt = prevDt) ->
                Calculating (
                    { stat with
                        BreakTime = stat.BreakTime + (currDt - prevDt)
                        TimePointNameStack = n :: stat.TimePointNameStack
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.BreakStarted (currDt, n, _), WorkEvent.WorkStarted (createdAt = prevDt) ->
                Calculating (
                    { stat with
                        WorkTime = stat.WorkTime + (currDt - prevDt)
                        TimePointNameStack = n :: stat.TimePointNameStack
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.BreakStarted (timePointName = n), WorkEvent.Stopped _ ->
                Calculating (
                    { stat with
                        TimePointNameStack = n :: stat.TimePointNameStack
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.Stopped _, WorkEvent.Stopped _ ->
                Calculating (stat, lastWorkEvent = ev)

            | WorkEvent.Stopped (currDt), WorkEvent.WorkStarted (createdAt = prevDt) ->
                Calculating (
                    { stat with
                        WorkTime = stat.WorkTime + (currDt - prevDt)
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.Stopped (currDt), WorkEvent.BreakStarted (createdAt = prevDt) ->
                Calculating (
                    { stat with
                        BreakTime = stat.BreakTime + (currDt - prevDt)
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.WorkReduced (_, v, _), _ ->
                Calculating (
                    { stat with
                        WorkTime = stat.WorkTime - v
                    }, lastWorkEvent = lastEvent)

            | WorkEvent.WorkIncreased (_, v, _), _ ->
                Calculating (
                    { stat with
                        WorkTime = stat.WorkTime + v
                    }, lastWorkEvent = lastEvent)

            | WorkEvent.BreakReduced (_, v, _), _ ->
                Calculating (
                    { stat with
                        BreakTime = stat.BreakTime - v
                    }, lastWorkEvent = lastEvent)

            | WorkEvent.BreakIncreased (_, v, _), _ ->
                Calculating (
                    { stat with
                        BreakTime = stat.BreakTime + v
                    }, lastWorkEvent = lastEvent)

            | _ -> raise (ArgumentException($"Unpredictable event order. Current: {ev}, previous: {lastEvent}"))
    ) StatisticBuilder.Initialized
    |> function
        | Initialized -> None
        | Calculating (stat, _) ->
            { stat with TimePointNameStack = stat.TimePointNameStack |> List.distinct |> List.rev }
            |> Some


let projectAllByPeriod (workEventRepo: IWorkEventRepository) (period: DateOnlyPeriod) ct =
    task {
        let! res = workEventRepo.FindAllByPeriodAsync period ct
        return
            res
            |> Result.map (fun workEvents ->
                workEvents
                |> List.map (fun workEvents ->
                    {
                        Work = workEvents.Work
                        Statistic = workEvents.Events |> projectStatistic
                    }
                )
            )
    }

let projectDailyByPeriod (workEventRepo: IWorkEventRepository) (period: DateOnlyPeriod) ct =
    task {
        let! res = workEventRepo.FindAllByPeriodAsync period ct
        return
            res
            |> Result.map (fun workEvents ->
                workEvents
                |> WorkEventList.List.groupByDay
                |> List.map (fun (day, wel) ->
                    {
                        Date = day
                        WorkStatistic =
                            wel
                            |> List.map (fun wel ->
                                {
                                    Work = wel.Work
                                    Statistic = wel.Events |> projectStatistic
                                }
                            )
                    }
                )
            )
    }

