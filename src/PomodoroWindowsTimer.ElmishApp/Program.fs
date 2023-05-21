module PomodoroWindowsTimer.ElmishApp.Program

open System
open Serilog
open Serilog.Extensions.Logging
open Telegram.Bot
open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure

let [<Literal>] tickMilliseconds = 200


let internal main (
    window,
    errorQueue,
    settingsManager,
    botSettings: IBotSettings,
    themeSwitcher: IThemeSwitcher,
    timePointPrototypesSettings : ITimePointPrototypesSettings,
    timePointSettings : ITimePointSettings,
    patternSettings: IPatternSettings,
    disableSkipBreakSettings: IDisableSkipBreakSettings)
    =
    let logger =
        LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Debug)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Debug)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Debug)
            .MinimumLevel.Override(nameof (PomodoroWindowsTimer.ElmishApp), Events.LogEventLevel.Debug)
#else
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Information)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Information)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Information)
            .MinimumLevel.Override(nameof (PomodoroWindowsTimer.ElmishApp), Events.LogEventLevel.Information)
#endif
            //.WriteTo.Console(outputTemplate="[{Timestamp:HH:mm:ss:fff} {Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console(outputTemplate="[{Timestamp:HH:mm:ss:fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug(outputTemplate="[{Timestamp:HH:mm:ss:fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            //.WriteTo.Seq("http://localhost:5341")
            .CreateLogger()

    let loggerFactory = new SerilogLoggerFactory(logger)

    // let mainModelLogger : ILogger = loggerFactory.CreateLogger(nameof (PomodoroWindowsTimer.ElmishApp.Models.MainModel))

    let timePointQueue = new TimePointQueue()
    let looper = new Looper((timePointQueue :> ITimePointQueue), tickMilliseconds)

    let subscribe _ =
        let effect dispatch =
            let onLooperEvt =
                fun evt ->
                    async {
                        do dispatch (MainModel.Msg.LooperMsg evt)
                    }
            looper.AddSubscriber(onLooperEvt)
        [ effect ]

    looper.Start()

    let sendToBot (botSettings: IBotSettings) =
        match botSettings.BotToken, botSettings.MyChatId with
        | Some botToken, Some myChatId ->
            let botClient = TelegramBotClient(botToken)
            Telegram.sendToBot botClient (Types.ChatId(myChatId))
        | _ ->
            fun _ -> task { return () }

#if DEBUG
    let windowsMinimizer = Windows.simWindowsMinimizer
#else
    let windowsMinimizer = Windows.prodWindowsMinimizer
#endif

    let mainModelCfg =
        {
            BotSettings = botSettings
            SendToBot = sendToBot
            Looper = looper
            TimePointQueue = timePointQueue
            WindowsMinimizer = windowsMinimizer
            ThemeSwitcher = themeSwitcher
            TimePointPrototypeStore = TimePointPrototypeStore.initialize timePointPrototypesSettings
            TimePointStore = TimePointStore.initialize timePointSettings
            PatternStore = PatternStore.initialize patternSettings
            DisableSkipBreakSettings = disableSkipBreakSettings
        }


    WpfProgram.mkProgram 
        (fun () -> MainModel.init settingsManager errorQueue mainModelCfg)
        (MainModel.Program.update mainModelCfg)
        MainModel.Bindings.bindings
    |> WpfProgram.withLogger loggerFactory
    |> WpfProgram.withSubscription subscribe
    |> WpfProgram.startElmishLoop window
