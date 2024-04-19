namespace PomodoroWindowsTimer.ElmishApp.Abstractions

open PomodoroWindowsTimer.Types

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

type IUserSettings =
    inherit IBotSettings
    inherit IPatternSettings
    inherit ITimePointPrototypesSettings
    inherit ITimePointSettings
    inherit IDisableSkipBreakSettings


/// Used in theme switcher on WPF side
type TimePointKind =
    | Undefined = 0
    | Work = 1
    | Break = 2

type IThemeSwitcher =
    interface
        abstract SwitchTheme: TimePointKind -> unit
    end


