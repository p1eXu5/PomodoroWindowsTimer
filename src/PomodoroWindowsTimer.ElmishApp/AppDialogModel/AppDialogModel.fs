namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Models

type AppDialogModel =
    {
        IsOpened: bool
        Dialog: AppDialogSubModel
    }
and AppDialogSubModel =
    | NoDialog
    | BotSettingsDialog of BotSettingsModel
    | TimePointsGeneratorDialog of TimePointsGeneratorModel
    | WorkStatisticsDialog of WorkStatisticListModel


module AppDialogModel =

    type Msg =
        | SetIsDialogOpened of bool

        | LoadBotSettingsDialogModel
        | BotSettingsModelMsg of BotSettingsModel.Msg
        | ApplyBotSettings
        
        | LoadTimePointsGeneratorDialogModel
        | TimePointsGeneratorModelMsg of TimePointsGeneratorModel.Msg
        
        | LoadWorkStatisticsDialogModel
        | WorkStatisticListModelMsg of WorkStatisticListModel.Msg
        
        | Unload

        | EnqueueError of string
        | EnqueueExn of exn

    module MsgWith =
        let (|BotSettingsModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model.Dialog with
            | Msg.BotSettingsModelMsg msg, AppDialogSubModel.BotSettingsDialog m ->
                (msg, m) |> Some
            | _ -> None

        let (|ApplyBotSettings|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model.Dialog with
            | Msg.ApplyBotSettings, AppDialogSubModel.BotSettingsDialog m ->
                m |> Some
            | _ -> None

        let (|TimePointsGeneratorModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model.Dialog with
            | Msg.TimePointsGeneratorModelMsg msg, AppDialogSubModel.TimePointsGeneratorDialog m ->
                (msg, m) |> Some
            | _ -> None

        let (|WorkStatisticListModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model.Dialog with
            | Msg.WorkStatisticListModelMsg msg, AppDialogSubModel.WorkStatisticsDialog m ->
                (msg, m) |> Some
            | _ -> None

    [<Struct>]
    [<RequireQualifiedAccess>]
    type AppDialogId =
        | BotSettingsDialogId
        | TimePointsGeneratorDialogId
        | WorkStatisticsDialogId

    let appDialogId (model: AppDialogModel) =
        match model.Dialog with
        | AppDialogSubModel.NoDialog -> None
        | AppDialogSubModel.BotSettingsDialog _ -> AppDialogId.BotSettingsDialogId |> Some
        | AppDialogSubModel.TimePointsGeneratorDialog _ -> AppDialogId.TimePointsGeneratorDialogId |> Some
        | AppDialogSubModel.WorkStatisticsDialog _ -> AppDialogId.WorkStatisticsDialogId |> Some

    let botSettingsModel (model: AppDialogModel) =
        match model.Dialog with
        | AppDialogSubModel.BotSettingsDialog m -> m |> Some
        | _ -> None

    let withBotSettingsDialogModel botSettingsDialogModel (model: AppDialogModel) =
        { model with Dialog = botSettingsDialogModel |> AppDialogSubModel.BotSettingsDialog }

    let timePointsGeneratorModel (model: AppDialogModel) =
        match model.Dialog with
        | AppDialogSubModel.TimePointsGeneratorDialog m -> m |> Some
        | _ -> None

    let withTimePointsGeneratorDialogModel timePointsGeneratorModel (model: AppDialogModel) =
        { model with Dialog = timePointsGeneratorModel |> AppDialogSubModel.TimePointsGeneratorDialog }

    let workStatisticListModel (model: AppDialogModel) =
        match model.Dialog with
        | AppDialogSubModel.WorkStatisticsDialog m -> m |> Some
        | _ -> None

    let withWorkStatisticsDialogModel workStatisticListModel (model: AppDialogModel) =
        { model with Dialog = workStatisticListModel |> AppDialogSubModel.WorkStatisticsDialog }

    let withIsOpened v (model: AppDialogModel) =
        { model with IsOpened = v }

    let withNoDialog (model: AppDialogModel) =
        { model with Dialog = AppDialogSubModel.NoDialog }
