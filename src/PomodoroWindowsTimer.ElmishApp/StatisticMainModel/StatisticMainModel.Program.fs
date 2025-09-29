module PomodoroWindowsTimer.ElmishApp.StatisticMainModel.Program

open Microsoft.Extensions.Logging

open Elmish

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.StatisticMainModel

let update
    dailyStatisticListModelUpdate
    workListModelUpdate
    (logger: ILogger<StatisticMainModel>)
    msg model
    =
    match msg with
    | MsgWith.DailyStatisticListModelMsg model (smsg, sm) ->
        let (dailyStatisticListModel, dailyStatisticListModelCmd, dailyStatisticListIntent) = dailyStatisticListModelUpdate smsg sm

        model |> withDailyStatisticListModel dailyStatisticListModel
        , Cmd.map (Msg.DailyStatisticListModelMsg) dailyStatisticListModelCmd
        , Intent.fromDailyStatisticListModelIntent dailyStatisticListIntent

    | MsgWith.WorkListModelMsg model (smsg, sm) ->
        let (workListModel, workListModelCmd, workListIntent) = workListModelUpdate smsg sm

        model |> withWorkListModel workListModel
        , Cmd.map (Msg.WorkListModelMsg) workListModelCmd
        , Intent.fromWorkListModelIntent workListIntent

    | MsgWith.SelectWorkListTab model () ->
        let (workListModel, workListModelCmd) = WorkListModel.init None

        model |> withWorkListModel workListModel
        , Cmd.map (Msg.WorkListModelMsg) workListModelCmd
        , Intent.None

    | Msg.CloseWindow ->
        let (model, cmd) =
            match model.DailyStatisticListModel with
            | Some sm ->
                let (sm, cmd, _) = dailyStatisticListModelUpdate DailyStatisticListModel.Msg.Close sm
                model |> withDailyStatisticListModel sm
                , Cmd.map (Msg.DailyStatisticListModelMsg) cmd
            | _ ->
                model, Cmd.none

        let (model, cmd) =
            match model.WorkListModel with
            | Some sm ->
                let (sm, scmd, _) = workListModelUpdate WorkListModel.Msg.Close sm
                model |> withWorkListModel sm
                , Cmd.batch [
                    cmd
                    Cmd.map (Msg.WorkListModelMsg) scmd
                ]
            | None -> model, cmd

        model, cmd, Intent.CloseWindow

    | _ ->
        logger.LogNonProcessedMessage(msg, model)
        model, Cmd.none, Intent.None

