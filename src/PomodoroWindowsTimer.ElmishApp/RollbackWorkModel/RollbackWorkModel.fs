namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp

type RollbackWorkModel =
    {
        WorkId: uint64
        Time: DateTimeOffset
        Difference: TimeSpan
        RememberChoice: bool
        LocalStrategy: LocalRollbackStrategy
    }

module RollbackWorkModel =

    type Cfg =
        {
            WorkEventRepository: IWorkEventRepository
            UserSettings: IUserSettings
        }

    type Msg =
        // TODO: | SetRememberChoice of bool
        | SetLocalRollbackStrategy of LocalRollbackStrategy
        | SetLocalRollbackStrategyAndClose of LocalRollbackStrategy

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | SubstractWorkAddBreakAndClose
        | DefaultedAndClose

    let init (workSpentTime: WorkSpentTime) time localStrategy =
        {
            WorkId = workSpentTime.Work.Id
            Time = time
            Difference = workSpentTime.SpentTime
            RememberChoice = false
            LocalStrategy = localStrategy
        }


