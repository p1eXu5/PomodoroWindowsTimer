module PomodoroWindowsTimer.ElmishApp.Program

open Microsoft.Extensions.Logging
open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Abstractions

[<Literal>]
let internal tickMilliseconds = 200<ms>

[<Literal>]
let internal title = "Pomodoro Windows Timer"

let internal main
    (window: System.Windows.Window)
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
    (excelBook: IExcelBook)
    (loggerFactory: ILoggerFactory)
    =
    let (initMainModel, updateMainModel, mainModelBindings, subscribe) =
        CompositionRoot.compose
            title
            workStatisticWindowFactory
            looper
            timePointQueue
            workRepository
            workEventRepository
            telegramBot
            windowsMinimizer
            themeSwitcher
            userSettings
            mainErrorMessageQueue
            dialogErrorMessageQueue
            timeProvider
            excelBook
            loggerFactory

    WpfProgram.mkProgram
        initMainModel
        updateMainModel
        mainModelBindings
    |> WpfProgram.withLogger loggerFactory
    |> WpfProgram.withSubscription subscribe
    |> WpfProgram.startElmishLoop window

