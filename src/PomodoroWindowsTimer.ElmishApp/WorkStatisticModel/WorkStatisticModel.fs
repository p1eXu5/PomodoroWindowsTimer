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

