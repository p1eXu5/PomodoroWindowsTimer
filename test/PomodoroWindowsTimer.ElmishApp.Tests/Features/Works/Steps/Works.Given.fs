module PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps.Given

open System
open NUnit.Framework
open PomodoroWindowsTimer.Testing.Fakers

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps

let ``Program has been initialized without CurrentWork`` (timePoints: TimePoint list) =
    scenario {

        do! Given.``Stored TimePoints`` timePoints
        do! Given.``Initialized Program`` ()

        do! Common.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None

        return ()
    }

let ``Program has been initialized with CurrentWork`` (timePoints: TimePoint list) =
    scenario {
        let currentWork = generateWork ()

        do! Given.``Stored TimePoints`` timePoints
        do! Given.``Stored CurrentWork`` currentWork
        do! Given.``Initialized Program`` ()

        do! Common.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
        do! Common.``SetCurrentWorkIfNone msg has been dispatched with`` currentWork

        return currentWork
    }