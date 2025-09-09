﻿namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure


type MainModel =
    {
        // BotSettingsModel: BotSettingsModel option
        // TimePointsGeneratorModel: TimePointsGeneratorModel option
        
        /// Left drawer model
        TimePointList: TimePointListModel
        IsTimePointsShown: bool

        Player: PlayerModel

        /// Right drawer model
        WorkSelector: WorkSelectorModel option
        CurrentWork: WorkModel option

        /// Statistic window
        StatisticMainModel: StatisticMainModel option

        AppDialog: AppDialogModel
    }

type MainModeConfig =
    {
        UserSettings: IUserSettings
        TelegramBot: ITelegramBot
        Looper: ILooper
        TimePointQueue: ITimePointQueue
        WindowsMinimizer: IWindowsMinimizer
        ThemeSwitcher: IThemeSwitcher
        TimePointStore: TimePointStore
        WorkEventStore: WorkEventStore
        TimeProvider: System.TimeProvider
    }
    member this.BotSettings = this.UserSettings :> IBotSettings
    member this.DisableSkipBreakSettings = this.UserSettings :> IDisableSkipBreakSettings
    member this.CurrentWorkItemSettings = this.UserSettings :> ICurrentWorkItemSettings


module MainModel =

    [<RequireQualifiedAccess>]
    type Msg =
        | TimePointListModelMsg of TimePointListModel.Msg

        | StartTimePoint of TimePointId
        | PlayStopCommand of initiator: TimePointId

        | LooperMsg of LooperEvent

        | PlayerModelMsg of PlayerModel.Msg
        | SetDisableMinimizeMaximizeWindows of bool

        | LoadTimePointsFromSettings
        /// Stores and loads generated timepoints from prototypes.
        | LoadTimePoints of TimePoint list
        | SetIsTimePointsShown of bool

        | SendToChatBot of Message

        | AppDialogModelMsg of AppDialogModel.Msg

        | LoadCurrentWork
        | SetCurrentWorkIfNone of Result<Work, string>
        | WorkModelMsg of WorkModel.Msg

        | SetIsWorkSelectorLoaded of bool
        | WorkSelectorModelMsg of WorkSelectorModel.Msg

        | SetIsWorkStatisticShown of bool
        | StatisticMainModelMsg of StatisticMainModel.Msg
        
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

    let init (userSettings: IUserSettings) : MainModel * Cmd<Msg> =
        //let botSettingsModel = BotSettingsModel.init cfg.BotSettings
        //let (tpSettingsModel, tpSettingsModelCmd) = TimePointsGenerator.init cfg.TimePointPrototypeStore cfg.PatternStore
        {
            TimePointList = TimePointListModel.init []
            IsTimePointsShown = false

            Player = PlayerModel.init userSettings

            WorkSelector = None
            CurrentWork = None
            //BotSettingsModel = None
            //TimePointsGeneratorModel = None

            StatisticMainModel = None

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
    let withWorkModel workModel (model: MainModel) =
         { model with CurrentWork = workModel }

    let withAppDialogModel addDialogModel (model: MainModel) =
         { model with AppDialog = addDialogModel }

    let withWorkSelectorModel workSelectorModel (model: MainModel) =
         { model with WorkSelector = workSelectorModel }

    let withoutWorkSelectorModel (model: MainModel) =
         { model with WorkSelector = None }

    let withIsTimePointsShown v (model: MainModel) =
         { model with IsTimePointsShown = v }

    let withDailyStatisticList workStatisticListModel (model: MainModel) =
        { model with StatisticMainModel = workStatisticListModel }

    let withPlayerModel playerModel (model: MainModel) =
        { model with Player = playerModel }

    let withInitTimePointListModel timePoints (model: MainModel) =
        { model with TimePointList = TimePointListModel.init timePoints }

    let withTimePointListModel timePointListModel (model: MainModel) =
        { model with TimePointList = timePointListModel }

    // // TODO: add TimePointListModel
    // let withTimePoints timePoints cfg (model: MainModel) =
    //     let model = model |> withStoppedLooper cfg
    //     cfg.TimePointQueue.Reload(timePoints)
    //     cfg.TimePointStore.Write(timePoints)
    //     cfg.Looper.PreloadTimePoint()
    //     { model with TimePoints = timePoints }

