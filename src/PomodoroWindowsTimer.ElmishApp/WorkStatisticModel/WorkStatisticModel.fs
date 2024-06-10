namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types

type WorkStatisticModel =
    {
        Work: WorkModel
        Statistic: Statistic option
    }
    member this.WorkId = this.Work.Id


module WorkStatisticModel =

    type Msg =
        | WorkModelMsg of WorkModel.Msg

    let init (workStatistic: WorkStatistic) =
        {
            Work = WorkModel.init workStatistic.Work
            Statistic = workStatistic.Statistic
        }

    module List =
        let sumBreakTime (models: WorkStatisticModel list) =
            models
            |> List.choose _.Statistic
            |> List.map _.BreakTime
            |> List.reduce (+)

        let sumWorkTime (models: WorkStatisticModel list) =
            models
            |> List.choose _.Statistic
            |> List.map _.WorkTime
            |> List.reduce (+)

        let sumOverallTime (models: WorkStatisticModel list) =
            models
            |> List.choose _.Statistic
            |> List.map (fun s -> s.WorkTime + s.BreakTime)
            |> List.reduce (+)

        let sumOverallAndBreakTime (models: WorkStatisticModel list) =
            models
            |> List.choose _.Statistic
            |> List.map (fun s -> (s.WorkTime, s.BreakTime))
            |> List.reduce (fun (w1, b1) (w2, b2) ->
                (w1 + w2 + b1 + b2), (b1 + b2)
            )

        let dailyBreakTimeRemains (models: WorkStatisticModel list) =
            Statistic.breakMinutesPerDayMax - (models |> sumBreakTime)

        let dailyWorkTimeRemains (models: WorkStatisticModel list) =
            Statistic.workMinutesPerDayMax - (models |> sumWorkTime)

        let dailyOverallTimeRemains (models: WorkStatisticModel list) =
            Statistic.overallMinutesPerDayMax - (models |> sumOverallTime)

        let dailyOverallAndBreakTimeRemains (models: WorkStatisticModel list) =
            let (sumOverall, sumBreak) = (models |> sumOverallAndBreakTime)
            Statistic.overallMinutesPerDayMax - sumOverall
            , Statistic.breakMinutesPerDayMax - sumBreak
