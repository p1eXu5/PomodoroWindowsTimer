module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.Then

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features

let ``Looper TimePointStarted event has been despatched with`` newTimePoint oldTimePoint =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "Finish of LoadRecipeCards" (fun msg ->
            match msg with
            | MainModel.Msg.PlayerMsg (
                PlayerMsg.LooperMsg (
                    LooperEvent.TimePointStarted (newTp, oldTp)
                )) when newTp = newTimePoint && oldTp = oldTimePoint -> true
            | _ -> false
        )
    }
    |> Scenario.log "Then.``Finish of LoadRecipeCards has been dispatched``"
