namespace PomodoroWindowsTimer.ElmishApp.Models


module TimePointPrototypeModel =
    open System

    type Msg =
        | SetTimeSpan of TimeSpan

