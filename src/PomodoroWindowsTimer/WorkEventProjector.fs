module PomodoroWindowsTimer.WorkEventProjector

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

type private StatisticBuilder =
    | Initialized
    | Calculating of Statistic * lastWorkEvent: WorkEvent

let internal project (workEvents: WorkEvent list) =
    workEvents
    |> List.fold (fun builder ev ->
        match builder with
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
            | WorkEvent.WorkStarted (currDt, n), WorkEvent.WorkStarted (createdAt = prevDt) ->
                Calculating (
                    { stat with
                        WorkTime = stat.WorkTime + (currDt - prevDt)
                        TimePointNameStack = n :: stat.TimePointNameStack
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.WorkStarted (currDt, n), WorkEvent.BreakStarted (createdAt = prevDt) ->
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

            | WorkEvent.BreakStarted (currDt, n), WorkEvent.BreakStarted (createdAt = prevDt) ->
                Calculating (
                    { stat with
                        BreakTime = stat.BreakTime + (currDt - prevDt)
                        TimePointNameStack = n :: stat.TimePointNameStack
                        Period = { stat.Period with EndInclusive = evDate }
                    }, lastWorkEvent = ev)

            | WorkEvent.BreakStarted (currDt, n), WorkEvent.WorkStarted (createdAt = prevDt) ->
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

    ) StatisticBuilder.Initialized
    |> function
        | Initialized -> None
        | Calculating (stat, _) ->
            { stat with TimePointNameStack = stat.TimePointNameStack |> List.distinct |> List.rev }
            |> Some

let projectByWorkIdDaily (workEventRepo: IWorkEventRepository) (workId: uint64) (date: DateOnly) ct =
    task {
        let! res = workEventRepo.FindByWorkIdByDateAsync workId date ct
        return res |> Result.map (Seq.toList >> project)
    }

let projectByWorkIdByPeriod (workEventRepo: IWorkEventRepository) (workId: uint64) (period: DateOnlyPeriod) ct =
    task {
        let! res = workEventRepo.FindByWorkIdByPeriodAsync workId period ct
        return res |> Result.map (Seq.toList >> project)
    }

let projectByWorkId (workEventRepo: IWorkEventRepository) (workId: uint64) ct =
    task {
        let! res = workEventRepo.FindByWorkIdAsync workId ct
        return res |> Result.map (Seq.toList >> project)
    }


let projectAllByPeriod (workEventRepo: IWorkEventRepository) (period: DateOnlyPeriod) ct =
    task {
        let! res = workEventRepo.FindAllByPeriodAsync period ct
        return
            res
            |> Result.map (fun workEventsSeq ->
                workEventsSeq
                |> Seq.map (fun workEvents ->
                    {
                        Work = workEvents.Work
                        Statistic = workEvents.Events |> project
                    }
                )
                |> Seq.toList
            )
    }

