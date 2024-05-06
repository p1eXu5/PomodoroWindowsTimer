namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Models


type AppDialogModel =
    | NoDialog
    | BotSettingsDialog of BotSettingsModel
    | TimePointsGeneratorDialog of TimePointsGeneratorModel
    | WorkStatisticsDialog of WorkStatisticListModel


module AppDialogModel =

    type Msg =
        | SetIsDialogOpened of bool

        | LoadBotSettingsDialogModel
        | BotSettingsModelMsg of BotSettingsModel.Msg
        
        | LoadTimePointsGeneratorDialogModel
        | TimePointsGeneratorModelMsg of TimePointsGeneratorModel.Msg
        
        | LoadWorkStatisticsDialogModel
        | WorkStatisticListModelMsg of WorkStatisticListModel.Msg
        
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

        let (|WorkStatisticListModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.WorkStatisticListModelMsg msg, AppDialogModel.WorkStatisticsDialog m ->
                (msg, m) |> Some
            | _ -> None

    [<Struct>]
    [<RequireQualifiedAccess>]
    type AppDialogId =
        | BotSettingsDialogId
        | TimePointsGeneratorDialogId
        | WorkStatisticsDialogId

    let appDialogId = function
        | AppDialogModel.NoDialog -> None
        | AppDialogModel.BotSettingsDialog _ -> AppDialogId.BotSettingsDialogId |> Some
        | AppDialogModel.TimePointsGeneratorDialog _ -> AppDialogId.TimePointsGeneratorDialogId |> Some
        | AppDialogModel.WorkStatisticsDialog _ -> AppDialogId.WorkStatisticsDialogId |> Some

    let botSettingsModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.BotSettingsDialog m -> m |> Some
        | _ -> None

    let timePointsGeneratorModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.TimePointsGeneratorDialog m -> m |> Some
        | _ -> None

    let workStatisticListModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.WorkStatisticsDialog m -> m |> Some
        | _ -> None

