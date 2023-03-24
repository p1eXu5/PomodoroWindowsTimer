namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.Types
open System
open Elmish.Extensions


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
        LastCommandInitiator: UIInitiator option
    }
and
    LooperState =
        | Playing
        | Stopped
and
    UIInitiator = UIInitiator of TimePoint

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
            LastCommandInitiator = None
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


    let setLooperState state m = { m with LooperState = state }

    let setUIInitiator m  =
        m.ActiveTimePoint
        |> Option.map (fun tp ->
            { m with LastCommandInitiator = tp |> UIInitiator |> Some }
        )
        |> Option.defaultValue m

    let setActiveTimePoint tp m = { m with ActiveTimePoint = tp; LastCommandInitiator = None }

    let isUIInitiator tp m =
        match m.LastCommandInitiator with
        | Some (UIInitiator atp) -> atp.Id = tp.Id
        | _ -> false