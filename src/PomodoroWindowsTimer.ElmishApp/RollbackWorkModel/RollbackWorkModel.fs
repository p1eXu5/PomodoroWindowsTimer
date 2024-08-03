namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp

type RollbackWorkModel =
    {
        WorkId: uint64
        TimePointKind: Kind
        Time: DateTimeOffset
        Difference: TimeSpan
        RememberChoice: bool
        RollbackStrategy: LocalRollbackStrategy
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
        | CorrectAndClose
        | Close

    let init (workSpentTime: WorkSpentTime) timePointKind time =
        {
            WorkId = workSpentTime.Work.Id
            TimePointKind = timePointKind
            Time = time
            Difference = workSpentTime.SpentTime
            RememberChoice = false
            RollbackStrategy = LocalRollbackStrategy.DoNotCorrect
        }


    let chooseIfWorkKind msg (model: RollbackWorkModel) =
        match model.TimePointKind with
        | Kind.Work -> msg |> Some
        | _ -> None

    let chooseIfBreakKind msg (model: RollbackWorkModel) =
        match model.TimePointKind with
        | Kind.Break
        | Kind.LongBreak -> msg |> Some
        | _ -> None

    let withRollbackStrategy strategy (model: RollbackWorkModel) =
        { model with RollbackStrategy = strategy }

