module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.When

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let ``Looper TimePointStarted event has been despatched with`` (newTimePointId: Guid) (oldTimePointId: Guid option) =
    Common.``Looper TimePointStarted event has been despatched with`` newTimePointId oldTimePointId
    |> Scenario.log (
        sprintf "When.``%s new TimePoint Id - %A and %s``."
            (nameof Common.``Looper TimePointStarted event has been despatched with``)
            newTimePointId
            (oldTimePointId |> Option.map (sprintf "old TimmePoint Id - %A") |> Option.defaultValue "no old TimePoint")
        )

let ``Looper TimePointReduced event has been despatched with`` (activeTimePointId: System.Guid) (expectedSeconds: float<sec>) tolerance =
    Common.``Looper TimePointReduced event has been despatched with`` activeTimePointId expectedSeconds tolerance
    |> Scenario.log (
        sprintf "When.``%s %A %A sec with %A tolerance``."
            (nameof Common.``Looper TimePointReduced event has been despatched with``)
            activeTimePointId
            expectedSeconds
            tolerance
        )


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

let rec ``PreChangeActiveTimeSpan msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.PreChangeActiveTimeSpan |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PreChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``PreChangeActiveTimeSpan msg has been dispatched``}``"

let rec ``PostChangeActiveTimeSpan msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.PostChangeActiveTimeSpan |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PostChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``PostChangeActiveTimeSpan msg has been dispatched``}``"

let rec ``ActiveTimeSeconds changed to`` (seconds: float<sec>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.ChangeActiveTimeSpan (float seconds) |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PostChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``ActiveTimeSeconds changed to``} {seconds} sec``."
