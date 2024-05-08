module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.When

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers


let ``Looper TimePointReduced event has been despatched with`` (activeTimePointId: System.Guid) (expectedSeconds: float<sec>) tolerance =
    Common.``Looper TimePointReduced event has been despatched with`` activeTimePointId expectedSeconds tolerance
    |> Scenario.log (
        sprintf "When.``%s %A %A sec with %A tolerance``."
            (nameof Common.``Looper TimePointReduced event has been despatched with``)
            activeTimePointId
            expectedSeconds
            tolerance
        )

