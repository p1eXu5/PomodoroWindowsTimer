namespace PomodoroWindowsTimer.ElmishApp.Tests

open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open NUnit.Framework
open TestServices
open PomodoroWindowsTimer.Types
open Microsoft.Extensions.Logging
open p1eXu5.AspNetCore.Testing.Logging
open NUnit.Framework

type TestElmishProgram =
    {
        Looper: Looper
        TestDispatch: TestDispatch
        mutable MainModel: MainModel
        mutable TestDispatchSubscriber: System.IDisposable
        MainModelCfg: MainModeConfig
    }

type MainModelCfgFactory = Looper -> TimePointQueue -> MainModeConfig


module ElmishTestState =
    let run (TestState f) =
        let (state, _) = f ()
        (state.Looper :> System.IDisposable).Dispose()
        state.TestDispatchSubscriber.Dispose()
        ()

module TestElmishProgram =

    open PomodoroWindowsTimer.ElmishApp.Infrastructure
    open Elmish

    let defaultMainModelCfg dict =
        let testBotConfiguration = TestBotConfiguration.create ()
        let sendToBot = TestBotSender.create ()

        fun () ->
            {
                BotSettings = testBotConfiguration
                SendToBot = sendToBot
                Looper = looper
                TimePointQueue = timePointQueue
                WindowsMinimizer = Windows.simWindowsMinimizer
                ThemeSwitcher = TestThemeSwitcher.create ()
                // TimePointPrototypeStore = TimePointPrototypeStore.initialize (TestTimePointPrototypeSettings.create dict)
                TimePointStore = TimePointStore.initialize (TestTimePointSettings.create dict)
                // PatternStore = PatternStore.initialize (TestPatternSettings.create dict)
                DisableSkipBreakSettings = TestDisableSkipBreakSettings.create dict
            }

    let run mainModelCfgFactory =
        let testLoggerFactory = TestLoggerFactory.CreateWith(TestContext.Progress, TestContext.Out)

        let mainModelCfg =
            mainModelCfgFactory ()

        let (initMainModel, updateMainModel, _, subscribe) =
            CompositionRoot.compose
                "Pomodoro Windows Timer"
                Program.tickMilliseconds
                (TestThemeSwitcher.create ())
                (TestUserSettings.create ())
                (TestErrorMessageQueue.create ())
                testLoggerFactory

        let testElmishProgram =
            {
                Looper = looper
                TestDispatch = TestDispatch()
                MainModel = Unchecked.defaultof<_>
                TestDispatchSubscriber = Unchecked.defaultof<_>
                MainModelCfg = mainModelCfg
            }


        let subscribe m : (SubId * Subscribe<_>) list =

            let sendExtMsgEffect dispatch =
                testElmishProgram.TestDispatchSubscriber <-
                    testElmishProgram.TestDispatch.MsgDispatchRequested.Subscribe(fun msg -> dispatch msg)
                { new System.IDisposable with 
                    member _.Dispose() = ()
                }

            ( ["sendExtMsgEffect"], sendExtMsgEffect ) :: (subscribe m)

        looper.Start()

        do
            Program.mkProgram 
                initMainModel
                updateMainModel
                (fun m _ -> testElmishProgram.MainModel <- m)
            |> Program.withSubscription subscribe
            |> Program.withTrace (fun msg _ _ -> TestContext.WriteLine(sprintf "%A" msg))
            |> Program.run

        testElmishProgram

