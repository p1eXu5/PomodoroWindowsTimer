module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps.When

open System
open PomodoroWindowsTimer.Types
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




let ``SetNumber msg has been dispatched with`` workNumber =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg =
            CreatingWorkModel.Msg.SetNumber workNumber
            |> WorkSelectorModel.Msg.CreatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg
        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetNumber" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``SetNumber msg has been dispatched with``}``"


let ``SetTitle msg has been dispatched with`` workTitle =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg =
            CreatingWorkModel.Msg.SetTitle workTitle
            |> WorkSelectorModel.Msg.CreatingWorkModelMsg
            |> MainModel.Msg.WorkSelectorModelMsg
        do sut.Dispatcher.Dispatch msg
        do! Scenario.msgDispatchedWithin2Sec "SetTitle" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``SetTitle msg has been dispatched with``}``"


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

