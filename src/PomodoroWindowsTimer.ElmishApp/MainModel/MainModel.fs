namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure


type MainModel =
    {
        Title: string
        AssemblyVersion: string
        SettingsManager : ISettingsManager
        ErrorQueue : IErrorMessageQueue
        ActiveTimePoint : TimePoint option
        LooperState : LooperState
        TimePoints: TimePoint list
        BotSettingsModel: BotSettingsModel
        TimePointsGeneratorModel: TimePointsGenerator
        DisableSkipBreak: bool
        IsMinimized: bool
        LastCommandInitiator: UIInitiator option
    }
and
    LooperState =
        | Initialized
        | Playing
        | Stopped
        | TimeShiftOnStopped of previosState: LooperState
and
    UIInitiator = UIInitiator of TimePoint


type MainModeConfig =
    {
        BotSettings: IBotSettings
        SendToBot: BotSender
        Looper: Looper
        TimePointQueue: TimePointQueue
        WindowsMinimizer: WindowsMinimizer
        ThemeSwitcher: IThemeSwitcher
        TimePointPrototypeStore: TimePointPrototypeStore
        TimePointStore: TimePointStore
        PatternStore: PatternStore
        DisableSkipBreakSettings: IDisableSkipBreakSettings
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
        | BotSettingsMsg of BotSettingsModel.Msg
        | InitializeTimePointsSettingsModel
        | TimePointsSettingsMsg of TimePointsGenerator.Msg
        | SetDisableSkipBreak of bool
        // test msgs
        | MinimizeWindows
        | SetIsMinimized of bool
        | RestoreWindows
        | RestoreMainWindow
        | SendToChatBot
        | SetActiveTimePoint of TimePoint option
        | SelectTimePoint of Guid option
        | PreChangeActiveTimeSpan
        | ChangeActiveTimeSpan of float
        | PostChangeActiveTimeSpan
        /// Stores and loads generated timepoints from prototypes.
        | TryStoreAndSetTimePoints
        | LoadTimePointsFromSettings


    open Elmish

    let initDefault () =
        {
            Title = "Pomodoro Windows Timer"
            AssemblyVersion = "Asm.v."
            SettingsManager = Unchecked.defaultof<_>
            ErrorQueue = Unchecked.defaultof<_>
            ActiveTimePoint = None
            LooperState = Initialized
            TimePoints = []
            BotSettingsModel = Unchecked.defaultof<_>
            TimePointsGeneratorModel = Unchecked.defaultof<_>
            DisableSkipBreak = false
            IsMinimized = false
            LastCommandInitiator = None
        }

    let init settingsManager errorQueue (cfg: MainModeConfig) : MainModel * Cmd<Msg> =

        let ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version
        let assemblyVer =
            sprintf "Version: %i.%i.%i" ver.Major ver.Minor ver.Build

        let botSettingsModel = BotSettingsModel.init cfg.BotSettings
        let (tpSettingsModel, tpSettingsModelCmd) = TimePointsGenerator.init cfg.TimePointPrototypeStore cfg.PatternStore

        { initDefault () with
            AssemblyVersion = assemblyVer
            SettingsManager = settingsManager
            ErrorQueue = errorQueue
            TimePoints = []
            BotSettingsModel = botSettingsModel
            TimePointsGeneratorModel = tpSettingsModel
            DisableSkipBreak = cfg.DisableSkipBreakSettings.DisableSkipBreak
        }
        , Cmd.batch [
            Cmd.ofMsg Msg.LoadTimePointsFromSettings
            Cmd.map Msg.TimePointsSettingsMsg tpSettingsModelCmd
        ]

    // =========
    // accessors
    // =========
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

    let isLooperRunning m =
        match m.LooperState with
        | TimeShiftOnStopped s ->
            match s with
            | Initialized -> false
            | _ -> true
        | Initialized -> false
        | _ -> true

    let isPlaying m =
        match m.LooperState with
        | Playing -> true
        | _ -> false

    let timePointKindEnum m =
        m.ActiveTimePoint
        |> Option.map (fun tp ->
            match tp.Kind with
            | Work -> TimePointKind.Work
            | Break -> TimePointKind.Break
            | LongBreak -> TimePointKind.Break
        )
        |> Option.defaultValue TimePointKind.Undefined

    let getActiveTimeSpan m =
        m.ActiveTimePoint
        |> Option.map (fun tp -> tp.TimeSpan)
        |> Option.defaultValue TimeSpan.Zero

    let getActiveSpentTime m =
        m.ActiveTimePoint
        |> Option.bind (fun atp ->
            m.TimePoints
            |> List.tryFind (fun tp -> tp.Id = atp.Id)
            |> Option.map (fun tp -> (tp.TimeSpan - atp.TimeSpan).TotalSeconds)
        )
        |> Option.defaultValue 0.0

    let getActiveTimeDuration m =
        m.ActiveTimePoint
        |> Option.bind (fun atp ->
            m.TimePoints
            |> List.tryFind (fun tp -> tp.Id = atp.Id)
        )
        |> Option.map (fun tp -> tp.TimeSpan.TotalSeconds)
        |> Option.defaultValue 0.0

    let getDisableSkipBreak m = m.DisableSkipBreak

    let nextMsg m =
        m.ActiveTimePoint
        |> Option.bind (fun atp ->
            match atp.Kind with
            | Kind.Break
            | Kind.LongBreak when m.DisableSkipBreak ->
                None
            | _ ->
                Msg.Next |> Some
        )