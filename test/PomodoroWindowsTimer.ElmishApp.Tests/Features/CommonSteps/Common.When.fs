module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.When

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features

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
        let msg = MainModel.ControllerMsg.Play |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Play" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Play msg has been dispatched with 2.5 ticks timeout``}``"

let rec ``Play msg has been dispatched`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Play |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.Dispatch(msg)
        do! Scenario.msgDispatchedWithin2SecT "Play" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Play msg has been dispatched``}``"


let rec ``Stop msg has been dispatched with 2.5 ticks timeout`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Stop |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Stop" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Stop msg has been dispatched with 2.5 ticks timeout``}``"

let rec ``Stop msg has been dispatched`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Stop |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.Dispatch(msg)
        do! Scenario.msgDispatchedWithin2SecT "Stop" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Stop msg has been dispatched with 2.5 ticks timeout``}``"


let rec ``Resume msg has been dispatched with 2.5 ticks timeout`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Resume |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Resume" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Resume msg has been dispatched with 2.5 ticks timeout``}``"

let rec ``Resume msg has been dispatched`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Resume |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.Dispatch(msg)
        do! Scenario.msgDispatchedWithin2SecT "Resume" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Resume msg has been dispatched``}``"


let rec ``Next msg has been dispatched`` (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Next |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.Dispatch(msg)
        do! Scenario.msgDispatchedWithin2SecT "Next" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Next msg has been dispatched``}``"

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


let rec ``PreChangeActiveTimeSpan msg has been dispatched`` times =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.PreChangeActiveTimeSpan |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2SecT "PreChangeActiveTimeSpan" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``PreChangeActiveTimeSpan msg has been dispatched``}``"

let rec ``PostChangeActiveTimeSpan msg has been dispatched`` times =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.PostChangeActiveTimeSpan |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2SecT "PostChangeActiveTimeSpan" times ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``PostChangeActiveTimeSpan msg has been dispatched``}``"

let rec ``ActiveTimeSeconds changed to`` (seconds: float<sec>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.ChangeActiveTimeSpan (float seconds) |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PostChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``ActiveTimeSeconds changed to``} {seconds} sec``."


