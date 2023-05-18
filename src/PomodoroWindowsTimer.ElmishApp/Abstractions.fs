namespace PomodoroWindowsTimer.ElmishApp.Abstractions

type ISettingsManager =
    interface
        abstract Load : key: string -> obj
        abstract Save : key: string -> value: obj -> unit
    end


type IErrorMessageQueue =
    interface
        abstract EnqueuError : string -> unit
    end


type IBotSettings =
    interface
        abstract BotToken : string option with get, set
        abstract MyChatId : string option with get, set
    end

/// Used in theme switcher on WPF side
type TimePointKind =
    | Undefined = 0
    | Work = 1
    | Break = 2

type IThemeSwitcher =
    interface
        abstract SwitchTheme: TimePointKind -> unit
    end


type ITimePointPrototypesSettings =
    interface
        abstract TimePointPrototypesSettings : string option with get, set
    end


type ITimePointSettings =
    interface
        abstract TimePointSettings : string option with get, set
    end


type IPatternSettings =
    interface
        abstract Read: unit -> string list
        abstract Write: string list -> unit
    end


type IDisableSkipBreakSettings =
    interface
        abstract DisableSkipBreak : bool with get, set
    end