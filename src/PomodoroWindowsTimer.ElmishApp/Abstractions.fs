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


type IBotConfiguration =
    interface
        abstract BotToken : string with get, set
        abstract MyChatId : string with get, set
    end


type TimePointKind =
    | Undefined = 0
    | Work = 1
    | Break = 2

type IThemeSwitcher =
    interface
        abstract SwitchTheme: TimePointKind -> unit
    end