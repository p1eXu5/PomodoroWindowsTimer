module PomodoroWindowsTimer.ElmishApp.WorkModel.Program

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkModel


let update (workRepo: IWorkRepository) (logger: ILogger<WorkModel>) (errorMessageQueue: IErrorMessageQueue) msg model =
    match msg with
    | Msg.SetNumber n -> model |> withNumber n |> withCmdNone |> withNoIntent
    | Msg.SetTitle t -> model |> withTitle t |> withCmdNone |> withNoIntent

    | Msg.Select -> model |> withCmdNone |> withSelectIntent
    | Msg.Edit -> model |> withCmdNone |> withStartEditIntent
    | Msg.CancelEdit -> model |> withCmdNone |> withEndEditIntent
    
    | MsgWith.``Start of Update`` model (deff, cts) ->
        let updateWork =
            { model.Work with
                Number = model.Number
                Title = model.Title
            }

        (
            model |> withUpdateState deff
            , Cmd.OfTask.perform (workRepo.UpdateAsync updateWork) cts.Token (AsyncOperation.finishWithin Msg.Update cts)
        )
        |> withNoIntent

    | MsgWith.``Finish of Update`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            model |> withUpdateState AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent
        | Ok (_, updatedAt) ->
            model |> withUpdatedWork updatedAt |> withUpdateState (AsyncDeferred.NotRequested) |> withCmdNone |> withEndEditIntent

    | MsgWith.``Start of CreateNew`` model (deff, cts) ->
        (
            model |> withCreateNewState deff
            , Cmd.OfTask.perform (workRepo.CreateAsync model.Number model.Title) cts.Token (AsyncOperation.finishWithin Msg.CreateNew cts)
        )
        |> withNoIntent

    | MsgWith.``Finish of CreateNew`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            model |> withCreateNewState AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent
        | Ok (_, (id, createdAt)) ->
            model |> withCreatedWork id createdAt |> withCreateNewState (AsyncDeferred.NotRequested) |> withCmdNone |> withNoIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent
