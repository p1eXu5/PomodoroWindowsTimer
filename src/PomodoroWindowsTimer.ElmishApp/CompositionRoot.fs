module PomodoroWindowsTimer.ElmishApp.CompositionRoot

open Microsoft.Extensions.Logging

open Elmish
open Telegram.Bot

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open System


let compose
    (title: string)
    (tickMilliseconds: int<ms>)
    (workRepository: IWorkRepository)
    (themeSwitcher: IThemeSwitcher)
    (userSettings: IUserSettings)
    (mainErrorMessageQueue: IErrorMessageQueue)
    (dialogErrorMessageQueue: IErrorMessageQueue)
    (loggerFactory: ILoggerFactory)
    =
    let timePointQueue = new TimePointQueue(loggerFactory.CreateLogger<TimePointQueue>())
    let looper = new Looper((timePointQueue :> ITimePointQueue), tickMilliseconds, loggerFactory.CreateLogger<Looper>())

    let sendToBot message =
        match userSettings.BotToken, userSettings.MyChatId with
        | Some botToken, Some myChatId ->
            let botClient = TelegramBotClient(botToken)
            message |> Telegram.sendToBot botClient (Types.ChatId(myChatId))
        | _ ->
            task { return () }

#if DEBUG
    let windowsMinimizer = Windows.simWindowsMinimizer
#else
    let windowsMinimizer = Windows.prodWindowsMinimizer title
#endif

    let patternStore = PatternStore.initialize userSettings
    let timePointPrototypeStore = TimePointPrototypeStore.initialize userSettings

    let mainModelCfg =
        {
            UserSettings = userSettings
            SendToBot = sendToBot
            Looper = looper
            TimePointQueue = timePointQueue
            WindowsMinimizer = windowsMinimizer
            ThemeSwitcher = themeSwitcher
            TimePointStore = TimePointStore.initialize userSettings
            WorkRepository = workRepository
        }
    // init
    let initMainModel () =
        MainModel.init mainModelCfg

    // update
    let updateMainModel =
        let initBotSettingsModel () =
            BotSettingsModel.init userSettings

        let updateBotSettingsModel =
            BotSettingsModel.Program.update userSettings

        let initTimePointGeneratorModel () =
            TimePointsGeneratorModel.init timePointPrototypeStore patternStore

        let updateTimePointGeneratorModel =
            TimePointsGeneratorModel.Program.update patternStore timePointPrototypeStore dialogErrorMessageQueue

        let updateWorkModel =
            WorkModel.Program.update workRepository (loggerFactory.CreateLogger<WorkModel>()) mainErrorMessageQueue

        let updateAppDialogModel =
            AppDialogModel.Program.update
                initBotSettingsModel
                updateBotSettingsModel
                initTimePointGeneratorModel
                updateTimePointGeneratorModel
                dialogErrorMessageQueue

        MainModel.Program.update
            mainModelCfg
            updateWorkModel
            updateAppDialogModel
            mainErrorMessageQueue
            (loggerFactory.CreateLogger<MainModel>())

    // bindings:
    let ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version
    let assemblyVer =
        sprintf "Version: %i.%i.%i" ver.Major ver.Minor ver.Build

    let mainModelBindings =
        fun () ->
            MainModel.Bindings.Bindings.ToList title assemblyVer mainErrorMessageQueue dialogErrorMessageQueue

    // subscriptions
    let subscribe _ : (SubId * Subscribe<_>) list =
        let looperSubscription dispatch =
            let onLooperEvt =
                fun evt ->
                    async {
                        do dispatch (MainModel.Msg.LooperMsg evt)
                    }
            looper.AddSubscriber(onLooperEvt)
            { new IDisposable with 
                member _.Dispose() = ()
            }
        [ ["Looper"], looperSubscription ]

    // initialization
    looper.Start()
    (initMainModel, updateMainModel, mainModelBindings, subscribe)

