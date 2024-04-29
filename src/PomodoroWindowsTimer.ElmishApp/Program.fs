module PomodoroWindowsTimer.ElmishApp.Program

open Microsoft.Extensions.Logging
open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Abstractions

[<Literal>]
let internal tickMilliseconds = 200<ms>

let internal main
    (window: System.Windows.Window)
    (workRepository: IWorkRepository)
    (themeSwitcher: IThemeSwitcher)
    (userSettings: IUserSettings)
    (errorMessageQueue: IErrorMessageQueue)
    (loggerFactory: ILoggerFactory)
    =
    let (initMainModel, updateMainModel, mainModelBindings, subscribe) =
        CompositionRoot.compose
            "Pomodoro Windows Timer"
            tickMilliseconds
            workRepository
            themeSwitcher
            userSettings
            errorMessageQueue
            loggerFactory

    WpfProgram.mkProgram
        initMainModel
        updateMainModel
        mainModelBindings
    |> WpfProgram.withLogger loggerFactory
    |> WpfProgram.withSubscription subscribe
    |> WpfProgram.startElmishLoop window
