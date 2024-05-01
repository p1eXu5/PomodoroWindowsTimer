module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.When

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let ``Looper TimePointStarted event has been despatched with`` (newTimePoint: TimePoint) (oldTimePoint: TimePoint option) =
    Common.``Looper TimePointStarted event has been despatched with`` newTimePoint oldTimePoint

let ``Looper TimePointReduced event has been despatched with`` (activeTimePointId: System.Guid) (expectedSeconds: float<sec>)  =
    Common.``Looper TimePointReduced event has been despatched with`` activeTimePointId expectedSeconds


let ``Play msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Play |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Play" ((=) msg)
    }

let ``Stop msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Stop |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Stop" ((=) msg)
    }

let ``Resume msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Resume |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Resume" ((=) msg)
    }

let ``Next msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Next |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Next" ((=) msg)
    }

let ``Replay msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Replay |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Replay" ((=) msg)
    }

let ``PreChangeActiveTimeSpan msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.PreChangeActiveTimeSpan |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PreChangeActiveTimeSpan" ((=) msg)
    }

let ``PostChangeActiveTimeSpan msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.PostChangeActiveTimeSpan |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PostChangeActiveTimeSpan" ((=) msg)
    }

let ``ActiveTimeSeconds changed to`` (seconds: float<sec>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.ChangeActiveTimeSpan (float seconds) |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PostChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log "When.``ActiveTimeSeconds changed to``"
