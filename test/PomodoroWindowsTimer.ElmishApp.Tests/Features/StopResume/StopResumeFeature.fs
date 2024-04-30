namespace PomodoroWindowsTimer.ElmishApp.Tests.Features

open NUnit.Framework

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.Fakers
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps
open PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps


module StopResumeFeature =

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC0-1 - Initialization Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Initialized
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC0-2 - Start Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()
            do! When.``Spent 2.5 ticks`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC0-3 - Start-Stop Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()
            do! When.``Stop msg has been dispatched`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Stopped
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC0-4 - Start-Stop-Resume Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()
            do! When.``Stop msg has been dispatched`` ()
            do! When.``Resume msg has been dispatched`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC0-5 - Start Work Next Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()
            do! When.``Next msg has been dispatched`` ()

            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1]
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC0-6 - Start-Stop-Next Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()
            do! When.``Stop msg has been dispatched`` ()
            do! When.``Next msg has been dispatched`` ()

            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1]
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC0-7 - Work to Break TimePoint transition Scenario`` () =
        scenario {
            let timePoints = [ workTP ``0.5 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()
            do! When.``Looper TimePointStarted event has been despatched with`` timePoints[1] (timePoints[0] |> Some)

            do! Then.``Active Point is set on`` timePoints[1]
            do! Then.``Active Point remaining time is equal to or less then`` timePoints[1]
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC0-8 - Start-Replay Work TimePoint Scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()
            do! When.``Replay msg has been dispatched`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    // -------
    //  Break
    // -------

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC1-1 - Initialization Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! Then.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Initialized
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC1-2 - Start Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Playing
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC1-3 - Start-Stop Break TimePoint Scenario`` () =
        scenario {
            let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
            do! Given.``Stored TimePoints`` timePoints
            do! Given.``Initialized Program`` ()

            do! When.``Looper TimePointStarted event has been despatched with`` timePoints.Head None
            do! When.``Play msg has been dispatched`` ()
            do! When.``Stop msg has been dispatched`` ()

            do! Then.``Active Point is set on`` timePoints.Head
            do! Then.``Active Point remaining time is equal to or less then`` timePoints.Head
            do! Then.``LooperState is`` LooperState.Stopped
            do! Then.``Windows should not be minimized`` ()
            do! Then.``Telegrtam bot should not be notified`` ()
        }
        |> Scenario.runTestAsync
    (*
    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC1-4 - Start-Stop-Resume Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is stopping`` ()
        When.``Looper is resuming`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        do! Then.``LooperState is`` LooperState.Playing
        Then.``Windows should be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC1-5 - Start Break Next Work TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        do! Then.``LooperState is`` LooperState.Playing
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC1-6 - Start-Stop-Break Work TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is stopping`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        do! Then.``LooperState is`` LooperState.Playing
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC1-7 - Breate to Work TimePoint transition Scenario`` () =
        let timePoints = [ breakTP ``0.5 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``TimePoint is changed on`` timePoints[1]

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        do! Then.``LooperState is`` LooperState.Playing
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC1-8 - Start-Replay Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Spent 2.5 ticks time`` ()

        When.``Looper starts playing`` ()
        When.``Looper is replaying`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        do! Then.``LooperState is`` LooperState.Playing
        Then.``Windows should be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()
    *)