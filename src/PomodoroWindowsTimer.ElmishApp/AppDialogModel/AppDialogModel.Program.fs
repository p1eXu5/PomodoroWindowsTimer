module PomodoroWindowsTimer.ElmishApp.AppDialogModel.Program

open Elmish
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.AppDialogModel


let update
    initBotSettingsModel
    updateBotSettingsModel
    initTimePointsGeneratorModel
    updateTimePointsGeneratorModel
    initWorkStatisticListModel
    updateWorkStatisticListModel
    (dialogErrorMessageQueue: IErrorMessageQueue)
    (msg: AppDialogModel.Msg)
    (model: AppDialogModel)
    =
    match msg with
    | Msg.LoadBotSettingsDialogModel ->
        initBotSettingsModel () |> AppDialogModel.BotSettingsDialog
        , Cmd.none

    | MsgWith.BotSettingsModelMsg model (bmsg, bm) ->
        let (m, intent) = updateBotSettingsModel bmsg bm
        match intent with
        | BotSettingsModel.Intent.None ->
            m |> AppDialogModel.BotSettingsDialog, Cmd.none
        | BotSettingsModel.Intent.CloseDialogRequested ->
            AppDialogModel.NoDialog, Cmd.none

    | Msg.LoadTimePointsGeneratorDialogModel ->
        let (m, cmd) = initTimePointsGeneratorModel ()
        m |> AppDialogModel.TimePointsGeneratorDialog
        , Cmd.map Msg.TimePointsGeneratorModelMsg cmd

    | MsgWith.TimePointsGeneratorModelMsg model (gmsg, gm) ->
        let (m, cmd, _) = updateTimePointsGeneratorModel gmsg gm
        m |> AppDialogModel.TimePointsGeneratorDialog
        , Cmd.map Msg.TimePointsGeneratorModelMsg cmd

    | Msg.LoadWorkStatisticsDialogModel ->
        let (m, cmd) = initWorkStatisticListModel ()
        m |> AppDialogModel.WorkStatisticsDialog
        , Cmd.map Msg.WorkStatisticListModelMsg cmd

    | MsgWith.WorkStatisticListModelMsg model (gmsg, gm) ->
        let (m, cmd, intent) = updateWorkStatisticListModel gmsg gm
        match intent with
        | WorkStatisticListModel.Intent.None ->
             m |> AppDialogModel.WorkStatisticsDialog
            , Cmd.map Msg.WorkStatisticListModelMsg cmd
        | WorkStatisticListModel.Intent.CloseDialogRequested ->
            AppDialogModel.NoDialog, Cmd.none

    | Msg.EnqueueExn e ->
        dialogErrorMessageQueue.EnqueueError (sprintf "%A" e)
        model, Cmd.none

    | _ ->
        model, Cmd.none


