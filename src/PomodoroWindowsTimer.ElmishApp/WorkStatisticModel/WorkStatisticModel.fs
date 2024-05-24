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

        let dailyBreakTimeRemains (models: WorkStatisticModel list) =
            Statistic.breakMinutesPerDayMax - (models |> sumBreakTime)

