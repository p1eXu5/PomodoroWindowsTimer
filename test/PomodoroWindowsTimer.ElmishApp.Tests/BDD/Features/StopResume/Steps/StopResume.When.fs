module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.When

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Models


let ``Looper TimePointReduced event has been despatched with`` (activeTimePointId: System.Guid) (expectedSeconds: float<sec>) tolerance =
    Common.``Looper TimePointReduced event has been despatched with`` activeTimePointId expectedSeconds tolerance
    |> Scenario.log (
        sprintf "When.``%s %A %A sec with %A tolerance``."
            (nameof Common.``Looper TimePointReduced event has been despatched with``)
            activeTimePointId
            expectedSeconds
            tolerance
        )

let ``User skips time`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.Dispatch (MainModel.Msg.AppDialog.skipTimeMsg ())
    }

let ``User applies time as break`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.Dispatch (MainModel.Msg.AppDialog.applyMissingTimeAsBreakMsg ())
    }

let ``User applies time as work`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.Dispatch (MainModel.Msg.AppDialog.applyMissingTimeAsWorkMsg ())
    }


let ``User leaves time as break`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.Dispatch (MainModel.Msg.AppDialog.leaveAsBreakMsg ())
    }

let ``SkipOrApplyMissingTime dialog has been shown`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "SkipOrApplyMissingTime dialog has been shown" (fun mainModel ->
            match mainModel.AppDialog with
            | AppDialogModel.SkipOrApplyMissingTime _ -> true
            | _ -> false
        )
    }
    |> Scenario.log $"When.``{nameof ``SkipOrApplyMissingTime dialog has been shown``}``"

let ``Dialog has been closed`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "Dialog has been closed" (fun mainModel ->
            match mainModel.AppDialog with
            | AppDialogModel.NoDialog -> true
            | _ -> false
        )
    }
    |> Scenario.log $"When.``{nameof ``Dialog has been closed``}``"



