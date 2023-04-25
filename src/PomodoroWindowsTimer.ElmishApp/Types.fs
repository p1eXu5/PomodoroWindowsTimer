namespace PomodoroWindowsTimer.ElmishApp.Types

open System.Threading.Tasks
open PomodoroWindowsTimer.ElmishApp.Abstractions


type WindowsMinimizer =
    {
        Minimize: string -> Async<unit>
        Restore: unit -> Async<unit>
        RestoreMainWindow: string -> Async<unit>
    }

type Message = string

type BotSender = IBotConfiguration -> Message -> Task<unit>