namespace PomodoroWindowsTimer.ElmishApp.Tests.BDD

open System
open System.Threading.Tasks
open NUnit.Framework
open FsUnit.TopLevelOperators
open ShouldExtensions
open Bogus

open Elmish
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests.TestServices
open PomodoroWindowsTimer.ElmishApp.Tests.TestStateCE
open PomodoroWindowsTimer.ElmishApp.Tests.Fakers
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer
open System.Collections.Generic


module GenerateTimePointsFeature =

    module Given =
        let ``No TimePoint settings`` () =
            test {
                let dict = Dictionary<string, obj>()
                return! TestState.replace (TestElmishProgram.defaultMainModelCfg dict)
            }

    module When =
        let ``Elmish Program running`` () =
            test {
                let! cfg = TestState.getState
                return! TestState.replace (TestElmishProgram.run cfg)
            }

        let ``Spent 2.5 ticks time`` () =
            test {
                let! cfg = TestState.getState
                do CommonSteps.``Spent 2.5 ticks time`` cfg.TestDispatch
            }


    module Then =
        let ``Default TimePoints are generated`` () =
            test {
                let! cfg = TestState.getState

                cfg.MainModel.TimePoints
                |> should equivalent Types.TimePoint.defaults
            }

        let ``Default TimePoints are stored in settings`` () =
            test {
                let! cfg = TestState.getState
                cfg.MainModelCfg.TimePointStore.Read()
                |> should equivalent Types.TimePoint.defaults
            }

        let ``Default pattern  are stored in settings`` () =
            test {
                let! cfg = TestState.getState
                cfg.MainModelCfg.PatternStore.Read()
                |> should equivalent Types.Pattern.defaults
            }


    [<Test>]
    let ``UC21 - Have no stored time point settings defaults are used`` () =
        test {
            do! Given.``No TimePoint settings``()
            do! When.``Elmish Program running``() 
            do! When.``Spent 2.5 ticks time`` () // wait when looper preload time point and send event

            do! Then.``Default TimePoints are generated`` ()
            do! Then.``Default TimePoints are stored in settings`` ()
            do! Then.``Default pattern  are stored in settings`` ()
        }
        |> ElmishTestState.run

