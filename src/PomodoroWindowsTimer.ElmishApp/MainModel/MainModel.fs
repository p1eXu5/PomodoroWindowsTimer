namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.ElmishApp
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
        IsTimePointsShown: bool
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
        SendToBot: ITelegramBot
        Looper: Looper
        TimePointQueue: TimePointQueue
        WindowsMinimizer: IWindowsMinimizer
        ThemeSwitcher: IThemeSwitcher
        TimePointStore: TimePointStore
        WorkRepository: IWorkRepository
        WorkEventRepository: IWorkEventRepository
    }
    member this.BotSettings = this.UserSettings :> IBotSettings
    member this.DisableSkipBreakSettings = this.UserSettings :> IDisableSkipBreakSettings
    member this.CurrentWorkItemSettings = this.UserSettings :> ICurrentWorkItemSettings


module MainModel =

    [<RequireQualifiedAccess>]
    type Msg =
        | SetDisableSkipBreak of bool
        | SetDisableMinimizeMaximizeWindows of bool

        | LoadTimePointsFromSettings
        /// Stores and loads generated timepoints from prototypes.
        | LoadTimePoints of TimePoint list
        | StartTimePoint of Operation<Guid, unit>
        | SetIsTimePointsShown of bool

        | PlayerMsg of PlayerMsg
        | WindowsMsg of WindowsMsg
        | SendToChatBot of Message

        | AppDialogModelMsg of AppDialogModel.Msg

        | LoadCurrentWork
        | SetCurrentWorkIfNone of Result<Work, string>
        | WorkModelMsg of WorkModel.Msg

        | SetIsWorkSelectorLoaded of bool
        | WorkSelectorModelMsg of WorkSelectorModel.Msg
        
        | OnError of string
        | OnExn of exn

        /// Using in test
        | Terminate
    and
        WindowsMsg =
            | MinimizeWindows
            | SetIsMinimized of bool
            | RestoreWindows
            | RestoreMainWindow
    and
        [<RequireQualifiedAccess>]
        PlayerMsg =
            | Play
            | Next
            | Replay
            | Stop
            | Resume
            | LooperMsg of LooperEvent
            | PreChangeActiveTimeSpan
            | ChangeActiveTimeSpan of float
            | PostChangeActiveTimeSpan

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

            IsTimePointsShown = false
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

    let getActiveTimeKind m =
        m.ActiveTimePoint
        |> Option.bind (fun atp ->
            m.TimePoints
            |> List.tryFind (fun tp -> tp.Id = atp.Id)
            |> Option.map (fun tp -> tp.Kind)
        )
        |> Option.defaultValue Kind.Work

    let withWorkModel workModel (model: MainModel) =
         { model with Work = workModel }

    let withAppDialogModel addDialogModel (model: MainModel) =
         { model with AppDialog = addDialogModel }

    let withWorkSelectorModel workSelectorModel (model: MainModel) =
         { model with WorkSelector = workSelectorModel }

    let withoutWorkSelectorModel (model: MainModel) =
         { model with WorkSelector = None }

    let withIsTimePointsShown v (model: MainModel) =
         { model with IsTimePointsShown = v }

    let withStoppedLooper cfg (model: MainModel) =
        match model.LooperState with
        | LooperState.Playing ->
            cfg.Looper.Stop()
            model |> setLooperState Stopped
        | _ -> model

    let withShiftedActiveTimePoint cfg (model: MainModel) =
        match model.ActiveTimePoint with
        | Some activeTp ->
            cfg.Looper.Shift(activeTp.TimeSpan.TotalSeconds * 1.0<sec>)
            model
        | _ ->
            model

    let withTimePoints timePoints cfg (model: MainModel) =
        let model = model |> withStoppedLooper cfg
        cfg.TimePointQueue.Reload(timePoints)
        cfg.TimePointStore.Write(timePoints)
        cfg.Looper.PreloadTimePoint()
        { model with TimePoints = timePoints }

