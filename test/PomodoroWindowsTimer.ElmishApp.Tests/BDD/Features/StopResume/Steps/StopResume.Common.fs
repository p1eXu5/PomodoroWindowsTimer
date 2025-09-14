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


let ``Looper TimePointReduced event has been despatched with`` (timePointId: System.Guid) (expectedSeconds: float<sec>) (tolerance: float<sec>) =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "TimePointReduced" (fun msg ->
            match msg with
            | MainModel.Msg.LooperMsg (LooperEvent.TimePointTimeReduced ({ ActiveTimePoint = atp }, _)) ->
                atp.OriginalId = timePointId
                && (
                    float (expectedSeconds - tolerance) <= atp.RemainingTimeSpan.TotalSeconds
                    && atp.RemainingTimeSpan.TotalSeconds <= float (expectedSeconds + tolerance)
                )

            | _ -> false
        )
    }


