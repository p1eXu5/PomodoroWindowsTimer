namespace PomodoroWindowsTimer.ElmishApp.Types

open System.Threading.Tasks
open PomodoroWindowsTimer.ElmishApp.Abstractions


type WindowsMinimizer =
    {
        Minimize: unit -> Async<unit>
        Restore: unit -> Async<unit>
        RestoreMainWindow: unit -> Async<unit>
    }

type Message = string

type BotSender = IBotConfiguration -> Message -> Task<unit>