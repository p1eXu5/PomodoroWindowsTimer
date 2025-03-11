namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp

type RollbackWorkModel =
    {
        Work: Work
        Kind: Kind
        ActiveTimePointId: TimePointId
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

    let init (workSpentTime: WorkSpentTime) timePointKind timePointId time =
        {
            Work = workSpentTime.Work
            Kind = timePointKind
            ActiveTimePointId = timePointId
            Time = time
            Difference = workSpentTime.SpentTime
            RememberChoice = false
            RollbackStrategy = LocalRollbackStrategy.DoNotCorrect
        }

    let initWithMissingTime work timePointKind timePointId difference time =
        {
            Work = work
            Kind = timePointKind
            ActiveTimePointId = timePointId
            Time = time
            Difference = difference
            RememberChoice = false
            RollbackStrategy = LocalRollbackStrategy.DoNotCorrect
        }


    let chooseIfWorkKind msg (model: RollbackWorkModel) =
        match model.Kind with
        | Kind.Work -> msg |> Some
        | _ -> None

    let chooseIfBreakKind msg (model: RollbackWorkModel) =
        match model.Kind with
        | Kind.Break
        | Kind.LongBreak -> msg |> Some
        | _ -> None

    let withRollbackStrategy strategy (model: RollbackWorkModel) =
        { model with RollbackStrategy = strategy }

