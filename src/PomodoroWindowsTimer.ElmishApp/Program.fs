﻿module PomodoroWindowsTimer.ElmishApp.Program

open System
open Serilog
open Serilog.Extensions.Logging
open Telegram.Bot
open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstrractions
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Abstractions

let mainModel = ViewModel.designInstance (MainModel.initDefault ()) (MainModel.Bindings.bindings ())

let [<Literal>] tickMilliseconds = 200

let internal main (window, errorQueue, settingsManager, botConfiguration: IBotConfiguration) =
    let logger =
        LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Error)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Error)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Error)
            .MinimumLevel.Override(nameof (PomodoroWindowsTimer.ElmishApp), Events.LogEventLevel.Error)
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

    let timePoints =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Break }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Break }
            { Id = Guid.NewGuid(); Name = "Focused Work 3"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 3"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Break }
            { Id = Guid.NewGuid(); Name = "Focused Work 4"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Long Break"; TimeSpan = TimeSpan.FromMinutes(20); Kind = Break }
        ]

    let testTimePoints =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromSeconds(4); Kind = Break }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromSeconds(4); Kind = Break }
        ]

    let timePointQueue = new TimePointQueue(timePoints)
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

    let sendToBot (botConfiguration: IBotConfiguration) =
        let botClient = TelegramBotClient(botConfiguration.BotToken)
        Infrastructure.sendToBot botClient (Types.ChatId(botConfiguration.MyChatId))

    WpfProgram.mkProgram 
        (fun () -> MainModel.init settingsManager botConfiguration errorQueue timePoints)
#if DEBUG
        (MainModel.Program.update botConfiguration sendToBot looper Infrastructure.simWindowsMinimizer)
#else
        (MainModel.Program.update botConfiguration sendToBot looper Infrastructure.prodWindowsMinimizer)
#endif
        MainModel.Bindings.bindings
    |> WpfProgram.withLogger loggerFactory
    |> WpfProgram.withSubscription subscribe
    |> WpfProgram.startElmishLoop window
