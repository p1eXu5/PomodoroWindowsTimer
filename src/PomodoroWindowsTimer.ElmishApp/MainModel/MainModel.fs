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
        ActiveTimePoint : TimePoint option
        LooperState : LooperState
        TimePoints: TimePoint list
        BotSettingsModel: BotSettingsModel option
        TimePointsGeneratorModel: TimePointsGeneratorModel option
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
        TimePointStore: TimePointStore
        DisableSkipBreakSettings: IDisableSkipBreakSettings
    }


module MainModel =

    type Msg =
        | LoadTimePointsFromSettings
        /// Stores and loads generated timepoints from prototypes.
        | LoadTimePoints of TimePoint list
        | LooperMsg of LooperEvent
        | Play
        | Next
        | Replay
        | Stop
        | Resume
        | StartTimePoint of Operation<Guid, unit>
        | BotSettingsMsg of BotSettingsModel.Msg
        | TimePointsGeneratorMsg of TimePointsGeneratorModel.Msg
        | SetDisableSkipBreak of bool
        // test msgs
        | MinimizeWindows
        | SetIsMinimized of bool
        | RestoreWindows
        | RestoreMainWindow
        | SendToChatBot
        | SelectTimePoint of Guid option
        | PreChangeActiveTimeSpan
        | ChangeActiveTimeSpan of float
        | PostChangeActiveTimeSpan
        | InitializeTimePointsGeneratorModel
        | EraseTimePointsGeneratorModel of isDrawerOpen: bool
        | OnError of exn

    module MsgWith =

        let (|TimePointsGeneratorMsg|_|) (model: MainModel) (msg: Msg) =
            match msg, model.TimePointsGeneratorModel with
            | Msg.TimePointsGeneratorMsg smsg, Some sm ->
                (smsg, sm) |> Some
            | _ -> None

        let (|BotSettingsMsg|_|) (model: MainModel) (msg: Msg) =
            match msg, model.BotSettingsModel with
            | Msg.BotSettingsMsg smsg, Some sm ->
                (smsg, sm) |> Some
            | _ -> None

    open Elmish

    let init (cfg: MainModeConfig) : MainModel * Cmd<Msg> =
        //let botSettingsModel = BotSettingsModel.init cfg.BotSettings
        //let (tpSettingsModel, tpSettingsModelCmd) = TimePointsGenerator.init cfg.TimePointPrototypeStore cfg.PatternStore

        {
            ActiveTimePoint = None
            LooperState = Initialized
            TimePoints = []
            BotSettingsModel = None
            TimePointsGeneratorModel = None
            IsMinimized = false
            LastCommandInitiator = None
            DisableSkipBreak = cfg.DisableSkipBreakSettings.DisableSkipBreak
        }
        , Cmd.batch [
            Cmd.ofMsg Msg.LoadTimePointsFromSettings
            // Cmd.map Msg.TimePointsGeneratorMsg tpSettingsModelCmd
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