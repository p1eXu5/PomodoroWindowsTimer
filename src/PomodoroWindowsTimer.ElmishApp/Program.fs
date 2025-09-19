module PomodoroWindowsTimer.ElmishApp.Program

open Microsoft.Extensions.Logging
open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Abstractions

[<Literal>]
let tickMilliseconds = 200<ms>

[<Literal>]
let title = "Pomodoro Windows Timer"

let main
    (window: System.Windows.Window)
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
    let (initMainModel, updateMainModel, mainModelBindings, subscribe) =
        CompositionRoot.compose
            title
            workStatisticWindowFactory
            looper
            timePointQueue
            workEventStore
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

    looper.Start()

