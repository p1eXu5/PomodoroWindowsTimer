[<RequireQualifiedAccess>]
module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.Common

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let ``Looper TimePointStarted event has been despatched with`` (newTimePoint: TimePoint) (oldTimePoint: TimePoint option) =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "TimePointStarted" (fun msg ->
            match msg with
            | MainModel.Msg.PlayerMsg (
                PlayerMsg.LooperMsg (
                    LooperEvent.TimePointStarted (newTp, oldTp)
                )) when newTp.Id = newTimePoint.Id ->
                    match oldTp, oldTimePoint with
                    | None, None -> true
                    | Some t1, Some t2 -> t1.Id = t2.Id
                    | _ -> false
            | _ -> false
        )
    }
    |> Scenario.log "When.``Looper TimePointStarted event has been despatched with``"

let ``Looper TimePointReduced event has been despatched with`` (activeTimePointId: System.Guid) (expectedSeconds: float<sec>)  =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "TimePointReduced" (fun msg ->
            match msg with
            | MainModel.Msg.PlayerMsg (
                PlayerMsg.LooperMsg (
                    LooperEvent.TimePointTimeReduced tp
                )) ->
                    tp.Id = activeTimePointId
                    && tp.TimeSpan.TotalSeconds = float expectedSeconds
            | _ -> false
        )
    }
    |> Scenario.log "When.``Looper TimePointReduced event has been despatched with``"


