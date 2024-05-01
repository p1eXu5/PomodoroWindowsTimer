namespace PomodoroWindowsTimer.ElmishApp.Abstractions

open System.Threading.Tasks
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


type IThemeSwitcher =
    interface
        abstract SwitchTheme: TimePointKind -> unit
    end

type ITelegramBot =
    abstract SendMessage: string -> Task<unit>

type IWindowsMinimizer =
    abstract MinimizeOtherAsync: unit -> Task<unit>
    abstract Restore: unit -> unit
    abstract RestoreMainWindow: unit -> unit

