namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Models


type AppDialogModel =
    | NoDialog
    | BotSettingsDialog of BotSettingsModel
    | TimePointsGeneratorDialog of TimePointsGeneratorModel
    | WorkListModelDialog of WorkListModel


module AppDialogModel =

    type Msg =
        | LoadBotSettingsDialogModel
        | BotSettingsModelMsg of BotSettingsModel.Msg
        | LoadTimePointsGeneratorDialogModel
        | TimePointsGeneratorModelMsg of TimePointsGeneratorModel.Msg
        | SetIsDialogOpened of bool
        | EnqueueError of string
        | EnqueueExn of exn

    module MsgWith =
        let (|BotSettingsModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.BotSettingsModelMsg msg, AppDialogModel.BotSettingsDialog m ->
                (msg, m) |> Some
            | _ -> None

        let (|TimePointsGeneratorModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.TimePointsGeneratorModelMsg msg, AppDialogModel.TimePointsGeneratorDialog m ->
                (msg, m) |> Some
            | _ -> None

    [<Struct>]
    [<RequireQualifiedAccess>]
    type AppDialogId =
        | BotSettingsDialogId
        | TimePointsGeneratorDialogId
        | WorkListModelId

    let appDialogId = function
        | AppDialogModel.NoDialog -> None
        | AppDialogModel.BotSettingsDialog _ -> AppDialogId.BotSettingsDialogId |> Some
        | AppDialogModel.TimePointsGeneratorDialog _ -> AppDialogId.TimePointsGeneratorDialogId |> Some
        | AppDialogModel.WorkListModelDialog _ -> AppDialogId.WorkListModelId |> Some

    let botSettingsModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.BotSettingsDialog m -> m |> Some
        | _ -> None

    let timePointsGeneratorModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.TimePointsGeneratorDialog m -> m |> Some
        | _ -> None

    let workListModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.WorkListModelDialog m -> m |> Some
        | _ -> None

