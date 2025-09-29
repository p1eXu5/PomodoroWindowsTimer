module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.Common

open System

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let ``Looper TimePointStarted event has been dispatched with`` (newTimePointId: Guid) (oldTimePointId: Guid option) =
    scenario {
        do! Scenario.msgDispatchedWithin 3.0<sec> "TimePointStarted" (fun msg ->
            match msg with
            | MainModel.Msg.LooperMsg (
                LooperEvent.TimePointStarted ({ NewActiveTimePoint = newTp; OldActiveTimePoint = oldTp}, _))
                    when newTp.OriginalId = newTimePointId
                ->
                match oldTp, oldTimePointId with
                | None, None -> true
                | Some t1, Some t2 -> t1.OriginalId = t2
                | _ -> false
            | _ -> false
        )
    }
    |> Scenario.log "Common.``Looper TimePointStarted event has been dispatched with``"

let ``Looper TimePointReady event has been dispatched with`` (timePointId: Guid) =
    scenario {
        do! Scenario.msgDispatchedWithin 3.0<sec> "TimePointReady" (fun msg ->
            match msg with
            | MainModel.Msg.LooperMsg (
                LooperEvent.TimePointReady (atp, _))
                    when atp.OriginalId = timePointId
                ->
                true
            | _ -> false
        )
    }
    |> Scenario.log "Common.``Looper TimePointReady event has been dispatched with``"
