namespace PomodoroWindowsTimer.ElmishApp.Tests.Features

open System
open NUnit.Framework
open PomodoroWindowsTimer.Testing.Fakers

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps
open PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps


module WorkEventFeature =

    [<Test>]
    [<Category("Work Events Scenarios")>]
    let ``UC4-1 - Initialization Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let currentWork = generateWork ()

            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Stored CurrentWork`` currentWork
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``SetCurrentWorkIfNone msg has been dispatched with`` currentWork

            do! Then.``Current Work is set on`` currentWork
            do! Then.``Have no work events in db within`` 1UL // newly created work Id
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Work Events Scenarios")>]
    let ``UC4-2 - Start Work adds WorkStarted event Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let currentWork = generateWork ()

            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Stored CurrentWork`` currentWork
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``SetCurrentWorkIfNone msg has been dispatched with`` currentWork
            do! When.``Play msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! Then.``Current Work is set on`` currentWork
            do! Then.``Work events in db exist`` 1UL [ <@ WorkEvent.WorkStarted @> ] // newly created work Id
        }
        |> Scenario.runTestAsync

