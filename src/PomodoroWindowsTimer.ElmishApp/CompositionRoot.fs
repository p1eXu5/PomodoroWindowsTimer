module PomodoroWindowsTimer.ElmishApp.CompositionRoot

open Telegram.Bot
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure


let compose
    (title: string)
    (tickMilliseconds: int<ms>)
    (themeSwitcher: IThemeSwitcher)
    (userSettings: IUserSettings)
    (mainErrorMessageQueue: IErrorMessageQueue)
    =
    let timePointQueue = new TimePointQueue()
    let looper = new Looper((timePointQueue :> ITimePointQueue), tickMilliseconds)

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
            BotSettings = userSettings
            SendToBot = sendToBot
            Looper = looper
            TimePointQueue = timePointQueue
            WindowsMinimizer = windowsMinimizer
            ThemeSwitcher = themeSwitcher
            TimePointStore = TimePointStore.initialize userSettings
            DisableSkipBreakSettings = userSettings
        }
    // init
    let initMainModel () =
        MainModel.init mainModelCfg

    // update
    let updateMainModel =
        let updateBotSettingsModel =
            BotSettingsModel.Program.update userSettings

        let updateTimePointGeneratorModel =
            TimePointsGeneratorModel.Program.update patternStore timePointPrototypeStore

        let initTimePointGeneratorModel () =
            TimePointsGeneratorModel.init timePointPrototypeStore patternStore

        let initBotSettingsModel () =
            BotSettingsModel.init userSettings
            |> Some

        MainModel.Program.update mainModelCfg initBotSettingsModel updateBotSettingsModel updateTimePointGeneratorModel initTimePointGeneratorModel mainErrorMessageQueue

    // bindings:
    let ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version
    let assemblyVer =
        sprintf "Version: %i.%i.%i" ver.Major ver.Minor ver.Build

    let mainModelBindings =
        fun () ->
            MainModel.Bindings.bindings title assemblyVer mainErrorMessageQueue

    // subscriptions
    let subscribe _ =
        let effect dispatch =
            let onLooperEvt =
                fun evt ->
                    async {
                        do dispatch (MainModel.Msg.LooperMsg evt)
                    }
            looper.AddSubscriber(onLooperEvt)
        [ effect ]

    // initialization
    looper.Start()
    (initMainModel, updateMainModel, mainModelBindings, subscribe)

