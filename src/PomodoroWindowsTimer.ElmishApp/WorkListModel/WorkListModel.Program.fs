﻿module PomodoroWindowsTimer.ElmishApp.WorkListModel.Program

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkListModel
open PomodoroWindowsTimer.ElmishApp

let update (userSettings: IUserSettings) (workEventStore: WorkEventStore) (logger: ILogger<WorkListModel>) (errorMessageQueue: IErrorMessageQueue) updateWorkModel msg model =
    let loadWorksTask ct =
        task {
            let workRepo = workEventStore.GetWorkRepository ()
            let! works = workRepo.ReadAllAsync ct
            return
                works
                |> Result.map (List.map WorkModel.init)
        }

    match msg with
    | Msg.SetSelectedWorkId id ->
        match id, model.SelectedWorkId with
        | Some newId, Some oldId when newId <> oldId ->
            model |> withSelectedWorkId id |> withCmdNone |> withSelectIntent
        | None, Some _
        | Some _, None ->
            model |> withSelectedWorkId id |> withCmdNone |> withSelectIntent
        | _ -> model |> withCmdNone |> withNoIntent


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
                model |> withWorks deff
                , Cmd.batch [
                    Cmd.ofMsg (Msg.SetSelectedWorkId None)
                    Cmd.ofMsg (Msg.SetLastDayCount "")
                ]
                , Intent.SwitchToCreateWork
            | _ ->
                match model.SelectedWorkId with
                | Some selId ->
                    if workModels |> List.exists (_.Work >> _.Id >> (=) selId) then
                        (
                            model |> withWorks deff
                            , Cmd.ofMsg Msg.LoadLastDayCount
                        )
                        |> withSelectIntent
                    else
                        model |> withWorks deff
                        , Cmd.batch [
                            Cmd.ofMsg (Msg.SetSelectedWorkId None)
                            Cmd.ofMsg Msg.LoadLastDayCount
                        ]
                        , Intent.Unselect
                | None ->
                    model |> withWorks deff |> withCmdNone |> withNoIntent

    | Msg.WorkModelMsg (id, wmsg) ->
        match model.Works with
        | AsyncDeferred.Retrieved works ->
            let (wmodelList, wcmd, intent) =
                works |> List.mapFirstCmdIntent (_.Work >> _.Id >> (=) id) (updateWorkModel wmsg) WorkModel.Intent.None

            match intent with
            | WorkModel.Intent.Select ->
                model |> withWorks (wmodelList |> AsyncDeferred.Retrieved)
                , Cmd.batch [
                    Cmd.ofMsg (Msg.SetSelectedWorkId (id |> Some))
                    Cmd.map (fun wmsg -> Msg.WorkModelMsg (id, wmsg)) wcmd
                ]
                , Intent.None
         
            | WorkModel.Intent.StartEdit ->
                model |> withWorks (wmodelList |> AsyncDeferred.Retrieved)
                , Cmd.map (fun wmsg -> Msg.WorkModelMsg (id, wmsg)) wcmd
                , Intent.Edit (wmodelList |> List.find (_.Work >> _.Id >> (=) id), model.SelectedWorkId)

            | WorkModel.Intent.EndEdit
            | WorkModel.Intent.None ->
                model |> withWorks (wmodelList |> AsyncDeferred.Retrieved)
                , Cmd.map (fun wmsg -> Msg.WorkModelMsg (id, wmsg)) wcmd
                , Intent.None

        | _ -> model |> withCmdNone |> withNoIntent

    | Msg.CreateWork ->
        model |> withCmdNone |> withSwitchToCreateWorkIntent

    | Msg.UnselectWork ->
        model |> withSelectedWorkId None |> withCmdNone |> withSelectIntent

    | Msg.SetLastDayCount v ->
        let model = model |> withParsingLastDayCount v

        model
        , Cmd.OfFunc.attempt (fun value -> userSettings.LastDayCount <- value) model.LastDayCount Msg.OnExn
        , Intent.None

    | Msg.LoadLastDayCount ->
        model |> withLastDayCount userSettings.LastDayCount |> withCmdNone |> withNoIntent

    | Msg.OnExn ex ->
        logger.LogError(ex, "Exception has been thrown in WorkListModel Program.")
        errorMessageQueue.EnqueueError ex.Message
        model |> withCmdNone |> withNoIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent

