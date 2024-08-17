﻿namespace PomodoroWindowsTimer.ElmishApp.Tests.Features

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

[<Category("UC4: Work Scenarios")>]
module WorkFeature =

    [<Test>]
    let ``UC4-1 - No stored works initialization`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Program has been initialized without CurrentWork`` timePoints

            do! Then.``Current Work has not been set`` ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    let ``UC4-2 - No stored works -> open work selector scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Program has been initialized without CurrentWork`` timePoints

            do! When.``WorkSelector drawer is opening`` ()

            do! Then.``CreatingWork sub model has been shown`` ()
            do! Then.``Current Work has not been set`` ()
        }
        |> Scenario.runTestAsync

    // ---------------------
    // create work scenarios
    // ---------------------
    [<Test>]
    let ``UC4-3 - No stored works -> open work selector -> create work scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Program has been initialized without CurrentWork`` timePoints

            do! When.``WorkSelector drawer is opening`` ()
            do! When.``CreatingWork sub model has been shown`` ()

            let work = Work.generate ()
            do! When.``CreatingWork SetNumber msg has been dispatched with`` work.Number
            do! When.``CreatingWork SetTitle msg has been dispatched with`` work.Title
            do! When.``CreatingWorkModel CreateWork msg has been dispatched`` ()

            do! Then.``WorkList sub model has been shown`` 2<times>
            do! Then.``WorkList sub model selected work is set on`` 1UL
            do! Then.``WorkList sub model work list contains`` work
            let! _ = Then.``Current Work has been set to`` work
            return ()
        }
        |> Scenario.runTestAsync

    [<Test>]
    let ``UC4-4 - No stored works -> open work selector -> create work 2 times scenario`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            do! Given.``Program has been initialized without CurrentWork`` timePoints

            do! When.``WorkSelector drawer is opening`` ()
            do! When.``CreatingWork sub model has been shown`` ()

            let work1 = Work.generate ()
            do! When.``CreatingWork SetNumber msg has been dispatched with`` work1.Number
            do! When.``CreatingWork SetTitle msg has been dispatched with`` work1.Title
            do! When.``CreatingWorkModel CreateWork msg has been dispatched`` ()
            do! When.``WorkList sub model has been shown`` 2<times>
            
            do! When.``WorkListModel CreateWork msg has been dispatched`` ()
            do! When.``CreatingWork sub model has been shown`` ()

            let work2 = Work.generate ()
            do! When.``CreatingWork SetNumber msg has been dispatched with`` work2.Number
            do! When.``CreatingWork SetTitle msg has been dispatched with`` work2.Title
            do! When.``CreatingWorkModel CreateWork msg has been dispatched`` ()
            do! When.``WorkList sub model has been shown`` 3<times>

            do! Then.``WorkList sub model selected work is set on`` 2UL
            do! Then.``WorkList sub model work list contains`` work2
            let! _ = Then.``Current Work has been set to`` work2
            return ()
        }
        |> Scenario.runTestAsync

    // ---------------------
    // select work scenarios
    // ---------------------
    [<Test>]
    let ``UC4-5 - Work selected -> try to create a new one and cancel -> selected work preserves`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let! work = Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``WorkSelector drawer is opening`` ()
            do! When.``WorkList sub model has been shown`` 1<times>
            do! When.``WorkListModel CreateWork msg has been dispatched`` ()
            do! When.``CreatingWork sub model has been shown`` ()
            do! When.``Canceling the creation of work`` ()

            do! Then.``WorkList sub model has been shown`` 2<times>
            do! Then.``WorkList sub model selected work is set on`` work.Id
        }
        |> Scenario.runTestAsync

    // ---------------------
    // update work scenarios
    // ---------------------
    [<Test>]
    let ``UC4-6 - Work selected -> update work -> work updated, return to WorkList`` () =
        scenario {
            let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
            let! work = Given.``Program has been initialized with CurrentWork`` timePoints

            do! When.``WorkSelector drawer is opening`` ()
            do! When.``WorkList sub model has been shown`` 1<times>
            do! When.``WorkListModel Edit WorkModel msg has been dispatched`` work
            do! When.``UpdatingWork sub model has been shown`` ()

            let updatedWork = Work.generate ()
            do! When.``UpdatingWork SetNumber msg has been dispatched with`` updatedWork.Number
            do! When.``UpdatingWork SetTitle msg has been dispatched with`` updatedWork.Title
            do! When.``Update WorkModel msg has been dispatched`` ()

            do! Then.``WorkList sub model has been shown`` 2<times>
            do! Then.``WorkList sub model selected work is set on`` work.Id
            do! Then.``WorkList sub model work list contains`` updatedWork
            do! Then.``WorkList sub model work list does not contain`` work
        }
        |> Scenario.runTestAsync