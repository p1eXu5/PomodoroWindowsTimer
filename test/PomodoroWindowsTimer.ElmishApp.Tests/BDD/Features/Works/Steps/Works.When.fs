module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps.When

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers
open System.ComponentModel
open Elmish.Extensions


let ``SetCurrentWorkIfNone msg has been dispatched with`` (expectedWork: Work) =
    Common.``SetCurrentWorkIfNone msg has been dispatched with`` expectedWork
    |> Scenario.log $"When.``{nameof Common.``SetCurrentWorkIfNone msg has been dispatched with``} {expectedWork}``"


/// Dispatches and wh=ait `MainModel.Msg.SetIsWorkSelectorLoaded true`.
///
/// To wait result call When.``WorkList sub model has been shown``.
let ``WorkSelector drawer is opening`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.Msg.SetIsWorkSelectorLoaded true
        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetIsWorkSelectorLoaded" ((=) msg)
    }

let ``WorkSelector drawer is closing`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.Msg.SetIsWorkSelectorLoaded false
        do sut.Dispatcher.Dispatch msg
    }

let ``CreatingWork sub model has been shown`` () =
    Common.``CreatingWork sub model has been shown`` ()
    |> Scenario.log $"When.``{nameof Common.``CreatingWork sub model has been shown``}``"

let ``WorkList sub model has been shown`` times =
    Common.``WorkList sub model has been shown`` times
    |> Scenario.log $"When.``{nameof Common.``WorkList sub model has been shown``}``"

let ``UpdatingWork sub model has been shown`` () =
    Common.``UpdatingWork sub model has been shown`` ()
    |> Scenario.log $"When.``{nameof Common.``UpdatingWork sub model has been shown``}``"


let ``CreatingWork SetNumber msg has been dispatched with`` workNumber =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg =
            CreatingWorkModel.Msg.SetNumber workNumber
            |> WorkSelectorModel.Msg.CreatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg
        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetNumber" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``CreatingWork SetNumber msg has been dispatched with``}``"


let ``CreatingWork SetTitle msg has been dispatched with`` workTitle =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg =
            CreatingWorkModel.Msg.SetTitle workTitle
            |> WorkSelectorModel.Msg.CreatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg
        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetTitle" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``CreatingWork SetTitle msg has been dispatched with``}``"

let ``CreatingWorkModel CreateWork msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.Dispatch (
            CreatingWorkModel.Msg.CreateWork
            |> AsyncOperation.startUnit
            |> WorkSelectorModel.Msg.CreatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg
        )

        do! Scenario.msgDispatchedWithin2Sec "CreateWork Finish" (function
            | MainModel.Msg.WorkSelectorModelMsg (
                WorkSelectorModel.Msg.CreatingWorkModelMsg (
                    CreatingWorkModel.Msg.CreateWork (AsyncOperation.Finish _)
                )) -> true
            | _ -> false
        )
    }

let ``UpdatingWork SetNumber msg has been dispatched with`` workNumber =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg =
            WorkModel.Msg.SetNumber workNumber
            |> WorkSelectorModel.Msg.UpdatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg
        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetNumber" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``UpdatingWork SetNumber msg has been dispatched with``}``"


let ``UpdatingWork SetTitle msg has been dispatched with`` workTitle =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg =
            WorkModel.Msg.SetTitle workTitle
            |> WorkSelectorModel.Msg.UpdatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg
        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetTitle" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``UpdatingWork SetTitle msg has been dispatched with``}``"

let ``Update WorkModel msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.Dispatch (
            WorkModel.Msg.Update
            |> AsyncOperation.startUnit
            |> WorkSelectorModel.Msg.UpdatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg
        )

        do! Scenario.msgDispatchedWithin2Sec "Update WorkModel Finish" (function
            | MainModel.Msg.WorkSelectorModelMsg (
                WorkSelectorModel.Msg.UpdatingWorkModelMsg (
                    WorkModel.Msg.Update (AsyncOperation.Finish _)
                )) -> true
            | _ -> false
        )
    }
    |> Scenario.log $"When.``{nameof ``Update WorkModel msg has been dispatched``}``"

/// Dispatches and wait WorkListModel.Msg.CreateWork
let ``WorkListModel CreateWork msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        let msg = 
            WorkListModel.Msg.CreateWork
            |> WorkSelectorModel.Msg.WorkListModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg

        do sut.Dispatcher.Dispatch msg

        do! Scenario.msgDispatchedWithin2Sec "WorkListModel CreateWork" ((=) msg)
    }

/// Dispatches and wait WorkListModel.Msg.CreateWork
let ``WorkListModel Edit WorkModel msg has been dispatched`` (work: Work) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        let msg = 
            WorkModel.Msg.Edit
            |> fun smsg ->  WorkListModel.Msg.WorkModelMsg (work.Id, smsg)
            |> WorkSelectorModel.Msg.WorkListModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg

        do sut.Dispatcher.Dispatch msg

        do! Scenario.msgDispatchedWithin2Sec "WorkListModel WorkModel Edit" ((=) msg)
    }


let ``Canceling the creation of work`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        let msg =
            CreatingWorkModel.Msg.Cancel
            |> WorkSelectorModel.Msg.CreatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg

        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "CreatingWorkModel Cancel" ((=) msg)
    }

let ``SkipOrApplyMissingTimeDialog has been opened`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        do! Scenario.modelSatisfiesWithin2Sec "SkipOrApplyMissingTimeDialog has been opened" (fun mainModel ->
            match mainModel.AppDialog with
            | AppDialogModel.SkipOrApplyMissingTime _ -> true
            | _ -> false
        )
    }
    |> Scenario.log $"When.``{nameof ``SkipOrApplyMissingTimeDialog has been opened``}``"

let ``User skipping missing time`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        let msg = MainModel.Msg.AppDialogModelMsg (
            AppDialogModel.Msg.SkipOrApplyMissingTimeModelMsg (
                RollbackWorkModel.Msg.SetLocalRollbackStrategyAndClose (LocalRollbackStrategy.DoNotCorrect)
            )
        )

        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetLocalRollbackStrategyAndClose DoNotCorrect" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``User skipping missing time``}``"

let ``RollbackWorkDialog has been opened`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        do! Scenario.modelSatisfiesWithin2Sec "RollbackWorkDialog has been opened" (fun mainModel ->
            match mainModel.AppDialog with
            | AppDialogModel.RollbackWork _ -> true
            | _ -> false
        )
    }
    |> Scenario.log $"When.``{nameof ``RollbackWorkDialog has been opened``}``"

let ``User do not correct time`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        let msg = MainModel.Msg.AppDialogModelMsg (
            AppDialogModel.Msg.RollbackWorkModelMsg (
                RollbackWorkModel.Msg.SetLocalRollbackStrategyAndClose (LocalRollbackStrategy.DoNotCorrect)
            )
        )

        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetLocalRollbackStrategyAndClose DoNotCorrect" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``User do not correct time``}``"

let ``User inverts time`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        let msg = MainModel.Msg.AppDialogModelMsg (
            AppDialogModel.Msg.RollbackWorkModelMsg (
                RollbackWorkModel.Msg.SetLocalRollbackStrategyAndClose (LocalRollbackStrategy.InvertSpentTime)
            )
        )

        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetLocalRollbackStrategyAndClose InvertSpentTime" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``User inverts time``}``"

