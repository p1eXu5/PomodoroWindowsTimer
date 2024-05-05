module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.When

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
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



let rec ``Stop msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Stop |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Stop" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Stop msg has been dispatched``}``"

let rec ``Resume msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Resume |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Resume" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Resume msg has been dispatched``}``"

let rec ``Next msg has been dispatched with 2.5 ticks timeout`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Next |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2SecT "Next" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Next msg has been dispatched with 2.5 ticks timeout``}``"

let rec ``Replay msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Replay |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Replay" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Replay msg has been dispatched``}``"

