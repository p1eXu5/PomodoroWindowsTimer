namespace PomodoroWindowsTimer.Abstractions

open System.Threading.Tasks


type IBotSettings =
    interface
        abstract BotToken : string option with get, set
        abstract MyChatId : string option with get, set
    end


type ITelegramBot =
    abstract SendMessage: string -> Task<unit>
