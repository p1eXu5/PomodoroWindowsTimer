﻿namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Models
open System
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp

[<RequireQualifiedAccess>]
type AppDialogModel =
    | NoDialog
    | BotSettings of BotSettingsModel
    | RollbackWork of RollbackWorkModel
    | RollbackWorkList of RollbackWorkListModel
    | SkipOrApplyMissingTimeDialog of RollbackWorkModel

module AppDialogModel =

    type Cfg =
        {
            UserSettings: IUserSettings
            WorkEventRepository: IWorkEventRepository
            MainErrorMessageQueue: IErrorMessageQueue
        }

    type Msg =
        | SetIsDialogOpened of bool

        | LoadBotSettingsDialogModel
        | BotSettingsModelMsg of BotSettingsModel.Msg

        | LoadRollbackWorkDialogModel of WorkSpentTime * Kind * DateTimeOffset
        | RollbackWorkModelMsg of RollbackWorkModel.Msg

        | LoadRollbackWorkListDialogModel of WorkSpentTime list * Kind * DateTimeOffset
        | RollbackWorkListModelMsg of RollbackWorkListModel.Msg

        | LoadSkipOrApplyMissingTimeDialogModel of WorkId * Kind * TimeSpan * DateTimeOffset
        | SkipOrApplyMissingTimeModelMsg of RollbackWorkModel.Msg

        | Unload

        | EnqueueError of string
        | EnqueueExn of exn

    module MsgWith =
        let (|BotSettingsModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.BotSettingsModelMsg msg, AppDialogModel.BotSettings m ->
                (msg, m) |> Some
            | _ -> None

        let (|RollbackWorkModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.RollbackWorkModelMsg msg, AppDialogModel.RollbackWork m ->
                (msg, m) |> Some
            | _ -> None

        let (|RollbackWorkListModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.RollbackWorkListModelMsg msg, AppDialogModel.RollbackWorkList m ->
                (msg, m) |> Some
            | _ -> None

        let (|SkipOrApplyMissingTimeModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.SkipOrApplyMissingTimeModelMsg msg, AppDialogModel.SkipOrApplyMissingTimeDialog m ->
                (msg, m) |> Some
            | _ -> None

    [<Struct>]
    [<RequireQualifiedAccess>]
    type AppDialogId =
        | BotSettingsDialogId
        | TimePointsGeneratorDialogId
        | WorkStatisticsDialogId
        | RollbackWorkDialogId
        | RollbackWorkListDialogId
        | SkipOrApplyMissingTimeDialogId

    let appDialogId = function
        | AppDialogModel.NoDialog -> None
        | AppDialogModel.BotSettings _ -> AppDialogId.BotSettingsDialogId |> Some
        | AppDialogModel.RollbackWork _ -> AppDialogId.RollbackWorkDialogId |> Some
        | AppDialogModel.RollbackWorkList _ -> AppDialogId.RollbackWorkListDialogId |> Some
        | AppDialogModel.SkipOrApplyMissingTimeDialog _ -> AppDialogId.SkipOrApplyMissingTimeDialogId |> Some

    let tryBotSettingsModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.BotSettings m -> m |> Some
        | _ -> None

    let tryRollbackWorkModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.RollbackWork m -> m |> Some
        | _ -> None

    let tryRollbackWorkListModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.RollbackWorkList m -> m |> Some
        | _ -> None

    let trySkipOrApplyMissingTimeModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.SkipOrApplyMissingTimeDialog m -> m |> Some
        | _ -> None


