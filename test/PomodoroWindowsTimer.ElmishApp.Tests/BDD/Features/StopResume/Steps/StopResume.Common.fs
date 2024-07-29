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


let ``Looper TimePointReduced event has been despatched with`` (activeTimePointId: System.Guid) (expectedSeconds: float<sec>) (tolerance: float<sec>) =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "TimePointReduced" (fun msg ->
            match msg with
            | MainModel.Msg.LooperMsg (LooperMsg.TimePointTimeReduced tp) ->
                tp.Id = activeTimePointId
                && (
                    float (expectedSeconds - tolerance) <= tp.TimeSpan.TotalSeconds
                    && tp.TimeSpan.TotalSeconds <= float (expectedSeconds + tolerance)
                )

            | _ -> false
        )
    }


