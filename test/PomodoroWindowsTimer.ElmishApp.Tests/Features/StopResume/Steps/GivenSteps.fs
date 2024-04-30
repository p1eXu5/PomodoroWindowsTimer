module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.GivenSteps

 //let ``Elmish Program with`` (timePoints: TimePoint list) =
 //   let timePointQueue = new TimePointQueue(timePoints)
 //   looper <- new Looper((timePointQueue :> ITimePointQueue), Program.tickMilliseconds)

 //   let subscribe _ =
 //       let looperEffect dispatch =
 //           let onLooperEvt =
 //               fun evt ->
 //                   async {
 //                       do dispatch (MainModel.Msg.LooperMsg evt)
 //                   }

 //           looper.AddSubscriber(onLooperEvt)

 //       let sendExtMsgEffect dispatch =
 //           testDispatchSubscriber <- testDispatch.MsgDispatchRequested.Subscribe(fun msg -> dispatch msg)

 //       [ looperEffect; sendExtMsgEffect ]

 //   looper.Start()

 //   let testBotConfiguration = TestBotConfiguration.create ()
 //   let sendToBot = TestBotSender.create ()

 //   let dict = Dictionary<string, obj>()

 //   let mainModelCfg =
 //       {
 //           BotSettings = testBotConfiguration
 //           SendToBot = sendToBot
 //           Looper = looper
 //           TimePointQueue = timePointQueue
 //           WindowsMinimizer = Windows.simWindowsMinimizer
 //           ThemeSwitcher = TestThemeSwitcher.create ()
 //           TimePointPrototypeStore = TestTimePointPrototypeStore.create ()
 //           TimePointStore = TestTimePointStore.create timePoints
 //           PatternStore = PatternStore.initialize (TestPatternSettings.create dict)
 //           DisableSkipBreakSettings = TestDisableSkipBreakSettings.create dict
 //       }

 //   Program.mkProgram 
 //       (fun () ->
 //           MainModel.init
 //               (TestSettingsManager.create ())
 //               (ErrorMessageQueueStub.create ())
 //               mainModelCfg
 //       ) 
 //       (MainModel.Program.update mainModelCfg)
 //       (fun m _ -> model <- m)
 //   |> Program.withSubscription subscribe
 //   |> Program.withTrace (fun msg _ -> TestContext.WriteLine(sprintf "%A" msg))
 //   |> Program.run



