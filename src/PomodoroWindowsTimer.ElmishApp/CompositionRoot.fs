module PomodoroWindowsTimer.ElmishApp.CompositionRoot

open System
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


let compose
    (title: string)
    (workStatisticWindowFactory: System.Func<#System.Windows.Window>)
    (looper: ILooper)
    (timePointQueue: ITimePointQueue)
    (workRepository: IWorkRepository)
    (workEventRepository: IWorkEventRepository)
    (telegramBot: ITelegramBot)
    (windowsMinimizer: IWindowsMinimizer)
    (themeSwitcher: IThemeSwitcher)
    (userSettings: IUserSettings)
    (mainErrorMessageQueue: IErrorMessageQueue)
    (dialogErrorMessageQueue: IErrorMessageQueue)
    (timeProvider: System.TimeProvider)
    (loggerFactory: ILoggerFactory)
    =
    let patternStore = PatternStore.init userSettings
    let timePointPrototypeStore = TimePointPrototypeStore.initialize userSettings

    let mainModelCfg =
        {
            UserSettings = userSettings
            SendToBot = telegramBot
            Looper = looper
            TimePointQueue = timePointQueue
            WindowsMinimizer = windowsMinimizer
            ThemeSwitcher = themeSwitcher
            TimePointStore = TimePointStore.initialize userSettings
            WorkRepository = workRepository
            WorkEventRepository = workEventRepository
            TimeProvider = timeProvider
        }

    let appDialogModelCfg : AppDialogModel.Cfg =
        {
            UserSettings = userSettings
            WorkEventRepository = workEventRepository
            MainErrorMessageQueue = mainErrorMessageQueue
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

        let updateWorkEventListModel =
            WorkEventListModel.Program.update workEventRepository dialogErrorMessageQueue (loggerFactory.CreateLogger<WorkEventListModel>())

        let initWorkStatisticListModel =
            fun () -> WorkStatisticListModel.init userSettings timeProvider

        let updateWorkStatisticListModel =
            WorkStatisticListModel.Program.update
                userSettings
                workEventRepository
                dialogErrorMessageQueue
                (loggerFactory.CreateLogger<WorkStatisticListModel>())
                updateWorkEventListModel

        let updateRollbackWorkModel =
            RollbackWorkModel.Program.update userSettings

        let updateAppDialogModel =
            AppDialogModel.Program.update
                appDialogModelCfg
                initBotSettingsModel
                updateBotSettingsModel
                updateRollbackWorkModel

        let updateWorkModel =
            WorkModel.Program.update workRepository (loggerFactory.CreateLogger<WorkModel>()) mainErrorMessageQueue

        let updateWorkListModel =
            WorkListModel.Program.update workRepository (loggerFactory.CreateLogger<WorkListModel>()) mainErrorMessageQueue updateWorkModel

        let updateCreatingWorkModel =
            CreatingWorkModel.Program.update workRepository mainErrorMessageQueue (loggerFactory.CreateLogger<CreatingWorkModel>())

        let updateWorkSelectorModel =
            WorkSelectorModel.Program.update updateWorkListModel updateCreatingWorkModel updateWorkModel (loggerFactory.CreateLogger<WorkSelectorModel>())

        MainModel.Program.update
            mainModelCfg
            updateWorkModel
            updateAppDialogModel
            updateWorkSelectorModel
            initWorkStatisticListModel
            updateWorkStatisticListModel
            mainErrorMessageQueue
            (loggerFactory.CreateLogger<MainModel>())

    // bindings:
    let ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version
    let assemblyVer =
        sprintf "Version: %i.%i.%i" ver.Major ver.Minor ver.Build

    let mainModelBindings =
        fun () ->
            MainModel.Bindings.ToList title assemblyVer workStatisticWindowFactory mainErrorMessageQueue dialogErrorMessageQueue

    // subscriptions
    let subscribe _ : (SubId * Subscribe<_>) list =
        let looperSubscription dispatch =
            let onLooperEvt =
                fun evt ->
                    async {
                        do dispatch (MainModel.ControllerMsg.LooperMsg evt |> MainModel.Msg.ControllerMsg)
                    }
            looper.AddSubscriber(onLooperEvt)
            { new IDisposable with 
                member _.Dispose() = ()
            }
        [ ["Looper"], looperSubscription ]

    // initialization
    looper.Start()
    (initMainModel, updateMainModel, mainModelBindings, subscribe)

