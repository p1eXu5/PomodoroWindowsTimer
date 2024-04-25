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
    (dialogErrorMessageQueue: IErrorMessageQueue)
    (msg: AppDialogModel.Msg)
    (model: AppDialogModel)
    =
    match msg with
    | Msg.LoadBotSettingsDialogModel ->
        initBotSettingsModel () |> AppDialogModel.BotSettingsDialog
        , Cmd.none

    | MsgWith.BotSettingsModelMsg model (bmsg, bm) ->
        updateBotSettingsModel bmsg bm |> AppDialogModel.BotSettingsDialog
        , Cmd.none

    | Msg.LoadTimePointsGeneratorDialogModel ->
        let (m, cmd) = initTimePointsGeneratorModel ()
        m |> AppDialogModel.TimePointsGeneratorDialog
        , Cmd.map Msg.TimePointsGeneratorModelMsg cmd

    | MsgWith.TimePointsGeneratorModelMsg model (gmsg, gm) ->
        let (m, cmd) = updateTimePointsGeneratorModel gmsg gm
        m |> AppDialogModel.TimePointsGeneratorDialog
        , Cmd.map Msg.TimePointsGeneratorModelMsg cmd

    | Msg.EnqueueExn e ->
        dialogErrorMessageQueue.EnqueueError (sprintf "%A" e)
        model, Cmd.none

    | _ ->
        model, Cmd.none


