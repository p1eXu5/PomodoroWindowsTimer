[<RequireQualifiedAccess>]
module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.Common

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
        do! Scenario.msgDispatchedWithin2Sec "TimePointStarted" (fun msg ->
            match msg with
            | MainModel.Msg.PlayerMsg (
                PlayerMsg.LooperMsg (
                    LooperEvent.TimePointStarted (newTp, oldTp)
                )) when newTp.Id = newTimePointId ->
                    match oldTp, oldTimePointId with
                    | None, None -> true
                    | Some t1, Some t2 -> t1.Id = t2
                    | _ -> false
            | _ -> false
        )
    }

let ``Looper TimePointReduced event has been despatched with`` (activeTimePointId: System.Guid) (expectedSeconds: float<sec>) (tolerance: float<sec>) =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "TimePointReduced" (fun msg ->
            match msg with
            | MainModel.Msg.PlayerMsg (
                PlayerMsg.LooperMsg (
                    LooperEvent.TimePointTimeReduced tp
                )) ->
                        tp.Id = activeTimePointId
                        && (
                           float (expectedSeconds - tolerance) <= tp.TimeSpan.TotalSeconds
                           && tp.TimeSpan.TotalSeconds <= float (expectedSeconds + tolerance)
                        )

            | _ -> false
        )
    }
  


