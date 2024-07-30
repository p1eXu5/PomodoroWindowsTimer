module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.When

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features
open Elmish.Extensions

let rec ``Spent 2.5 ticks`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.WaitTimeout()
    }
    |> Scenario.log $"When.``{nameof ``Spent 2.5 ticks``}``"

let rec ``Spent`` (seconds: float<sec>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        do sut.Dispatcher.WaitTimeout(seconds)
    }
    |> Scenario.log $"When.``{nameof ``Spent``} {seconds} sec``"


let ``Looper TimePointStarted event has been despatched with`` (newTimePointId: Guid) (oldTimePointId: Guid option) =
    Common.``Looper TimePointStarted event has been despatched with`` newTimePointId oldTimePointId
    |> Scenario.log (
        sprintf "When.``%s new TimePoint Id - %A and %s``."
            (nameof Common.``Looper TimePointStarted event has been despatched with``)
            newTimePointId
            (oldTimePointId |> Option.map (sprintf "old TimmePoint Id - %A") |> Option.defaultValue "no old TimePoint")
        )

let rec ``Play msg has been dispatched with 2.5 ticks timeout`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Play |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Play" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Play msg has been dispatched with 2.5 ticks timeout``}``"

let rec ``Play msg has been dispatched`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Play |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.Dispatch(msg)
        do! Scenario.msgDispatchedWithin2SecT "Play" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Play msg has been dispatched``}``"


let rec ``Stop msg has been dispatched with 2.5 ticks timeout`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Stop |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Stop" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Stop msg has been dispatched with 2.5 ticks timeout``}``"

let rec ``Stop msg has been dispatched`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Stop |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.Dispatch(msg)
        do! Scenario.msgDispatchedWithin2SecT "Stop" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Stop msg has been dispatched with 2.5 ticks timeout``}``"


let rec ``Resume msg has been dispatched with 2.5 ticks timeout`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Resume |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Resume" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Resume msg has been dispatched with 2.5 ticks timeout``}``"

let rec ``Resume msg has been dispatched`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Resume |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.Dispatch(msg)
        do! Scenario.msgDispatchedWithin2SecT "Resume" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Resume msg has been dispatched``}``"


let rec ``Next msg has been dispatched`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Next |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.Dispatch(msg)
        do! Scenario.msgDispatchedWithin2SecT "Next" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Next msg has been dispatched``}``"

let rec ``Next msg has been dispatched with 2.5 ticks timeout`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Next |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2SecT "Next" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Next msg has been dispatched with 2.5 ticks timeout``}``"

let rec ``Replay msg has been dispatched with 2.5 ticks timeout`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.Replay |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Replay" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Replay msg has been dispatched with 2.5 ticks timeout``}``"

/// Dispatches StartTimePoint Start msg and waits StartTimePoint Finish msg.
let rec ``StartTimePoint msg has been dispatched with 2.5 ticks timeout`` (timePointId: Guid) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.Msg.StartTimePoint timePointId
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec $"Start StartTimePoint {timePointId}" ((=) msg)
        do!
            Scenario.msgDispatchedWithin2Sec
                $"Finish StartTimePoint {timePointId}"
                ((=) (MainModel.Msg.PlayerModelMsg (PlayerModel.Msg.StartTimePoint (Operation.Finish ()))))
    }
    |> Scenario.log $"When.``{nameof ``Play msg has been dispatched with 2.5 ticks timeout``}``"


let rec ``PreChangeActiveTimeSpan msg has been dispatched`` times =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.PreChangeActiveTimeSpan |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2SecT "PreChangeActiveTimeSpan" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``PreChangeActiveTimeSpan msg has been dispatched``}``"

let rec ``PostChangeActiveTimeSpan msg has been dispatched`` times =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.PostChangeActiveTimeSpan (Start ()) |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2SecT "PostChangeActiveTimeSpan" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``PostChangeActiveTimeSpan msg has been dispatched``}``"

let rec ``ActiveTimeSeconds changed to`` (seconds: float<sec>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = PlayerModel.Msg.ChangeActiveTimeSpan (float seconds) |> MainModel.Msg.PlayerModelMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PostChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``ActiveTimeSeconds changed to``} {seconds} sec``."


