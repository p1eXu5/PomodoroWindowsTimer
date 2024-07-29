module PomodoroWindowsTimer.ElmishApp.Tests.Features.GenerateTimePointsFeature

()
//open System
//open System.Threading.Tasks
//open NUnit.Framework
//open FsUnit.TopLevelOperators
//open ShouldExtensions
//open Bogus

//open Elmish
//open PomodoroWindowsTimer.Types
//open PomodoroWindowsTimer.Abstractions
//open PomodoroWindowsTimer.TimePointQueue
//open PomodoroWindowsTimer.Looper

//open PomodoroWindowsTimer.ElmishApp
//open PomodoroWindowsTimer.ElmishApp.Infrastructure
//open PomodoroWindowsTimer.ElmishApp.Models
//open PomodoroWindowsTimer.ElmishApp.Models.MainModel

//open PomodoroWindowsTimer.ElmishApp.Tests.TestServices
//open PomodoroWindowsTimer.ElmishApp.Tests.TestStateCE
//open PomodoroWindowsTimer.ElmishApp.Tests.Fakers
//open PomodoroWindowsTimer.ElmishApp.Tests
//open PomodoroWindowsTimer
//open System.Collections.Generic


//module GenerateTimePointsFeature =

//    module Given =
//        let ``No TimePoint and Pattern settings`` () =
//            test {
//                let dict = Dictionary<string, obj>()
//                return! TestState.replace (TestElmishProgram.defaultMainModelCfg dict)
//            }

//    module When =
//        let ``Elmish Program running`` () =
//            test {
//                let! cfg = TestState.getState
//                return! TestState.replace (TestElmishProgram.run cfg)
//            }

//        let ``Spent 2.5 ticks time`` () =
//            test {
//                let! cfg = TestState.getState
//                do CommonSteps.``Spent 2.5 ticks time`` cfg.TestDispatch
//            }

//        let ``User enters pattern`` (pattern: Pattern) =
//            test {
//                let! cfg = TestState.getState
//                do cfg.TestDispatch.TriggerWithTimeout(
//                    pattern
//                    |> Some
//                    |> TimePointsGenerator.Msg.SetSelectedPattern
//                    |> MainModel.Msg.TimePointsGeneratorMsg
//                )
//            }

//        let ``User applies pattern`` () =
//            test {
//                let! cfg = TestState.getState
//                do cfg.TestDispatch.TriggerWithTimeout(
//                    TimePointsGenerator.Msg.ApplyTimePoints
//                    |> MainModel.Msg.TimePointsGeneratorMsg
//                )
//            }

//    module Then =
//        let ``Default TimePoints are loaded`` () =
//            test {
//                let! cfg = TestState.getState

//                cfg.MainModel.TimePoints
//                |> should equivalent Types.TimePoint.defaults
//            }

//        let ``Default TimePoints are stored in settings`` () =
//            test {
//                let! cfg = TestState.getState
//                cfg.MainModelCfg.TimePointStore.Read()
//                |> should equivalent Types.TimePoint.defaults
//            }

//        let ``Default pattern are stored in settings`` () =
//            test {
//                let! cfg = TestState.getState
//                cfg.MainModelCfg.PatternStore.Read()
//                |> should equivalent Types.Pattern.defaults
//            }

//        let ``Generated TimePoints are loaded`` () =
//            test {
//                let! cfg = TestState.getState

//                cfg.MainModel.TimePoints
//                |> should equivalent cfg.MainModel.TimePointsGeneratorModel.TimePoints
//            }

//        let ``Generated TimePoints are stored in settings`` () =
//            test {
//                let! cfg = TestState.getState
//                cfg.MainModelCfg.TimePointStore.Read()
//                |> should equivalent cfg.MainModel.TimePointsGeneratorModel.TimePoints
//            }

//        let ``Pattern is stored in settings`` (pattern: Pattern) =
//            test {
//                let! cfg = TestState.getState
//                cfg.MainModelCfg.PatternStore.Read()
//                |> should contain pattern
//            }


//    [<Test>]
//    let ``UC21 - Have no stored time points and patterns -> defaults are used`` () =
//        test {
//            do! Given.``No TimePoint and Pattern settings``()

//            do! When.``Elmish Program running``() 
//            do! When.``Spent 2.5 ticks time`` () // wait when looper preload time point and send event

//            do! Then.``Default TimePoints are loaded`` ()
//            do! Then.``Default TimePoints are stored in settings`` ()
//            do! Then.``Default pattern are stored in settings`` ()
//        }
//        |> ElmishTestState.run

//    [<Test>]
//    let ``UC22 - User enters correct pattern but is not applying it`` () =
//        test {
//            do! Given.``No TimePoint and Pattern settings``()

//            do! When.``Elmish Program running``() 
//            do! When.``Spent 2.5 ticks time`` () // wait when looper preload time point and send event
//            do! When.``User enters pattern`` ("(w-b)")

//            do! Then.``Default TimePoints are loaded`` ()
//            do! Then.``Default TimePoints are stored in settings`` ()
//            do! Then.``Default pattern are stored in settings`` ()
//        }
//        |> ElmishTestState.run

//    [<Test>]
//    let ``UC23 - User enters correct pattern and applies it`` () =
//        test {
//            do! Given.``No TimePoint and Pattern settings``()

//            do! When.``Elmish Program running``() 
//            do! When.``Spent 2.5 ticks time`` () // wait when looper preload time point and send event
//            do! When.``User enters pattern`` ("(w-b)")
//            do! When.``User applies pattern`` ()

//            do! Then.``Generated TimePoints are loaded`` ()
//            do! Then.``Generated TimePoints are stored in settings`` ()
//            do! Then.``Pattern is stored in settings`` ("(w-b)")
//        }
//        |> ElmishTestState.run

