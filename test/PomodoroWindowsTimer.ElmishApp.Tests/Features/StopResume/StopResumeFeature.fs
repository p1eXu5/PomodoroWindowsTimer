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


module StopResumeFeature =

    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-1 - Initialization Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Initialized
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Work 1<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-2 - Start Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head None
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Work 2<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-3 - Start-Stop Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Stop msg has been dispatched with 2.5 ticks timeout`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head None
            do! Then.``LooperState is`` LooperState.Stopped
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Work 2<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-4 - Start-Stop-Resume Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Stop msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Resume msg has been dispatched with 2.5 ticks timeout`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head None
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Work 2<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-5 - Start Work -> Next to Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Next msg has been dispatched with 2.5 ticks timeout`` 1<times>

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints[1].Id (timePoints[0].Id |> Some)
            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Work 2<times>
            do! Then.``Theme should been switched with`` TimePointKind.Break 1<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-6 - Start Work -> Stop -> Next to Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Stop msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Next msg has been dispatched with 2.5 ticks timeout`` 1<times>

            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Work 2<times>
            do! Then.``Theme should been switched with`` TimePointKind.Break 1<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-7 - Work to Break TimePoint transition Scenario`` () =
        scenario {
            let timePoints = [ workTP ``0.5 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[1].Id (timePoints[0].Id |> Some)

            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Work 2<times>
            do! Then.``Theme should been switched with`` TimePointKind.Break 1<times>
        }
        |> Scenario.runTestAsync

    // TODO: Failed - need to implement PlayerModel.Msg.PostChangeActiveTimeSpan handler.
    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-8 - Start-Replay Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP 10.0<sec>; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Replay msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.Spent 1.0<sec>

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is less then`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``MainModel.IsMinimized should be`` false
            do! Then.``WindowsMinimizer.MinimizeOtherAsync is called`` 0
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Work 2<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC0: Work TimePoint Scenarios")>]
    let ``UC0-9 - Start Work -> Next to Break -> Next to Work -> LongBreak transition Scenario`` () =
        scenario {
            let timePoints = [ namedWorkTP "Work 1" 4.<sec>; breakTP 4.<sec>; namedWorkTP "Work 2" 2.<sec>; longBreakTP 10.<sec>; ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` () // Work
            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None

            do! When.``Next msg has been dispatched with 2.5 ticks timeout`` 1<times> // to Break
            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[1].Id (timePoints[0].Id |> Some)

            do! When.``Next msg has been dispatched with 2.5 ticks timeout`` 2<times>  // to Work
            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[2].Id (timePoints[1].Id |> Some)

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[3].Id (timePoints[2].Id |> Some) // Wait LongBreak
            do! Then.``Active Point is set on`` timePoints[3]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[3] (1.0<sec> |> Some) // reminder can be appended to the next tp
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``MainModel.IsMinimized should be`` true
            do! Then.``Theme should been switched with`` TimePointKind.Work 3<times>
            do! Then.``Theme should been switched with`` TimePointKind.Break 2<times>
        }
        |> Scenario.runTestAsync

    // -------
    //  Break
    // -------

    [<Test>]
    [<Category("UC1: Break TimePoint Scenarios")>]
    let ``UC1-1 - Initialization Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Initialized
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Break 1<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC1: Break TimePoint Scenarios")>]
    let ``UC1-2 - Start Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Break 2<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC1: Break TimePoint Scenarios")>]
    let ``UC1-3 - Start-Stop Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Stop msg has been dispatched with 2.5 ticks timeout`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head None
            do! Then.``LooperState is`` LooperState.Stopped
            // TODO: do! Then.``MainModel.IsMinimized should be`` false
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Break 2<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC1: Break TimePoint Scenarios")>]
    let ``UC1-4 - Start-Stop-Resume Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Stop msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Resume msg has been dispatched with 2.5 ticks timeout`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Break 2<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC1: Break TimePoint Scenarios")>]
    let ``UC1-5 - Start Break Next Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Next msg has been dispatched with 2.5 ticks timeout`` 1<times>

            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``MainModel.IsMinimized should be`` false
            // TODO: do! Then.``Telegrtam bot should be notified with`` timePoints[1].Name
            do! Then.``Theme should been switched with`` TimePointKind.Break 2<times>
            do! Then.``Theme should been switched with`` TimePointKind.Work 1<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC1: Break TimePoint Scenarios")>]
    let ``UC1-6 - Start Break -> Stop -> Next Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Stop msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Next msg has been dispatched with 2.5 ticks timeout`` 1<times>

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints[1].Id (timePoints[0].Id |> Some)
            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``MainModel.IsMinimized should be`` false
            // TODO: do! Then.``Telegrtam bot should be notified with`` timePoints[1].Name
            do! Then.``Theme should been switched with`` TimePointKind.Break 2<times>
            do! Then.``Theme should been switched with`` TimePointKind.Work 1<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC1: Break TimePoint Scenarios")>]
    let ``UC1-7 - Break to Work TimePoint transition Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``0.5 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Spent 2.5 ticks`` ()
            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[1].Id (timePoints[0].Id |> Some)


            // TODO: do! Then.``Telegrtam bot should be notified with`` timePoints[1].Name
            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1] None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``MainModel.IsMinimized should be`` false
            do! Then.``Theme should been switched with`` TimePointKind.Break 2<times>
            do! Then.``Theme should been switched with`` TimePointKind.Work 1<times>
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("UC1: Break TimePoint Scenarios")>]
    let ``UC1-8 - Start-Replay Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
            do! When.``Play msg has been dispatched with 2.5 ticks timeout`` ()
            do! When.``Replay msg has been dispatched with 2.5 ticks timeout`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head None
            do! Then.``LooperState is`` LooperState.Playing
            // TODO: do! Then.``MainModel.IsMinimized should be`` true
            do! Then.``Telegrtam bot should not be notified`` ()
            do! Then.``Theme should been switched with`` TimePointKind.Break 2<times>
        }
        |> Scenario.runTestAsync
