namespace PomodoroWindowsTimer.ElmishApp.Tests.Features.Works

open System
open NUnit.Framework
open PomodoroWindowsTimer.Testing.Fakers

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.Features
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps
open PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps



module TimeSliderFeature =

    [<Test>]
    [<Category("UC6: Time slider Initial Scenarios, Work is set")>]
    let ``UC6-0 - Initial break time point move forward when work is set and time skipping scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Work in WorkRepository and UserSettings`` ()
            do! Given.``WorkEventStore substitution`` ()
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` 1<times>
            do! When.``Active time point slider value is changing to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan Start msg has been dispatched`` 1<times>

            do! Then.``SkipOrApplyMissingTime dialog has been shown`` ()

            do! When.``User skips time`` ()

            do! Then.``Dialog has been closed`` ()
            do! Then.``No event have been storred (with mock)`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC6: Time slider Initial Scenarios, Work is set")>]
    let ``UC6-1 - Initial break time point move forward when work is set and time applying scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Work in WorkRepository and UserSettings`` ()
            do! Given.``WorkEventStore substitution`` ()
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` 1<times>
            do! When.``Active time point slider value is changing to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan Start msg has been dispatched`` 1<times>

            do! Then.``SkipOrApplyMissingTime dialog has been shown`` ()

            do! When.``User applies time as break`` ()

            do! Then.``Dialog has been closed`` ()
            do! Then.``BreakIncreased event has been storred (with mock)`` "CurrentWork" 1.5<sec>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC6: Time slider Initial Scenarios, Work is set")>]
    let ``UC6-3 - Initial work time point move forward when work is set and time skipping scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Work in WorkRepository and UserSettings`` ()
            do! Given.``WorkEventStore substitution`` ()
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` 1<times>
            do! When.``Active time point slider value is changing to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan Start msg has been dispatched`` 1<times>

            do! Then.``SkipOrApplyMissingTime dialog has been shown`` ()

            do! When.``User skips time`` ()

            do! Then.``Dialog has been closed`` ()
            do! Then.``No event have been storred (with mock)`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC6: Time slider Initial Scenarios, Work is set")>]
    let ``UC6-4 - Initial work time point move forward when work is set and time applying scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Work in WorkRepository and UserSettings`` ()
            do! Given.``WorkEventStore substitution`` ()
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` 1<times>
            do! When.``Active time point slider value is changing to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan Start msg has been dispatched`` 1<times>

            do! Then.``SkipOrApplyMissingTime dialog has been shown`` ()

            do! When.``User applies time as work`` ()

            do! Then.``Dialog has been closed`` ()
            do! Then.``WorkIncreased event has been storred (with mock)`` "CurrentWork" 1.5<sec>
        }
        |> Scenario.runTestAsync



    [<Test>]
    [<Category("UC6: Time slider Initial Scenarios, Work is set")>]
    let ``UC6-5 - Initial break time point move forward with skipping then backward when work is set scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` 1<times>
            do! When.``Active time point slider value is changing to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan Start msg has been dispatched`` 1<times>
            do! When.``SkipOrApplyMissingTime dialog has been shown`` ()
            do! When.``User skips time`` ()
            do! When.``Dialog has been closed`` ()

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` 2<times>
            do! When.``Active time point slider value is changing to`` 0.0<sec>
            do! When.``PostChangeActiveTimeSpan Start msg has been dispatched`` 2<times>

            do! Then.``No dialog has been shown`` ()
            do! Then.``Have no work events in db within`` currentWork.Id
            do! Then.``No errors are emitted`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC6: Time slider Initial Scenarios, Work is set")>]
    let ``UC6-6 - Initial break time point move forward with applying then backward leaves time as break when work is set scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            let! currentWork =
                Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` 1<times>
            do! When.``Active time point slider value is changing to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan Start msg has been dispatched`` 1<times>
            do! When.``SkipOrApplyMissingTime dialog has been shown`` ()
            do! When.``User applies time as work`` ()
            do! When.``Dialog has been closed`` ()

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` 2<times>
            do! When.``Active time point slider value is changing to`` 0.0<sec>
            do! When.``PostChangeActiveTimeSpan Start msg has been dispatched`` 2<times>

            do! Then.``RollbackWork dialog has been shown`` ()

            do! When.``User leaves time as break`` ()
            do! Then.``Dialog has been closed`` ()
            
            do! Then.``Work events in db exist`` currentWork.Id [ <@@ WorkEvent.BreakIncreased @@> ]
            do! Then.``No errors are emitted`` ()
        }
        |> Scenario.runTestAsync


