namespace PomodoroWindowsTimer.ElmishApp.Tests

open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open NUnit.Framework
open TestServices

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

        fun looper timePointQueue ->
            {
                BotSettings = testBotConfiguration
                SendToBot = sendToBot
                Looper = looper
                TimePointQueue = timePointQueue
                WindowsMinimizer = Windows.simWindowsMinimizer
                ThemeSwitcher = TestThemeSwitcher.create ()
                TimePointPrototypeStore = TimePointPrototypeStore.initialize (TestTimePointPrototypeSettings.create dict)
                TimePointStore = TimePointStore.initialize (TestTimePointSettings.create dict)
                PatternStore = PatternStore.initialize (TestPatternSettings.create dict)
                DisableSkipBreakSettings = TestDisableSkipBreakSettings.create dict
            }

    let run mainModelCfgFactory =
        let timePointQueue = new TimePointQueue()
        let looper = new Looper((timePointQueue :> ITimePointQueue), Program.tickMilliseconds)
        
        let mainModelCfg =
            mainModelCfgFactory looper timePointQueue

        let testElmishProgram =
            {
                Looper = looper
                TestDispatch = TestDispatch()
                MainModel = MainModel.initDefault ()
                TestDispatchSubscriber = Unchecked.defaultof<_>
                MainModelCfg = mainModelCfg
            }


        let subscribe _ =
            let looperEffect dispatch =
                let onLooperEvt =
                    fun evt ->
                        async {
                            do dispatch (MainModel.Msg.LooperMsg evt)
                        }

                looper.AddSubscriber(onLooperEvt)

            let sendExtMsgEffect dispatch =
                testElmishProgram.TestDispatchSubscriber <-
                    testElmishProgram.TestDispatch.MsgDispatchRequested.Subscribe(fun msg -> dispatch msg)

            [ looperEffect; sendExtMsgEffect ]

        looper.Start()


        do
            Program.mkProgram 
                (fun () ->
                    MainModel.init
                        (TestSettingsManager.create ())
                        (TestErrorMessageQueue.create ())
                        mainModelCfg
                ) 
                (MainModel.Program.update mainModelCfg)
                (fun m _ -> testElmishProgram.MainModel <- m)
            |> Program.withSubscription subscribe
            |> Program.withTrace (fun msg _ -> TestContext.WriteLine(sprintf "%A" msg))
            |> Program.run

        testElmishProgram

