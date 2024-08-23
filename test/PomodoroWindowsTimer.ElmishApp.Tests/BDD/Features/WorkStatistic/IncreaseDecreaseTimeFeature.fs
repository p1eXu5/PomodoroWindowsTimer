namespace PomodoroWindowsTimer.ElmishApp.Tests.BDD.Features.WorkStatistic

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
open PomodoroWindowsTimer.ElmishApp.Tests.BDD.Features.WorkStatistic.Steps

[<Category("UC7: Increase/Decrease Work Time Scenarios")>]
module IncreaseDecreaseTimeFeature =

    [<Test>]
    let ``UC7-01: Existing work started event, when openning daily statistic, work daily statistic is presenting`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! When.``User opens work statistic window`` ()

            do! Then.``Work daily statistic is shown`` currentWork.Id
        }
        |> Scenario.runTestAsync


    [<Test>]
    let ``UC7-02: Existing work started event, can increase work time`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! When.``User opens work statistic window`` ()
            do! When.``Work daily statistic is shown`` currentWork.Id

            // do! When.``User opens AddWorkTime dialog`` ()
            // do! When.``User set work time to`` (TimeSpan.FromMinutes(10))
            // do! When.
        }
        |> Scenario.runTestAsync


