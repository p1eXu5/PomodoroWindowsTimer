namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.Types
open System
open Elmish.Extensions

type LooperState =
    | Playing
    | Stopped

type MainModel =
    {
        AssemblyVersion: string
        SettingsManager : ISettingsManager
        ErrorQueue : IErrorMessageQueue
        ActiveTimePoint : TimePoint option
        LooperState : LooperState
        TimePoints: TimePoint list
        BotSettingsModel: BotSettingsModel
        IsMinimized: bool
    }


module MainModel =

    type Msg =
        | LooperMsg of LooperEvent
        | Play
        | Next
        | Replay
        | Stop
        | Resume
        | OnError of exn
        | PickFirstTimePoint
        | StartTimePoint of Operation<Guid, unit>
        | BotSettingsModelMsg of BotSettingsModel.Msg
        // test msgs
        | MinimizeWindows
        | SetIsMinimized of bool
        | RestoreWindows
        | RestoreMainWindow
        | SendToChatBot


    open Elmish

    let initDefault () =
        {
            AssemblyVersion = "Asm.v."
            SettingsManager = Unchecked.defaultof<_>
            ErrorQueue = Unchecked.defaultof<_>
            ActiveTimePoint = None
            LooperState = Stopped
            TimePoints = []
            BotSettingsModel = Unchecked.defaultof<_>
            IsMinimized = false
        }

    let init (settingsManager : ISettingsManager) (botConfiguration: IBotConfiguration) (errorQueue : IErrorMessageQueue) timePoints : MainModel * Cmd<Msg> =

        let assemblyVer = "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString()

        { initDefault () with
            AssemblyVersion = assemblyVer
            SettingsManager = settingsManager
            ErrorQueue = errorQueue
            TimePoints = timePoints
            BotSettingsModel = BotSettingsModel.init botConfiguration
        }
        , Cmd.ofMsg Msg.PickFirstTimePoint
