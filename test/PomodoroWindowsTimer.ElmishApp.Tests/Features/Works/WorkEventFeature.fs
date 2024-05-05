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
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps


[<Category("5: Work Events Scenarios")>]
module WorkEventFeature =

    [<Test>]
    let ``UC5-1 - Initialization Work TimePoint Scenario`` () =
        scenario {
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` ()

            do! Then.``Current Work has been set to`` currentWork
            do! Then.``Have no work events in db within`` 1UL // newly created work Id
        }
        |> Scenario.runTestAsync

    [<Test>]
    let ``UC5-2 - Start Work adds WorkStarted event Scenario`` () =
        scenario {
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` ()

            do! When.``Play msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! Then.``Current Work has been set to`` currentWork
            do! Then.``Work events in db exist`` 1UL [ <@ WorkEvent.WorkStarted @> ] // newly created work Id
        }
        |> Scenario.runTestAsync

