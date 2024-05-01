module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.When

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features

let ``Spent 2.5 ticks`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.WaitTimeout()
    }
    |> Scenario.log "Given.``Initialized Program``"

