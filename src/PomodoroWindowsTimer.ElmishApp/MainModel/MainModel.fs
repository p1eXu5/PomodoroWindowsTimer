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
    }


module MainModel =

    type Msg =
        | LooperMsg of LooperEvent
        | Play
        | Stop
        | OnError of exn
        | PickFirstTimePoint
        | StartTimePoint of Operation<Guid, unit>
        | BotSettingsModelMsg of BotSettingsModel.Msg
        // test msgs
        | Minimize
        | SendToChatBot


    open Elmish

    let init (settingsManager : ISettingsManager) (botConfiguration: IBotConfiguration) (errorQueue : IErrorMessageQueue) timePoints : MainModel * Cmd<Msg> =

        let assemblyVer = "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString()

        {
            AssemblyVersion = assemblyVer
            SettingsManager = settingsManager
            ErrorQueue = errorQueue
            ActiveTimePoint = None
            LooperState = Stopped
            TimePoints = timePoints
            BotSettingsModel = BotSettingsModel.init botConfiguration
        }
        , Cmd.ofMsg Msg.PickFirstTimePoint


    let initForDesign () =
        {
            AssemblyVersion = "Asm.v."
            SettingsManager = Unchecked.defaultof<_>
            ErrorQueue = Unchecked.defaultof<_>
            ActiveTimePoint = None
            LooperState = Stopped
            TimePoints = []
            BotSettingsModel = Unchecked.defaultof<_>
        }