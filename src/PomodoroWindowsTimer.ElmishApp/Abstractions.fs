namespace PomodoroWindowsTimer.ElmishApp.Abstractions

open System
open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp

type IErrorMessageQueue =
    interface
        abstract EnqueueError : string -> unit
    end


type IBotSettings =
    interface
        abstract BotToken : string option with get, set
        abstract MyChatId : string option with get, set
    end

type IPatternSettings =
    interface
        abstract Patterns : Pattern list with get, set
    end

type ITimePointPrototypesSettings =
    interface
        abstract TimePointPrototypesSettings : string option with get, set
    end

type ITimePointSettings =
    interface
        abstract TimePointSettings : string option with get, set
    end

type IDisableSkipBreakSettings =
    interface
        abstract DisableSkipBreak : bool with get, set
    end

type ICurrentWorkItemSettings =
    interface
        abstract CurrentWork : Work option with get, set
    end

type IUserSettings =
    inherit IBotSettings
    inherit IPatternSettings
    inherit ITimePointPrototypesSettings
    inherit ITimePointSettings
    inherit IDisableSkipBreakSettings
    inherit ICurrentWorkItemSettings
    inherit IDatabaseSettings
    abstract LastStatisticPeriod: DateOnlyPeriod option with get, set
    // TODO: abstract RollbackWorkStrategy: RollbackWorkStrategy with get, set
    abstract LastDayCount: int with get, set
    abstract CurrentVersion: string with get, set


type IThemeSwitcher =
    interface
        abstract SwitchTheme: TimePointKind -> unit
    end

type ITelegramBot =
    abstract SendMessage: string -> Task<unit>

type IWindowsMinimizer =
    abstract GetIsMinimized: unit -> bool
    abstract MinimizeAllRestoreAppWindowAsync: unit -> unit
    abstract RestoreAllMinimized: unit -> unit
    abstract RestoreAppWindow: unit -> unit
    abstract AppWindowPtr: IntPtr with set


