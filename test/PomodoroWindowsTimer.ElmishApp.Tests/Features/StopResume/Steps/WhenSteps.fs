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


let rec ``Play msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Play |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Play" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Play msg has been dispatched``}``"

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

let rec ``Next msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Next |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Next" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Next msg has been dispatched``}``"

let rec ``Replay msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.Replay |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Replay" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``Replay msg has been dispatched``}``"

let rec ``PreChangeActiveTimeSpan msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.PreChangeActiveTimeSpan |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PreChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``PreChangeActiveTimeSpan msg has been dispatched``}``"

let rec ``PostChangeActiveTimeSpan msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.PostChangeActiveTimeSpan |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PostChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``PostChangeActiveTimeSpan msg has been dispatched``}``"


let rec ``SetCurrentWorkIfNone msg has been dispatched with`` (expectedWork: Work) =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "SetCurrentWorkIfNone" (fun msg ->
            match msg with
            | MainModel.Msg.SetCurrentWorkIfNone (Ok work) ->
                work.Number = expectedWork.Number && work.Title = expectedWork.Title
            | _ -> false
        )
    }
    |> Scenario.log $"When.``{nameof ``SetCurrentWorkIfNone msg has been dispatched with``} {expectedWork}``"


let rec ``ActiveTimeSeconds changed to`` (seconds: float<sec>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.ControllerMsg.ChangeActiveTimeSpan (float seconds) |> MainModel.Msg.ControllerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "PostChangeActiveTimeSpan" ((=) msg)
    }
    |> Scenario.log $"When.``{nameof ``ActiveTimeSeconds changed to``} {seconds} sec``."
