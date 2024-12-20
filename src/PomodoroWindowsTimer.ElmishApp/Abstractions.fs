namespace PomodoroWindowsTimer.ElmishApp.Abstractions

open System
open PomodoroWindowsTimer.ElmishApp

type IErrorMessageQueue =
    interface
        abstract EnqueueError : string -> unit
    end


type IThemeSwitcher =
    interface
        abstract SwitchTheme: TimePointKind -> unit
    end


type IWindowsMinimizer =
    abstract GetIsMinimized: unit -> bool
    abstract MinimizeAllRestoreAppWindowAsync: unit -> unit
    abstract RestoreAllMinimized: unit -> unit
    abstract RestoreAppWindow: unit -> unit
    abstract AppWindowPtr: IntPtr with set


