module PomodoroWindowsTimer.ElmishApp.CreatingWorkModel.Program

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.CreatingWorkModel

let update (workEventStore: WorkEventStore) (errorMessageQueue: IErrorMessageQueue) (logger: ILogger<CreatingWorkModel>) msg model =
    let createWorkTask number title ct =
        task {
            let workRepo = workEventStore.GetWorkRepository ()
            let! res = workRepo.InsertAsync number title ct
            return
                res |> Result.map fst
        }

    match msg with
    | Msg.SetNumber n -> model |> withNumber n |> withCmdNone |> withNoIntent
    | Msg.SetTitle t -> model |> withTitle t |> withCmdNone |> withNoIntent

    | MsgWith.``Start of CreateWork`` model (deff, cts) ->
        model |> withCreatingState deff
        , Cmd.OfTask.perform (createWorkTask model.Number model.Title) cts.Token (AsyncOperation.finishWithin Msg.CreateWork cts)
        , Intent.None

    | MsgWith.``Finish of CreateWork`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            model |> withCreatingState AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent
        | Ok (deff, id) ->
            model |> withCreatingState deff |> withCmdNone |> withSwitchToWorkListIntent id

    | Msg.Cancel ->
        model |> withCmdNone |> withCancelIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent
