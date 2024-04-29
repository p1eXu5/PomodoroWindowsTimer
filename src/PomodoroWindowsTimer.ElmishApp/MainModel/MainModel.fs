namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.Abstractions


type MainModel =
    {
        // BotSettingsModel: BotSettingsModel option
        // TimePointsGeneratorModel: TimePointsGeneratorModel option

        LooperState : LooperState
        LastCommandInitiator: UIInitiator option
        
        IsMinimized: bool
        DisableMinimizeMaximizeWindows: bool
        DisableSkipBreak: bool

        /// Left drawer model
        TimePoints: TimePoint list
        ActiveTimePoint : TimePoint option

        /// Right drawer model
        WorkSelector: WorkSelectorModel option
        Work: WorkModel option

        AppDialog: AppDialogModel
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
        UserSettings: IUserSettings
        SendToBot: BotSender
        Looper: Looper
        TimePointQueue: TimePointQueue
        WindowsMinimizer: WindowsMinimizer
        ThemeSwitcher: IThemeSwitcher
        TimePointStore: TimePointStore
        WorkRepository: IWorkRepository
    }
    member this.BotSettings = this.UserSettings :> IBotSettings
    member this.DisableSkipBreakSettings = this.UserSettings :> IDisableSkipBreakSettings
    member this.CurrentWorkItemSettings = this.UserSettings :> ICurrentWorkItemSettings


module MainModel =

    type Msg =
        | SetDisableSkipBreak of bool
        | SetDisableMinimizeMaximizeWindows of bool

        | LoadTimePointsFromSettings
        /// Stores and loads generated timepoints from prototypes.
        | LoadTimePoints of TimePoint list
        | StartTimePoint of Operation<Guid, unit>
        
        | LoadCurrentWork
        | SetCurrentWorkIfNone of Result<Work, string>
        | WorkModelMsg of WorkModel.Msg

        | PlayerMsg of PlayerMsg
        | LooperMsg of LooperEvent
        | WindowsMsg of WindowsMsg

        // | InitializeTimePointsGeneratorModel
        // // | TimePointsGeneratorMsg of TimePointsGeneratorModel.Msg
        // | EraseTimePointsGeneratorModel of isDrawerOpen: bool

        // | LoadBotSettingsModel
        // | BotSettingsMsg of BotSettingsModel.Msg
        | SendToChatBot
        
        | PreChangeActiveTimeSpan
        | ChangeActiveTimeSpan of float
        | PostChangeActiveTimeSpan

        | AppDialogModelMsg of AppDialogModel.Msg
        | WorkSelectorModelMsg of WorkSelectorModel.Msg
        
        | OnError of string
        | OnExn of exn
    and
        WindowsMsg =
            | MinimizeWindows
            | SetIsMinimized of bool
            | RestoreWindows
            | RestoreMainWindow
    and
        PlayerMsg =
            | Play
            | Next
            | Replay
            | Stop
            | Resume

    module Msg = 
        let tryNext (model: MainModel) =
            model.ActiveTimePoint
            |> Option.bind (fun atp ->
                match atp.Kind with
                | Kind.Break
                | Kind.LongBreak when model.DisableSkipBreak -> None
                | _ ->
                    PlayerMsg.Next |> Msg.PlayerMsg |> Some
            )

    module MsgWith =

        let (|WorkSelectorModelMsg|_|) (model: MainModel) (msg: Msg) =
            match msg, model.WorkSelector with
            | Msg.WorkSelectorModelMsg smsg, Some sm ->
                (smsg, sm) |> Some
            | _ -> None


    //    let (|TimePointsGeneratorMsg|_|) (model: MainModel) (msg: Msg) =
    //        match msg, model.TimePointsGeneratorModel with
    //        | Msg.TimePointsGeneratorMsg smsg, Some sm ->
    //            (smsg, sm) |> Some
    //        | _ -> None

    //    let (|BotSettingsMsg|_|) (model: MainModel) (msg: Msg) =
    //        match msg, model.BotSettingsModel with
    //        | Msg.BotSettingsMsg smsg, Some sm ->
    //            (smsg, sm) |> Some
    //        | _ -> None

    open Elmish

    let init (cfg: MainModeConfig) : MainModel * Cmd<Msg> =
        //let botSettingsModel = BotSettingsModel.init cfg.BotSettings
        //let (tpSettingsModel, tpSettingsModelCmd) = TimePointsGenerator.init cfg.TimePointPrototypeStore cfg.PatternStore

        {
            LooperState = Initialized
            LastCommandInitiator = None

            IsMinimized = false
            DisableMinimizeMaximizeWindows = false
            DisableSkipBreak = cfg.DisableSkipBreakSettings.DisableSkipBreak
            
            TimePoints = []
            ActiveTimePoint = None

            WorkSelector = None
            Work = None
            //BotSettingsModel = None
            //TimePointsGeneratorModel = None

            AppDialog = AppDialogModel.NoDialog
        }
        , Cmd.batch [
            Cmd.ofMsg Msg.LoadTimePointsFromSettings
            Cmd.ofMsg Msg.LoadCurrentWork
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

    let isUIInitiator (tp: TimePoint) m =
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

    let withWorkModel workModel (model: MainModel) =
         { model with Work = workModel }

    let withAppDialogModel addDialogModel (model: MainModel) =
         { model with AppDialog = addDialogModel }

    let withWorkSelectorModel workSelectorModel (model: MainModel) =
         { model with WorkSelector = workSelectorModel |> Some }

