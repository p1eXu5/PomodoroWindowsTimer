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


module TimeSliderFeature =

    [<Test>]
    [<Category("UC2: Time slider Initial Scenarios")>]
    let ``UC2-0 - Initial time point move forward scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()

            do! Then.``Looper TimePointReduced event has been despatched with`` timePoints.Head.Id 1.5<sec> 0.0<sec>
            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active TimePoint remaining time is equal to`` 1.5<sec>
            do! Then.``LooperState is`` LooperState.Initialized
            do! Then.``MainModel.IsMinimized should be`` false // we just started app and move slider
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC2: Time slider Initial Scenarios")>]
    let ``UC2-1 - Initial time point move forward then backward scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 1.5<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()
            
            do! When.``Looper TimePointReduced event has been despatched with`` timePoints.Head.Id 1.5<sec> 0.0<sec>
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 0.0<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()

            do! Then.``Looper TimePointReduced event has been despatched with`` timePoints.Head.Id 3.0<sec> 0.0<sec>
            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active TimePoint remaining time is equal to`` 3.0<sec>
            do! Then.``LooperState is`` LooperState.Initialized
            do! Then.``MainModel.IsMinimized should be`` false
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC2: Time slider Initial Scenarios")>]
    let ``UC2-2 - Initial Break time point move forward to the end then play scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP 10.0<sec> ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 3.0<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()

            do! When.``Looper TimePointReduced event has been despatched with`` timePoints.Head.Id 0.0<sec> 0.0<sec>
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints[1].Id (timePoints[0].Id |> Some)
            do! Then.``MinimizeWindows msg has not been dispatched`` ()
            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``MainModel.IsMinimized should be`` false // switched to the next Work tp
            do! Then.``Telegrtam bot should be notified with`` timePoints[1].Name
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC2: Time slider Initial Scenarios")>]
    let ``UC2-3 - Initial Work time point move forward to the end then play scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP 10.0<sec>;  ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 3.0<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()

            do! When.``Looper TimePointReduced event has been despatched with`` timePoints.Head.Id 0.0<sec> 0.0<sec>
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints[1].Id (timePoints[0].Id |> Some)
            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``MainModel.IsMinimized should be`` true
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    // ---------------------------------

    [<Test>]
    [<Category("UC3: Time slider Playing Scenarios")>]
    let ``UC3-0 - Playing time point move forward scenario`` () =
        scenario {
            let timePoints = [ workTP 5.0<sec>; breakTP ``3 sec``;  ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 2.0<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()


            do! Then.``Looper TimePointReduced event has been despatched with`` timePoints.Head.Id 2.0<sec> 0.25<sec>
            do! Then.``Active Point is set on`` timePoints[0]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[0] None
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC3: Time slider Playing Scenarios")>]
    let ``UC3-1 - Playing time point move backward scenario`` () =
        scenario {
            let timePoints = [ workTP 10.0<sec>; breakTP ``3 sec``;  ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 2.0<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()
            
            do! When.``Looper TimePointReduced event has been despatched with`` timePoints.Head.Id 8.0<sec> 0.5<sec>
            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 0.0<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()

            do! Then.``Looper TimePointReduced event has been despatched with`` timePoints.Head.Id 10.0<sec> 0.0<sec>
            do! Then.``Active Point is set on`` timePoints[0]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[0] None
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC3: Time slider Playing Scenarios")>]
    let ``UC3-2 - Playing time point move forward to the end, transition to the next time point scenario`` () =
        scenario {
            let timePoints = [ workTP 5.0<sec>; breakTP ``3 sec``;  ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! When.``PreChangeActiveTimeSpan msg has been dispatched`` ()
            do! When.``ActiveTimeSeconds changed to`` 5.0<sec>
            do! When.``PostChangeActiveTimeSpan msg has been dispatched`` ()

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints[1].Id (timePoints[0].Id |> Some)
            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync
