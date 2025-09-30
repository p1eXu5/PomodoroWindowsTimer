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
    | Msg.SetNumber n -> model |> withNumber n, Cmd.none, Intent.None
    | Msg.SetTitle t -> model |> withTitle t, Cmd.none, Intent.None

    | MsgWith.``Start of CreateWork`` model (deff, cts) ->
        model |> withCreatingState deff
        , Cmd.OfTask.perform (createWorkTask model.Number model.Title) cts.Token (AsyncOperation.finishWithin Msg.CreateWork cts)
        , Intent.None

    | MsgWith.``Finish of CreateWork`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            model |> withCreatingState AsyncDeferred.NotRequested, Cmd.none, Intent.None
        | Ok (deff, id) ->
            model |> withCreatingState deff, Cmd.none, Intent.SwitchToWorkList id

    | Msg.Cancel ->
        model, Cmd.none, (if model.CanBeCancelling then Intent.Cancel else Intent.Close)

    | _ ->
        logger.LogNonProcessedMessage(msg, model)
        model, Cmd.none, Intent.Cancel
