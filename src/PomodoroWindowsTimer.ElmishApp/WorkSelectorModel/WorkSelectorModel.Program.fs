module PomodoroWindowsTimer.ElmishApp.WorkSelectorModel.Program

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions.Helpers

open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkSelectorModel

let update updateWorkListModel updateCreatingWorkModel updateWorkModel (logger: ILogger<WorkSelectorModel>) msg model =
    match msg, model.SubModel with
    | Msg.WorkListModelMsg smsg, WorkList sm ->
        let (sm, cmd, intent) = updateWorkListModel smsg sm

        match intent with
        | WorkListModel.Intent.None ->
            model |> withWorkListModel sm
            , Cmd.map Msg.WorkListModelMsg cmd
            , Intent.None

        | WorkListModel.Intent.SwitchToCreateWork ->
            model
            |> withCreatingWorkModel (CreatingWorkModel.init ())
            |> withCmdNone
            |> withNoIntent

        | WorkListModel.Intent.Select ->
            model |> withWorkListModel sm
            , Cmd.map Msg.WorkListModelMsg cmd
            , Intent.SelectCurrentWork (sm |> WorkListModel.selectedWorkModel)

        | WorkListModel.Intent.Unselect ->
            model |> withWorkListModel sm
            , Cmd.map Msg.WorkListModelMsg cmd
            , Intent.UnselectCurrentWork

        | WorkListModel.Intent.Edit (workModel, selectedWorkId) ->
            model |> withUpdatingWorkModel workModel selectedWorkId |> withCmdNone |> withNoIntent

    | Msg.CreatingWorkModelMsg smsg, CreatingWork sm ->
        let (sm, cmd, intent) = updateCreatingWorkModel smsg sm

        match intent with
        | CreatingWorkModel.Intent.None ->
            model |> withCreatingWorkModel sm
            , Cmd.map Msg.CreatingWorkModelMsg cmd
            , Intent.None

        | CreatingWorkModel.Intent.Cancel ->
            let (m, cmd) = WorkListModel.init None
            model |> withWorkListModel m
            , Cmd.map Msg.WorkListModelMsg cmd
            , Intent.None

        | CreatingWorkModel.Intent.SwitchToWorkList newWorkId ->
            let (m, cmd) = WorkListModel.init None
            model |> withWorkListModel m
            , Cmd.batch [
                Cmd.map Msg.WorkListModelMsg cmd
                Cmd.ofMsg (WorkListModel.Msg.SetSelectedWorkId (newWorkId |> Some) |> Msg.WorkListModelMsg)
            ]
            , Intent.None

    | Msg.UpdatingWorkModelMsg smsg, UpdatingWork (workModel, selectedWorkId) ->
        let (workModel, cmd, intent) = updateWorkModel smsg workModel

        match intent with
        | WorkModel.Intent.EndEdit ->
            let (m, cmd) = WorkListModel.init selectedWorkId
            model |> withWorkListModel m
            , Cmd.map Msg.WorkListModelMsg cmd
            , Intent.SelectCurrentWork (workModel |> Some)

        | WorkModel.Intent.StartEdit
        | WorkModel.Intent.Select  // TODO: add ability to select work from edit mode
        | WorkModel.Intent.None ->
            model |> withUpdatingWorkModel workModel selectedWorkId
            , Cmd.map Msg.UpdatingWorkModelMsg cmd
            , Intent.None

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent
