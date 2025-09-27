module PomodoroWindowsTimer.ElmishApp.CompositionRoot

open System
open Microsoft.Extensions.Logging

open Elmish

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

    // TODO: remove
    // let mainModelCfg =
    //     {
    //         UserSettings = userSettings
    //         TelegramBot = telegramBot
    //         Looper = looper
    //         TimePointQueue = timePointQueue
    //         WindowsMinimizer = windowsMinimizer
    //         ThemeSwitcher = themeSwitcher
    //         TimePointStore = TimePointStore.initialize userSettings
    //         WorkEventStore = workEventStore
    //         TimeProvider = timeProvider
    //     }
    // let appDialogModelCfg : AppDialogModel.Cfg =
    //     {
    //         UserSettings = userSettings
    //         RepositoryFactory = repositoryFactory
    //         MainErrorMessageQueue = mainErrorMessageQueue
    //     }

    // init
    let initMainModel () =
        let initCurrentWorkModel () =
            CurrentWorkModel.init userSettings (workEventStore.GetWorkRepository())

        MainModel.init userSettings initCurrentWorkModel

    // update
    let updateMainModel =
        // -------------------------
        // BotSettingsModel
        // -------------------------
        let initBotSettingsModel () =
            BotSettingsModel.init userSettings

        let updateBotSettingsModel =
            BotSettingsModel.Program.update userSettings

        // -------------------------
        // DatabaseSettingsModel
        // -------------------------
        let initDatabaseSettingsModel () =
            DatabaseSettingsModel.init userSettings

        let updateDatabaseSettingsModel =
            DatabaseSettingsModel.Program.update userSettings

        // -------------------------
        // WorkEventListModel
        // -------------------------
        let updateWorkEventListModel =
            WorkEventListModel.Program.update workEventStore dialogErrorMessageQueue (loggerFactory.CreateLogger<WorkEventListModel>())

        // -------------------------
        // DailyStatisticModel
        // -------------------------
        let updateDailyStatisticModel =
            DailyStatisticModel.Program.update
                timeProvider
                workEventStore
                excelBook
                dialogErrorMessageQueue
                (loggerFactory.CreateLogger<DailyStatisticModel>())

        // -------------------------
        // DailyStatisticListModel
        // -------------------------
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

        // -------------------------
        // RollbackWorkModel
        // -------------------------
        let updateRollbackWorkModel =
            RollbackWorkModel.Program.update userSettings

        // -------------------------
        // RollbackWorkListModel
        // -------------------------
        let updateRollbackWorkListModel =
            RollbackWorkListModel.Program.update updateRollbackWorkModel

        // -------------------------
        // AppDialogModel
        // -------------------------
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

        // -------------------------
        // WorkModel
        // -------------------------
        let updateWorkModel =
            WorkModel.Program.update workEventStore (loggerFactory.CreateLogger<WorkModel>()) mainErrorMessageQueue

        // -------------------------
        // WorkListModel
        // -------------------------
        let updateWorkListModel =
            WorkListModel.Program.update
                userSettings
                workEventStore
                (loggerFactory.CreateLogger<WorkListModel>())
                mainErrorMessageQueue
                updateWorkModel

        // -------------------------
        // CreatingWorkModel
        // -------------------------
        let updateCreatingWorkModel =
            CreatingWorkModel.Program.update workEventStore mainErrorMessageQueue (loggerFactory.CreateLogger<CreatingWorkModel>())

        // -------------------------
        // WorkSelectorModel
        // -------------------------
        let initWorkSelectorModel =
            WorkSelectorModel.init userSettings

        let updateWorkSelectorModel =
            WorkSelectorModel.Program.update updateWorkListModel updateCreatingWorkModel updateWorkModel (loggerFactory.CreateLogger<WorkSelectorModel>())

        // -------------------------
        // StatisticMainModel
        // -------------------------
        let initStatisticMainModel () =
            StatisticMainModel.init userSettings timeProvider

        let updateStatisticMainModel =
            StatisticMainModel.Program.update
                updateDailyStatisticListModel
                updateWorkListModel
                (loggerFactory.CreateLogger<StatisticMainModel>())

        // -------------------------
        // TimePointModel
        // -------------------------
        let updateTimePointModel =
            TimePointModel.Program.update
                timePointQueue
                looper
                mainErrorMessageQueue
                (loggerFactory.CreateLogger<TimePointModel>())

        // -------------------------
        // TimePointsGeneratorModel
        // -------------------------
        let initTimePointsGeneratorModel () =
            TimePointsGeneratorModel.init timePointPrototypeStore patternStore

        let updateTimePointsGeneratorModel =
            TimePointsGeneratorModel.Program.update
                patternStore
                timePointPrototypeStore
                timePointQueue
                dialogErrorMessageQueue
                (loggerFactory.CreateLogger<TimePointsGeneratorModel>())
                updateTimePointModel

        // -------------------------
        // RunningTimePointListModel
        // -------------------------
        let initRunningTimePointListModel =
            fun () ->
                RunningTimePointListModel.init timePointQueue userSettings

        let updateRunningTimePointListModel =
            RunningTimePointListModel.Program.update
                userSettings
                mainErrorMessageQueue
                (loggerFactory.CreateLogger<RunningTimePointListModel>())
                updateTimePointModel

        // -------------------------
        // CurrentWorkModel
        // -------------------------
        let updateCurrentWorkModel =
            CurrentWorkModel.Program.update
                userSettings
                workEventStore
                looper
                timeProvider
                telegramBot
                mainErrorMessageQueue
                (loggerFactory.CreateLogger<CurrentWorkModel>())

        // -------------------------
        // PlayerModel
        // -------------------------
        let updatePlayerModel =
            PlayerModel.Program.update
                looper
                windowsMinimizer
                timeProvider
                workEventStore 
                themeSwitcher 
                userSettings 
                timePointQueue 
                mainErrorMessageQueue 
                (loggerFactory.CreateLogger<PlayerModel>())

        // -------------------------
        // TimePointsDrawerModel
        // -------------------------
        let initWithTimePointsDrawerModel =
            TimePointsDrawerModel.initWithRunningTimePoints initRunningTimePointListModel

        let updateTimePointsDrawerModel =
            TimePointsDrawerModel.Program.update
                (loggerFactory.CreateLogger<TimePointsDrawerModel>())
                initRunningTimePointListModel
                updateRunningTimePointListModel
                initTimePointsGeneratorModel
                updateTimePointsGeneratorModel

        MainModel.Program.update
            telegramBot
            mainErrorMessageQueue
            (loggerFactory.CreateLogger<MainModel>())
            updatePlayerModel
            updateCurrentWorkModel
            initWithTimePointsDrawerModel
            updateTimePointsDrawerModel
            updateAppDialogModel
            initWorkSelectorModel
            updateWorkSelectorModel
            initStatisticMainModel
            updateStatisticMainModel

    // bindings:
    let ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version
    let assemblyVer =
        sprintf "Version: %i.%i.%i" ver.Major ver.Minor ver.Build

    do userSettings.CurrentVersion <- assemblyVer

    let mainModelBindings =
        fun () ->
            MainModel.Bindings.bindings title assemblyVer workStatisticWindowFactory mainErrorMessageQueue dialogErrorMessageQueue

    // subscriptions
    let subscribe _ : (SubId * Subscribe<_>) list =
        let looperSubscription dispatch =
            let onLooperEvt =
                fun evt ->
                    do dispatch (evt |> MainModel.Msg.LooperMsg)
            looper.AddSubscriber(onLooperEvt)
            { new IDisposable with 
                member _.Dispose() =
                    ()
            }

        let timePointQueueTimePointsChangedSubscription dispatch =
            let onTimePointChanged timePoints =
                do dispatch (timePoints |> MainModel.Msg.TimePointsChangedQueueMsg)
            timePointQueue.TimePointsChanged.Subscribe onTimePointChanged

        let timePointQueueTimePointsLoopComplettedSubscription dispatch =
            let onTimePointChanged () =
                do dispatch MainModel.Msg.TimePointsLoopComplettedQueueMsg
            timePointQueue.TimePointsLoopCompletted.Subscribe onTimePointChanged

        let playerUserSettingsSubscription dispatch =
            let onSettingsChanged () =
                do dispatch (MainModel.Msg.PlayerUserSettingsChanged)
            userSettings.PlayerUserSettingsChanged.Subscribe onSettingsChanged

        [
            ["Looper"], looperSubscription
            ["TimePointQueue.TimePointsChanged"], timePointQueueTimePointsChangedSubscription
            ["TimePointQueue.TimePointsLoopCompletted"], timePointQueueTimePointsLoopComplettedSubscription
            ["PlayerUserSettings"], playerUserSettingsSubscription
        ]

    (initMainModel, updateMainModel, mainModelBindings, subscribe)

