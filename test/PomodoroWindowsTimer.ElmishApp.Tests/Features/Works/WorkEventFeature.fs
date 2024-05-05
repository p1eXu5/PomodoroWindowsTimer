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


[<Category("UC5: Work Events Scenarios")>]
module WorkEventFeature =

    [<Test>]
    let ``UC5-1: Initialization Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! Then.``Current Work has been set to`` currentWork
            do! Then.``Have no work events in db within`` 1UL // newly created work Id
        }
        |> Scenario.runTestAsync

    [<Test>]
    let ``UC5-2: Start Work adds WorkStarted event Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``Play msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! Then.``Current Work has been set to`` currentWork
            do! Then.``Work events in db exist`` 1UL [ <@ WorkEvent.WorkStarted @> ] // newly created work Id
        }
        |> Scenario.runTestAsync

    [<Test>]
    let ``UC5-3: Moving time slider when Initialized -> does not add events Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! Then.``Current Work has been set to`` currentWork
            do! Then.``Have no work events in db within`` 1UL // newly created work Id
        }
        |> Scenario.runTestAsync

    [<Test>]
    let ``UC5-4: Moving time slider when Playing -> adds events Scenario`` () =
        scenario {
            let timePoints = [ workTP 10.0<sec>; breakTP ``3 sec`` ]
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``Play msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 5.5<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! Then.``Current Work has been set to`` currentWork
            do! Then.``Work events in db exist`` 1UL [ // newly created work Id
                <@@ WorkEvent.WorkStarted @@>
                <@@ WorkEvent.Stopped @@>
                <@@ WorkEvent.WorkStarted @@>
            ]
        }
        |> Scenario.runTestAsync

    // ---------------------
    // change work scenarios
    // ---------------------
    [<Test>]
    let ``UC5-5: Current Work set -> new Work is created and selected -> does not add events`` () =
        scenario {
            let timePoints = [ workTP 10.0<sec>; breakTP ``3 sec`` ]
            let! _ =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``WorkSelector drawer is opening`` ()
            do! When.``WorkList sub model has been shown`` 1<times>
            do! When.``WorkListModel CreateWork msg has been dispatched`` ()
            do! When.``CreatingWork sub model has been shown`` ()

            let work2 = generateWork ()
            do! When.``SetNumber msg has been dispatched with`` work2.Number
            do! When.``SetTitle msg has been dispatched with`` work2.Title
            do! When.``CreatingWorkModel CreateWork msg has been dispatched`` ()
            do! When.``WorkList sub model has been shown`` 2<times>
            do! When.``WorkSelector drawer is closing`` ()

            do! Then.``MainModel WorkSelector becomes None`` ()
            do! Then.``Current Work has been set to`` work2
            do! Then.``Have no work events in db within`` 1UL // first newly created work Id
            do! Then.``Have no work events in db within`` 2UL // second newly created work Id
        }
        |> Scenario.runTestAsync

    [<Test>]
    let ``UC5-6: Current Work set -> Playing -> new Work is created and selected -> adds events`` () =
        scenario {
            let timePoints = [ workTP 10.0<sec>; breakTP ``3 sec`` ]
            let! _ =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``Play msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! When.``WorkSelector drawer is opening`` ()
            do! When.``WorkList sub model has been shown`` 1<times>
            do! When.``WorkListModel CreateWork msg has been dispatched`` ()
            do! When.``CreatingWork sub model has been shown`` ()

            let work2 = generateWork ()
            do! When.``SetNumber msg has been dispatched with`` work2.Number
            do! When.``SetTitle msg has been dispatched with`` work2.Title
            do! When.``CreatingWorkModel CreateWork msg has been dispatched`` ()
            do! When.``WorkList sub model has been shown`` 2<times>
            do! When.``WorkSelector drawer is closing`` ()

            do! Then.``MainModel WorkSelector becomes None`` ()
            do! Then.``Current Work has been set to`` work2
            do! Then.``Work events in db exist`` 1UL [ // first newly created work Id
                <@@ WorkEvent.WorkStarted @@>
                <@@ WorkEvent.Stopped @@>
            ]
            do! Then.``Work events in db exist`` 2UL [ // first newly created work Id
                <@@ WorkEvent.WorkStarted @@>
            ]
        }
        |> Scenario.runTestAsync

