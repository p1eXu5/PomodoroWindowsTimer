module PomodoroWindowsTimer.ElmishApp.WorkListModel.Program

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkListModel
open PomodoroWindowsTimer.Abstractions

let update (workRepo: IWorkRepository) (logger: ILogger<WorkListModel>) (errorMessageQueue: IErrorMessageQueue) updateWorkModel msg model =
    let loadWorksTask ct =
        task {
            let! works = workRepo.ReadAll ct
            return
                works
                |> Result.map (Seq.map WorkModel.init >> Seq.toList)
        }

    match msg with
    | Msg.SetSelectedWorkId id -> model |> withSelectedWorkId id |> withCmdNone |> withNoIntent

    | MsgWith.``Start of LoadWorkList`` model (deff, cts) ->
        model |> withWorks deff
        , Cmd.OfTask.perform loadWorksTask cts.Token (AsyncOperation.finishWithin Msg.LoadWorkList cts)
        , Intent.None

    | MsgWith.``Finish of LoadWorkList`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            model |> withWorks (AsyncDeferred.NotRequested) |> withCmdNone |> withNoIntent
        | Ok (deff, workModels) ->
            match workModels with
            | [] ->
                model |> withWorks deff |> withSelectedWorkId None |> withCmdNone |> withSwitchToCreateWorkIntent
            | head :: _ ->
                model |> withWorks deff |> withSelectedWorkId (head.Work.Id |> Some) |> withCmdNone |> withNoIntent

    | Msg.WorkModelMsg (id, wmsg) ->
        match model.Works with
        | AsyncDeferred.Retrieved works ->
            let (wmodelList, wcmd) =
                works |> mapFirstCmd (_.Work >> _.Id >> (=) id) (updateWorkModel wmsg)

            model |> withWorks (wmodelList |> AsyncDeferred.Retrieved)
            , Cmd.map (fun wmsg -> Msg.WorkModelMsg (id, wmsg)) wcmd
            , Intent.None

        | _ -> model |> withCmdNone |> withNoIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent

