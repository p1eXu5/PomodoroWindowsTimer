module PomodoroWindowsTimer.ElmishApp.WorkSelectorModel.Program

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions.Helpers

open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkSelectorModel
open PomodoroWindowsTimer.ElmishApp.Abstractions

/// <summary>
/// Msg.WorkListModelMsg handler
/// </summary>
let private mapWorkListModelMsg updateWorkListModel smsg sm (model: WorkSelectorModel) =
    let (sm, cmd, intent) = updateWorkListModel smsg sm

    match intent with
    | WorkListModel.Intent.CloseDialogRequested
    | WorkListModel.Intent.None ->
        model |> withWorkListModel sm
        , Cmd.map Msg.WorkListModelMsg cmd
        , Intent.None

    | WorkListModel.Intent.SwitchToCreateWork canBeCancalling ->
        model
        |> withCreatingWorkModel (CreatingWorkModel.init canBeCancalling, sm.SelectedWorkId)
        , Cmd.none, Intent.None

    | WorkListModel.Intent.Select ->
        match sm |> WorkListModel.selectedWorkModel with
        | Some workModel ->
            model |> withWorkListModel sm
            , Cmd.map Msg.WorkListModelMsg cmd
            , Intent.SelectCurrentWork workModel
        | None ->
            model |> withWorkListModel sm
            , Cmd.map Msg.WorkListModelMsg cmd
            , Intent.UnselectCurrentWork

    | WorkListModel.Intent.Unselect ->
        model |> withWorkListModel sm
        , Cmd.map Msg.WorkListModelMsg cmd
        , Intent.UnselectCurrentWork

    | WorkListModel.Intent.Edit (workModel, selectedWorkId) ->
        model |> withUpdatingWorkModel workModel selectedWorkId |> withCmdNone |> withNoIntent

/// <summary>
/// Msg.CreatingWorkModelMsg handler.
/// </summary>
let private mapCreatingWorkModelMsg updateCreatingWorkModel smsg selectedWorkIdOpt sm (model: WorkSelectorModel) =
    let (sm, cmd, intent) = updateCreatingWorkModel smsg sm

    match intent with
    | CreatingWorkModel.Intent.None ->
        model |> withCreatingWorkModel (sm, selectedWorkIdOpt)
        , Cmd.map Msg.CreatingWorkModelMsg cmd
        , Intent.None

    | CreatingWorkModel.Intent.Cancel ->
        let (m, cmd) = WorkListModel.init selectedWorkIdOpt
        model |> withWorkListModel m
        , Cmd.map Msg.WorkListModelMsg cmd
        , Intent.None

    | CreatingWorkModel.Intent.Close ->
        model |> withCreatingWorkModel (sm, selectedWorkIdOpt)
        , Cmd.map Msg.CreatingWorkModelMsg cmd
        , Intent.Close

    | CreatingWorkModel.Intent.SwitchToWorkList newWorkId ->
        let (m, cmd) = WorkListModel.init (newWorkId |> Some)
        model |> withWorkListModel m
        , Cmd.batch [
            Cmd.map Msg.WorkListModelMsg cmd
            // Cmd.ofMsg (WorkListModel.Msg.SetSelectedWorkId (newWorkId |> Some) |> Msg.WorkListModelMsg)
        ]
        , Intent.None

/// <summary>
/// Msg.UpdatingWorkModelMsg handler.
/// </summary>
let private mapUpdatingWorkModelMsg updateWorkModel smsg selectedWorkId sm (model: WorkSelectorModel) =
    let (workModel, cmd, intent) = updateWorkModel smsg sm

    match intent with
    | WorkModel.Intent.EndEdit ->
        let (m, cmd) = WorkListModel.init selectedWorkId
        model |> withWorkListModel m
        , Cmd.map Msg.WorkListModelMsg cmd
        , Intent.SelectCurrentWork workModel

    | WorkModel.Intent.StartEdit
    | WorkModel.Intent.Select  // TODO: add ability to select work from edit mode
    | WorkModel.Intent.None ->
        model |> withUpdatingWorkModel workModel selectedWorkId
        , Cmd.map Msg.UpdatingWorkModelMsg cmd
        , Intent.None

/// <summary>
/// WorkSelectorModel update function.
/// </summary>
let update updateWorkListModel updateCreatingWorkModel updateWorkModel (logger: ILogger<WorkSelectorModel>) msg model =
    match msg, model.SubModel with
    | Msg.WorkListModelMsg smsg, WorkList sm ->
        model |> mapWorkListModelMsg updateWorkListModel smsg sm

    | Msg.CreatingWorkModelMsg smsg, CreatingWork (sm, selectedWorkIdOpt) ->
        model |> mapCreatingWorkModelMsg updateCreatingWorkModel smsg selectedWorkIdOpt sm

    | Msg.UpdatingWorkModelMsg smsg, UpdatingWork (workModel, selectedWorkId) ->
        model |> mapUpdatingWorkModelMsg updateWorkModel smsg selectedWorkId workModel

    | _ ->
        logger.LogNonProcessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent
