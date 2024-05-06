module PomodoroWindowsTimer.ElmishApp.AppDialogModel.Program

open Elmish
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.AppDialogModel


let update
    (botConfiguration: IBotSettings)
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
    | Msg.SetIsDialogOpened v ->
        model |> withIsOpened v |> withCmdNone

    | Msg.LoadBotSettingsDialogModel ->
        model
        |> withBotSettingsDialogModel (initBotSettingsModel ())
        |> withCmdNone

    | MsgWith.BotSettingsModelMsg model (bmsg, bm) ->
        let (m, intent) = updateBotSettingsModel bmsg bm
        match intent with
        | BotSettingsModel.Intent.None ->
            model |> withBotSettingsDialogModel m |> withCmdNone
        | BotSettingsModel.Intent.CloseDialogRequested ->
            model |> withNoDialog |> withCmdNone

    | MsgWith.ApplyBotSettings model bm ->
        botConfiguration.MyChatId <- bm.ChatId
        botConfiguration.BotToken <- bm.BotToken
        model |> withNoDialog |> withCmdNone

    | Msg.LoadTimePointsGeneratorDialogModel ->
        let (m, cmd) = initTimePointsGeneratorModel ()
        model |> withTimePointsGeneratorDialogModel m
        , Cmd.map Msg.TimePointsGeneratorModelMsg cmd

    | MsgWith.TimePointsGeneratorModelMsg model (gmsg, gm) ->
        let (m, cmd, _) = updateTimePointsGeneratorModel gmsg gm
        model |> withTimePointsGeneratorDialogModel m
        , Cmd.map Msg.TimePointsGeneratorModelMsg cmd

    | Msg.LoadWorkStatisticsDialogModel ->
        let (m, cmd) = initWorkStatisticListModel ()
        model |> withWorkStatisticsDialogModel m
        , Cmd.map Msg.WorkStatisticListModelMsg cmd

    | MsgWith.WorkStatisticListModelMsg model (gmsg, gm) ->
        let (m, cmd, intent) = updateWorkStatisticListModel gmsg gm
        match intent with
        | WorkStatisticListModel.Intent.None ->
            model |> withWorkStatisticsDialogModel m
            , Cmd.map Msg.WorkStatisticListModelMsg cmd
        | WorkStatisticListModel.Intent.CloseDialogRequested ->
            model |> withNoDialog |> withCmdNone

    | Msg.Unload ->
        model |> withNoDialog |> withCmdNone

    | Msg.EnqueueExn e ->
        dialogErrorMessageQueue.EnqueueError (sprintf "%A" e)
        model, Cmd.none

    | _ ->
        model, Cmd.none


