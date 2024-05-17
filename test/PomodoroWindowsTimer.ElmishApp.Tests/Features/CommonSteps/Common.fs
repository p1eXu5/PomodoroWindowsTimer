module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.Common

open System
open FsUnit
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers
open p1eXu5.FSharp.Testing.ShouldExtensions

let ``Looper TimePointStarted event has been despatched with`` (newTimePointId: Guid) (oldTimePointId: Guid option) =
    scenario {
        do! Scenario.msgDispatchedWithin 3.0<sec> "TimePointStarted" (fun msg ->
            match msg with
            | MainModel.Msg.ControllerMsg (
                ControllerMsg.LooperMsg (
                    LooperEvent.TimePointStarted (newTp, oldTp)
                )) when newTp.Id = newTimePointId ->
                    match oldTp, oldTimePointId with
                    | None, None -> true
                    | Some t1, Some t2 -> t1.Id = t2
                    | _ -> false
            | _ -> false
        )
    }
