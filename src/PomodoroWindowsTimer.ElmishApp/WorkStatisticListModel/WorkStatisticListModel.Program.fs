module PomodoroWindowsTimer.ElmishApp.WorkStatisticListModel.Program

open System
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkStatisticListModel
open PomodoroWindowsTimer.ElmishApp.Abstractions

let update (userSettings: IUserSettings) (workEventRepo: IWorkEventRepository) (errorMessageQueue: IErrorMessageQueue) (logger: ILogger<WorkStatisticListModel>) msg (model: WorkStatisticListModel) =
    match msg with
    | Msg.SetStartDate startDate when model.StartDate <> startDate ->
        model |> withStartDate userSettings startDate
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadStatistics)
        , Intent.None

    | Msg.SetEndDate endDate when model.EndDate <> endDate ->
        model |> withEndDate userSettings endDate
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadStatistics)
        , Intent.None

    | Msg.SetIsByDay v ->
        let model' = model |> withIsByDay userSettings v
        if model'.StartDate <> model.StartDate || model'.EndDate <> model.EndDate then
            model'
            , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadStatistics)
            , Intent.None
        else
            model' |> withCmdNone |> withNoIntent

    | MsgWith.``Start of LoadStatistics`` model (deff, cts) ->
        let period =
            {
                Start = model.StartDate
                EndInclusive = model.EndDate
            }
            : DateOnlyPeriod

        model |> withStatistics deff
        , Cmd.OfTask.perform (WorkEventProjector.projectAllByPeriod workEventRepo period) cts.Token (AsyncOperation.finishWithin Msg.LoadStatistics cts)
        , Intent.None

    | MsgWith.``Finish of LoadStatistics`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withStatistics AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent
        | Ok (deff, _) ->
            model |> withStatistics deff |> withCmdNone |> withNoIntent

    | Msg.Close ->
        match model.WorkStatistics with
        | AsyncDeferred.InProgress cts ->
            cts.Cancel()
        | _ -> ()
        model |> withCmdNone |> withCloseIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent

