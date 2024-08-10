module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.When

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests


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

