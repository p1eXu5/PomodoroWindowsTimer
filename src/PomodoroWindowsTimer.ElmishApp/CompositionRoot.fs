module PomodoroWindowsTimer.ElmishApp.CompositionRoot

open System
open Microsoft.Extensions.Logging

open Elmish

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure


let compose
    (title: string)
    (workStatisticWindowFactory: System.Func<System.Windows.Window>)
    (looper: ILooper)
    (timePointQueue: ITimePointQueue)
    (workEventStore: WorkEventStore)
    (telegramBot: ITelegramBot)
    (windowsMinimizer: IWindowsMinimizer)
    (themeSwitcher: IThemeSwitcher)
    (userSettings: IUserSettings)
    (mainErrorMessageQueue: IErrorMessageQueue)
    (dialogErrorMessageQueue: IErrorMessageQueue)
    (timeProvider: System.TimeProvider)
    (excelBook: IExcelBook)
    (loggerFactory: ILoggerFactory)
    =
    let patternStore = PatternStore.init userSettings
    let timePointPrototypeStore = TimePointPrototypeStore.initialize userSettings

    let mainModelCfg =
        {
            UserSettings = userSettings
            TelegramBot = telegramBot
            Looper = looper
            TimePointQueue = timePointQueue
            WindowsMinimizer = windowsMinimizer
            ThemeSwitcher = themeSwitcher
            TimePointStore = TimePointStore.initialize userSettings
            WorkEventStore = workEventStore
            TimeProvider = timeProvider
        }

    // TODO: remove
    // let appDialogModelCfg : AppDialogModel.Cfg =
    //     {
    //         UserSettings = userSettings
    //         RepositoryFactory = repositoryFactory
    //         MainErrorMessageQueue = mainErrorMessageQueue
    //     }

    // init
    let initMainModel () =
        MainModel.init userSettings

    // update
    let updateMainModel =
        let initBotSettingsModel () =
            BotSettingsModel.init userSettings

        let updateBotSettingsModel =
            BotSettingsModel.Program.update userSettings

        let initDatabaseSettingsModel () =
            DatabaseSettingsModel.init userSettings

        let updateDatabaseSettingsModel =
            DatabaseSettingsModel.Program.update userSettings

        let initTimePointGeneratorModel () =
            TimePointsGeneratorModel.init timePointPrototypeStore patternStore

        let updateTimePointGeneratorModel =
            TimePointsGeneratorModel.Program.update patternStore timePointPrototypeStore dialogErrorMessageQueue

        let updateWorkEventListModel =
            WorkEventListModel.Program.update workEventStore dialogErrorMessageQueue (loggerFactory.CreateLogger<WorkEventListModel>())

        let updateDailyStatisticModel =
            DailyStatisticModel.Program.update
                timeProvider
                workEventStore
                excelBook
                dialogErrorMessageQueue
                (loggerFactory.CreateLogger<DailyStatisticModel>())

        let initDailyStatisticListModel =
            fun () ->
                DailyStatisticListModel.init
                    userSettings
                    timeProvider

        let updateDailyStatisticListModel =
            DailyStatisticListModel.Program.update
                userSettings
                workEventStore
                excelBook
                dialogErrorMessageQueue
                (loggerFactory.CreateLogger<DailyStatisticListModel>())
                updateDailyStatisticModel
                updateWorkEventListModel

        let updateRollbackWorkModel =
            RollbackWorkModel.Program.update userSettings

        let updateRollbackWorkListModel =
            RollbackWorkListModel.Program.update updateRollbackWorkModel

        let updateAppDialogModel =
            AppDialogModel.Program.update
                workEventStore
                userSettings
                mainErrorMessageQueue
                initBotSettingsModel
                updateBotSettingsModel
                initDatabaseSettingsModel
                updateDatabaseSettingsModel
                updateRollbackWorkModel
                updateRollbackWorkListModel

        let updateWorkModel =
            WorkModel.Program.update workEventStore (loggerFactory.CreateLogger<WorkModel>()) mainErrorMessageQueue

        let updateWorkListModel =
            WorkListModel.Program.update userSettings workEventStore (loggerFactory.CreateLogger<WorkListModel>()) mainErrorMessageQueue updateWorkModel

        let updateCreatingWorkModel =
            CreatingWorkModel.Program.update workEventStore mainErrorMessageQueue (loggerFactory.CreateLogger<CreatingWorkModel>())

        let updateWorkSelectorModel =
            WorkSelectorModel.Program.update updateWorkListModel updateCreatingWorkModel updateWorkModel (loggerFactory.CreateLogger<WorkSelectorModel>())

        let initStatisticMainModel () =
            StatisticMainModel.init userSettings timeProvider

        let updateStatisticMainModel =
            StatisticMainModel.Program.update
                updateDailyStatisticListModel
                updateWorkListModel
                (loggerFactory.CreateLogger<StatisticMainModel>())

        let updatePlayerModel =
            PlayerModel.Program.update
                looper
                windowsMinimizer
                timeProvider
                workEventStore 
                themeSwitcher 
                telegramBot 
                userSettings 
                timePointQueue 
                mainErrorMessageQueue 
                (loggerFactory.CreateLogger<PlayerModel>())

        MainModel.Program.update
            mainModelCfg
            workEventStore
            updateWorkModel
            updateAppDialogModel
            updateWorkSelectorModel
            initStatisticMainModel
            updateStatisticMainModel
            updatePlayerModel
            mainErrorMessageQueue
            (loggerFactory.CreateLogger<MainModel>())

    // bindings:
    let ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version
    let assemblyVer =
        sprintf "Version: %i.%i.%i" ver.Major ver.Minor ver.Build

    do userSettings.CurrentVersion <- assemblyVer

    let mainModelBindings =
        fun () ->
            MainModel.Bindings.ToList title assemblyVer workStatisticWindowFactory mainErrorMessageQueue dialogErrorMessageQueue timePointQueue looper

    // subscriptions
    let subscribe _ : (SubId * Subscribe<_>) list =
        let looperSubscription dispatch =
            let onLooperEvt =
                fun evt ->
                    async {
                        match evt with
                        | LooperEvent.TimePointTimeReduced tp ->
                            do dispatch (MainModel.LooperMsg.TimePointTimeReduced tp |> MainModel.Msg.LooperMsg)

                        | LooperEvent.TimePointStarted args ->
                            do dispatch (MainModel.LooperMsg.TimePointStarted args |> MainModel.Msg.LooperMsg)
                    }
            looper.AddSubscriber(onLooperEvt)
            { new IDisposable with 
                member _.Dispose() = ()
            }
        [ ["Looper"], looperSubscription ]

    // initialization
    looper.Start()
    (initMainModel, updateMainModel, mainModelBindings, subscribe)

