namespace CycleBell.ElmishApp.Models

open CycleBell.ElmishApp.Abstractions
open CycleBell.Looper
open CycleBell.Types

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
    }


module MainModel =

    type Msg =
        | LooperMsg of LooperEvent
        | Play
        | Stop
        | OnError of exn
        | Minimize
        | PickFirstTimePoint


    open Elmish

    let init (settingsManager : ISettingsManager) (errorQueue : IErrorMessageQueue) timePoints : MainModel * Cmd<Msg> =

        let assemblyVer = "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString()

        {
            AssemblyVersion = assemblyVer
            SettingsManager = settingsManager
            ErrorQueue = errorQueue
            ActiveTimePoint = None
            LooperState = Stopped
            TimePoints = timePoints
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
        }