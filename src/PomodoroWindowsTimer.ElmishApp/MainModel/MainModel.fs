namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open System.Threading


type MainModel =
    {
        // BotSettingsModel: BotSettingsModel option
        // TimePointsGeneratorModel: TimePointsGeneratorModel option
        
        /// Left drawer model
        // TimePointList: TimePointListModel

        // Generator can be preserved when drawer is closing
        IsTimePointsDrawerShown: bool
        TimePointsDrawer: TimePointsDrawerModel

        CurrentWork: CurrentWorkModel
        Player: PlayerModel

        /// Right drawer model
        WorkSelector: WorkSelectorModel option

        /// Statistic window
        StatisticMainModel: StatisticMainModel option

        AppDialog: AppDialogModel
    }

module MainModel =

    [<RequireQualifiedAccess>]
    type Msg =
        | SetIsTimePointsDrawerShown of bool
        | TimePointsDrawerMsg of TimePointsDrawerModel.Msg
        | TimePointQueueMsg of TimePoint list * TimePointId option
        | TimePointsLoopComplettedQueueMsg
        | StartTimePoint of TimePointId

        | PlayStopCommand of initiator: TimePointId
        | LooperMsg of LooperEvent
        | PlayerModelMsg of PlayerModel.Msg
        | PlayerUserSettingsChanged

        | CurrentWorkModelMsg of CurrentWorkModel.Msg
        | SetIsWorkSelectorLoaded of bool
        | WorkSelectorModelMsg of WorkSelectorModel.Msg

        | AppDialogModelMsg of AppDialogModel.Msg
        | SetIsWorkStatisticShown of bool
        | StatisticMainModelMsg of StatisticMainModel.Msg

        | SendToChatBot of Message


        // | SetDisableMinimizeMaximizeWindows of bool
        // | LoadTimePointsFromSettings
        /// Stores and loads generated timepoints from prototypes.
        // | LoadTimePoints of TimePoint list
        // | LoadCurrentWork

        | OnError of string
        | OnExn of exn

        /// Using in test
        | Terminate

    module MsgWith =

        let (|WorkSelectorModelMsg|_|) (model: MainModel) (msg: Msg) =
            match msg, model.WorkSelector with
            | Msg.WorkSelectorModelMsg smsg, Some sm ->
                (smsg, sm) |> Some
            | _ -> None

        let (|StatisticMainModelMsg|_|) (model: MainModel) (msg: Msg) =
            match model.StatisticMainModel with
            | Some sm ->
                match msg with
                | Msg.StatisticMainModelMsg smsg -> (smsg, sm) |> Some
                | _ -> None
            | _ -> None

        //let (|TimePointsDrawerMsg|_|) (model: MainModel) (msg: Msg) =
        //    match model.TimePointsDrawer, msg with
        //    | Some subModel, Msg.TimePointsDrawerMsg subMsg ->
        //        (subMsg, subModel) |> Some
        //    | _ -> None

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

    let init
        (userSettings: IUserSettings)
        initCurrentWorkModel
        : MainModel * Cmd<Msg>
        =
        let (currentWorkModel, currentWorkCmd) = initCurrentWorkModel ()
        {
            // TimePointList = TimePointListModel.init timePoints
            IsTimePointsDrawerShown = false

            TimePointsDrawer = TimePointsDrawerModel.None

            Player = PlayerModel.init userSettings

            WorkSelector = None
            CurrentWork = currentWorkModel
            //BotSettingsModel = None
            //TimePointsGeneratorModel = None

            StatisticMainModel = None

            AppDialog = AppDialogModel.NoDialog
        }
        , Cmd.map Msg.CurrentWorkModelMsg currentWorkCmd


    // =========
    // accessors
    // =========
    let withCurrentWorkModel currentWorkModel (model: MainModel) =
         { model with CurrentWork = currentWorkModel }

    let withAppDialogModel addDialogModel (model: MainModel) =
         { model with AppDialog = addDialogModel }

    let withWorkSelectorModel workSelectorModel (model: MainModel) =
         { model with WorkSelector = workSelectorModel |> Some }

    let withoutWorkSelectorModel (model: MainModel) =
         { model with WorkSelector = None }

    let withIsTimePointsDrawerShown initRunningTimePoints v (model: MainModel) =
        // TODO: clear drower when closed
        match v, model.TimePointsDrawer with
        | true, TimePointsDrawerModel.None ->
            { model with
                IsTimePointsDrawerShown = v;
                TimePointsDrawer = initRunningTimePoints () |> TimePointsDrawerModel.RunningTimePoints }
        | _ -> { model with IsTimePointsDrawerShown = v; }

    let withTimePointsDrawer drawerModelOpt (model: MainModel) =
         { model with TimePointsDrawer = drawerModelOpt }

    let withDailyStatisticList workStatisticListModel (model: MainModel) =
        { model with StatisticMainModel = workStatisticListModel }

    let withPlayerModel playerModel (model: MainModel) =
        { model with Player = playerModel }

    //let withInitTimePointListModel timePoints (model: MainModel) =
    //    { model with TimePointList = TimePointListModel.init timePoints }

    //let withTimePointListModel timePointListModel (model: MainModel) =
    //    { model with TimePointList = timePointListModel }

    // // TODO: add TimePointListModel
    // let withTimePoints timePoints cfg (model: MainModel) =
    //     let model = model |> withStoppedLooper cfg
    //     cfg.TimePointQueue.Reload(timePoints)
    //     cfg.TimePointStore.Write(timePoints)
    //     cfg.Looper.PreloadTimePoint()
    //     { model with TimePoints = timePoints }

