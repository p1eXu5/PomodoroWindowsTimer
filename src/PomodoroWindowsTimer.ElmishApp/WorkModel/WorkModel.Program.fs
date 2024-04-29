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
    | Msg.SetNumber n -> model |> withNumber n |> withCmdNone
    | Msg.SetTitle t -> model |> withTitle t |> withCmdNone
    
    | MsgWith.``Start of Update`` model (deff, cts) ->
        let updateWork =
            { model.Work with
                Number = model.Number
                Title = model.Title
            }

        model |> withUpdateState deff
        , Cmd.OfTask.perform (workRepo.Update updateWork) cts.Token (AsyncOperation.finishWithin Msg.Update cts)

    | MsgWith.``Finish of Update`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            model |> withUpdateState AsyncDeferred.NotRequested |> withCmdNone
        | Ok (_, updatedAt) ->
            model |> withUpdatedWork updatedAt |> withUpdateState (AsyncDeferred.NotRequested) |> withCmdNone

    | MsgWith.``Start of CreateNew`` model (deff, cts) ->
        model |> withCreateNewState deff
        , Cmd.OfTask.perform (workRepo.Create model.Number model.Title) cts.Token (AsyncOperation.finishWithin Msg.CreateNew cts)

    | MsgWith.``Finish of CreateNew`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            model |> withCreateNewState AsyncDeferred.NotRequested |> withCmdNone
        | Ok (_, (id, createdAt)) ->
            model |> withCreatedWork id createdAt |> withCreateNewState (AsyncDeferred.NotRequested) |> withCmdNone

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none
