module PomodoroWindowsTimer.ElmishApp.WorkSelectorModel.Program

open Microsoft.Extensions.Logging

open Elmish

open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkSelectorModel

let update updateWorkListModel updateWorkModel (logger: ILogger<WorkSelectorModel>) msg model =
    match msg, model with
    | Msg.WorkListModelMsg smsg, WorkList sm ->
        let (sm, cmd, intent) = updateWorkListModel smsg sm


        model |> withWorkListModel sm
        , Cmd.map Msg.WorkListModelMsg cmd

    | Msg.CreatingWorkModelMsg smsg, CreatingWork sm ->
        let (sm, cmd) = updateWorkModel smsg sm
        model |> withCreatingWorkModel sm
        , Cmd.map Msg.CreatingWorkModelMsg cmd

    | Msg.UpdatingWorkModelMsg smsg, UpdatingWork sm ->
        let (sm, cmd) = updateWorkModel smsg sm
        model |> withUpdatingWorkModel sm
        , Cmd.map Msg.UpdatingWorkModelMsg cmd

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none
