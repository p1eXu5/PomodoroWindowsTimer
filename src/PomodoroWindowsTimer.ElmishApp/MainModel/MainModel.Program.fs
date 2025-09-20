﻿module PomodoroWindowsTimer.ElmishApp.MainModel.Program

open System.Threading
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.Types

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.ElmishApp.Logging

open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.Abstractions


/// Msg.SetIsTimePointsShown handler.
let private setIsTimePointsDrawerShown initRunningTimePoints (v: bool) (model: MainModel) =
    if v then model |> withoutWorkSelectorModel else model
    |> withIsTimePointsDrawerShown initRunningTimePoints v
    , Cmd.none


let private mapTimePointsDrawerMsg updateTimePointsDrawerModel smsg (model: MainModel) =
    let (drawerModel', drawerCmd) = model.TimePointsDrawer |> updateTimePointsDrawerModel smsg
    model |> withTimePointsDrawer drawerModel'
    , Cmd.map Msg.TimePointsDrawerMsg drawerCmd

/// SetIsWorkSelectorLoaded handler
let private setIsWorkSelectorLoaded initRunningTimePoints initWorkSelectorModel (v: bool) (model: MainModel) =
    if v then
        let (m, cmd) = initWorkSelectorModel (model.CurrentWork.Id)
        model |> withWorkSelectorModel m |> withIsTimePointsDrawerShown initRunningTimePoints false
        , Cmd.map Msg.WorkSelectorModelMsg cmd
    else
        model |> withoutWorkSelectorModel |> withCmdNone


/// Maps PlayerModel.Intent to MainModel.Msg
let private playerIntentCmd playerIntent (model: MainModel) =
    match playerIntent with
    | PlayerModel.Intent.None -> Cmd.none

    | PlayerModel.Intent.RollbackTime (workSpentTime, kind, atpId, time) ->
        Cmd.ofMsg (
            Msg.AppDialogModelMsg (
                AppDialogModel.Msg.LoadRollbackWorkDialogModel (workSpentTime, kind, atpId, time)
            )
        )

    | PlayerModel.Intent.MultipleRollbackTime (workSpentTime, kind, atpId, time) ->
        Cmd.ofMsg (
            Msg.AppDialogModelMsg (
                AppDialogModel.Msg.LoadRollbackWorkListDialogModel (workSpentTime, kind, atpId, time)
            )
        )

    | PlayerModel.Intent.SkipOrApplyMissingTime (kind, atpId, diff, time) ->
        model.CurrentWork.Work
        |> Option.map (fun work ->
            Cmd.ofMsg (
                Msg.AppDialogModelMsg (
                    AppDialogModel.Msg.LoadSkipOrApplyMissingTimeDialogModel (work, kind, atpId, diff, time)
                )
            )
        )
        |> Option.defaultValue Cmd.none


/// Msg.PlayerModelMsg handler.
let private mapPlayerModelMsg updatePlayerModel pmsg (model: MainModel) =
    let (playerModel, playerCmd, playerIntent) = updatePlayerModel pmsg model.Player

    let cmd = Cmd.map Msg.PlayerModelMsg playerCmd
    let intentCmd = playerIntentCmd playerIntent model

    model |> withPlayerModel playerModel
    , Cmd.batch [ cmd; intentCmd ]


/// Msg.LooperMsg handler.
let private mapLooperMsg updatePlayerModel updateCurrentWorkModel updateTimePointsDrawerModel lmsg (model: MainModel) =
    let (playerModel, playerCmd, playerIntent) =
        updatePlayerModel (lmsg |> PlayerModel.Msg.LooperMsg) model.Player

    let (currentWorkModel, currentWorkCmd) =
        updateCurrentWorkModel (lmsg |> CurrentWorkModel.Msg.LooperMsg) model.CurrentWork
       
    let (timePointsDrawerOpt, timePointsDrawerCmd) =
        model.TimePointsDrawer
            |> updateTimePointsDrawerModel (lmsg |> TimePointsDrawerModel.Msg.LooperMsg) // TODO: change to RunningTimePointListModel.Msg

    model
    |> withPlayerModel playerModel
    |> withCurrentWorkModel currentWorkModel
    |> withTimePointsDrawer timePointsDrawerOpt
    , Cmd.batch [
        Cmd.map Msg.PlayerModelMsg playerCmd
        playerIntentCmd playerIntent model
        Cmd.map Msg.CurrentWorkModelMsg currentWorkCmd
        Cmd.map Msg.TimePointsDrawerMsg timePointsDrawerCmd
    ]


/// Maps WorkSelectorModel.Intent to MainModel.Msg.
let private workSelectorIntentCmd intent =
    match intent with
    | WorkSelectorModel.Intent.SelectCurrentWork workModel ->
        Cmd.ofMsg (
            Msg.CurrentWorkModelMsg (
                CurrentWorkModel.Msg.SetWork workModel.Work
            )
        )
    | WorkSelectorModel.Intent.UnselectCurrentWork ->
        Cmd.ofMsg (
            Msg.CurrentWorkModelMsg (
                CurrentWorkModel.Msg.UnsetWork
            )
        )
    | _ ->
        Cmd.none


/// MsgWith.WorkSelectorModelMsg handler
let private mapWorkSelectorModelMsg updateWorkSelectorModel smsg m (model: MainModel) =
    let (workSelectorModel, workSelectorCmd, intent) = updateWorkSelectorModel smsg m

    let cmd =  Cmd.map Msg.WorkSelectorModelMsg workSelectorCmd
    let intentCmd = workSelectorIntentCmd intent

    intent
    |> function
        | WorkSelectorModel.Intent.Close -> model |> withoutWorkSelectorModel
        | _ -> model
    |> withWorkSelectorModel workSelectorModel
    , Cmd.batch [ cmd; intentCmd ]


/// Msg.SetIsWorkStatisticShown handler.
let private setIsWorkStatisticShown initStatisticMainModel v (model: MainModel) =
    if v then
        let (statisticMainModel, statisticCmd) = initStatisticMainModel ()
        model |> MainModel.withDailyStatisticList (statisticMainModel |> Some)
        , Cmd.map Msg.StatisticMainModelMsg statisticCmd
    else
        model |> MainModel.withDailyStatisticList None
        , Cmd.none


/// MsgWith.StatisticMainModelMsg handler.
let private mapStatisticMainModelMsg updateStatisticMainModel (sm: StatisticMainModel) (model: MainModel) =
    let (statisticMainModel, statisticCmd, statisticIntent) = updateStatisticMainModel sm

    match statisticIntent with
    | StatisticMainModel.Intent.None ->
        model |> MainModel.withDailyStatisticList (statisticMainModel |> Some)
        , Cmd.map Msg.StatisticMainModelMsg statisticCmd

    | StatisticMainModel.Intent.CloseWindow ->
        model |> MainModel.withDailyStatisticList None
        , Cmd.none


/// Msg.PlayerUserSettingsChanged handler
let private mapPlayerUserSettingsChangedMsg updatePlayerModel updateTimePointsDrawerModel (model: MainModel) =
    model
    |> mapPlayerModelMsg updatePlayerModel PlayerModel.Msg.PlayerUserSettingsChanged
    |> mapmc (mapTimePointsDrawerMsg updateTimePointsDrawerModel (
        TimePointsDrawerModel.Msg.RunningTimePointsMsg RunningTimePointListModel.Msg.PlayerUserSettingsChanged))

let private mapTimePointQueueMsg updateTimePointsDrawerModel (timePointsAndId: TimePoint list * TimePointId option) (model: MainModel) =
    model
    |> mapc _.TimePointsDrawer withTimePointsDrawer Msg.TimePointsDrawerMsg (
        updateTimePointsDrawerModel (TimePointsDrawerModel.Msg.RunningTimePointsMsg (
            RunningTimePointListModel.Msg.TimePointQueueMsg timePointsAndId)))

    (*
    
    let (workSelectorModel, workSelectorCmd, intent) = updateWorkSelectorModel smsg m
        let cmd =  Cmd.map Msg.WorkSelectorModelMsg workSelectorCmd

        match intent with
        | WorkSelectorModel.Intent.None ->
            model |> withWorkSelectorModel workSelectorModel |> withCmd cmd

        | WorkSelectorModel.Intent.SelectCurrentWork workModel ->
            cfg.CurrentWorkItemSettings.CurrentWork <- workModel.Work |> Some
            
            let (currWorkModel', currWorkCmd) =
                match model.Player.LooperState, model.CurrentWork with
                | LooperState.Playing, Some currWorkModel ->
                    currWorkModel
                    |> updateCurrentWorkModel
                        (CurrentWorkModel.Msg.SetPlayingWork (workModel.Work, model.Player.ActiveTimePoint.Value)) 

                | LooperState.Playing, None ->
                    initPlayingCurrentWorkModel workModel.Work

                | _, Some currWorkModel ->
                    currWorkModel
                    |> updateCurrentWorkModel
                        (CurrentWorkModel.Msg.SetWork workModel.Work)
                        
                | _, None ->
                    CurrentWorkModel.init workModel.Work

            model
            |> withWorkSelectorModel workSelectorModel
            |> withCurrentWorkModel currWorkModel'
            , Cmd.batch [
                cmd
                Cmd.map Msg.CurrentWorkModelMsg currWorkCmd
            ]

            (*
            let currentWorkModel = workModel.Work |> CurrentWorkModel.init

            if model.Player.LooperState = LooperState.Playing then
                let time = cfg.TimeProvider.GetUtcNow()
                match model.CurrentWork with
                | Some currWork when currWork.Id <> workModel.Id ->
                    model
                    |> withWorkSelectorModel workSelectorModel
                    |> withCurrentWorkModel currentWorkModel
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (currWork.Id, time.AddMilliseconds(-1), model.Player.ActiveTimePoint.Value) Msg.OnExn
                        Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (workModel.Id, time, model.Player.ActiveTimePoint.Value) Msg.OnExn
                    ]
                | Some _ ->
                    model
                    |> withWorkSelectorModel workSelectorModel
                    |> withCurrentWorkModel currentWorkModel
                    , cmd
                | None ->
                    model
                    |> withWorkSelectorModel workSelectorModel
                    |> withCurrentWorkModel currentWorkModel
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (workModel.Id, time, model.Player.ActiveTimePoint.Value) Msg.OnExn
                    ]
            else
                model
                |> withWorkSelectorModel workSelectorModel
                |> withCurrentWorkModel currentWorkModel
                , cmd
            *)  

        | WorkSelectorModel.Intent.UnselectCurrentWork ->
            cfg.CurrentWorkItemSettings.CurrentWork <- None

            if model.Player.LooperState = LooperState.Playing then
                match model.CurrentWork with
                | Some currWork ->
                    let time = cfg.TimeProvider.GetUtcNow()
                    model
                    |> withWorkSelectorModel workSelectorModel
                    |> withoutCurrentWorkModel
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (currWork.Id, time, model.Player.ActiveTimePoint.Value) Msg.OnExn
                    ]
                | None ->
                    model |> withWorkSelectorModel workSelectorModel |> withoutCurrentWorkModel
                    , cmd
            else
                model |> withWorkSelectorModel workSelectorModel |> withoutCurrentWorkModel
                , cmd

        | WorkSelectorModel.Intent.Close ->
            model |> withoutWorkSelectorModel |> withCmdNone

    *)

/// MainModel.Program update function.
let update
    (telegramBot: ITelegramBot)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<MainModel>)
    updatePlayerModel
    updateCurrentWorkModel
    initRunningTimePoints
    updateTimePointsDrawerModel
    updateAppDialogModel
    initWorkSelectorModel
    updateWorkSelectorModel
    initStatisticMainModel
    updateStatisticMainModel
    (msg: Msg)
    (model: MainModel)
    =
    match msg with
    // --------------------
    // Time Points
    // --------------------
    | Msg.SetIsTimePointsDrawerShown v ->
        model |> setIsTimePointsDrawerShown initRunningTimePoints v

    | Msg.TimePointsDrawerMsg smsg ->
        model |> mapTimePointsDrawerMsg updateTimePointsDrawerModel smsg

    | Msg.TimePointQueueMsg (timePoints, timePointIdOpt) ->
        model |> mapTimePointQueueMsg updateTimePointsDrawerModel (timePoints, timePointIdOpt)

    | Msg.StartTimePoint tpId ->
        model, Cmd.ofMsg (tpId |> Operation.Start |> PlayerModel.Msg.StartTimePoint |> Msg.PlayerModelMsg)

    // --------------------
    // Player
    // --------------------
    | Msg.PlayStopCommand _ ->
        model, Cmd.ofMsg (model.Player |> PlayerModel.Msg.playStopResume |> Msg.PlayerModelMsg)

    | Msg.LooperMsg lmsg ->
        model |> mapLooperMsg updatePlayerModel updateCurrentWorkModel updateTimePointsDrawerModel lmsg

    | Msg.PlayerModelMsg pmsg ->
        model |> mapPlayerModelMsg updatePlayerModel pmsg

    | Msg.PlayerUserSettingsChanged ->
        model |> mapPlayerUserSettingsChangedMsg updatePlayerModel updateTimePointsDrawerModel

    // --------------------
    // Work
    // --------------------
    | Msg.CurrentWorkModelMsg currWorkMsg ->
        model |> mapc _.CurrentWork withCurrentWorkModel Msg.CurrentWorkModelMsg (updateCurrentWorkModel currWorkMsg)

    | Msg.SetIsWorkSelectorLoaded v ->
        model |> setIsWorkSelectorLoaded initRunningTimePoints initWorkSelectorModel v

    | MsgWith.WorkSelectorModelMsg model (smsg, m) ->
        model |> mapWorkSelectorModelMsg updateWorkSelectorModel smsg m

    // --------------------
    // Dialog Windows
    // --------------------
    | Msg.AppDialogModelMsg smsg ->
        model |> mapc _.AppDialog withAppDialogModel Msg.AppDialogModelMsg (updateAppDialogModel smsg)

    | Msg.SetIsWorkStatisticShown v ->
        model |> setIsWorkStatisticShown initStatisticMainModel v

    | MsgWith.StatisticMainModelMsg model (smsg, sm) ->
        model |> mapStatisticMainModelMsg (updateStatisticMainModel smsg) sm

    // --------------------
    
    // for test purpose:
    | Msg.SendToChatBot message ->
        model, Cmd.OfTask.attempt telegramBot.SendMessage message Msg.OnExn

    // --------------------
    | Msg.OnError err ->
        logger.LogProgramError err
        errorMessageQueue.EnqueueError err
        model, Cmd.none

    | Msg.OnExn ex ->
        logger.LogProgramExn ex
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none

    // TODO: remove
    // old:
    // 
    // | Msg.LoadTimePointsFromSettings ->
    //    let timePoints = cfg.TimePointStore.Read()
    //    cfg.TimePointQueue.Reload(timePoints)
    //    cfg.Looper.PreloadTimePoint()
    //
    //    model |> withInitTimePointListModel timePoints |> withCmdNone
    //
    // | Msg.LoadCurrentWork ->
    //    match cfg.CurrentWorkItemSettings.CurrentWork with
    //    | None -> model, Cmd.none
    //    | Some work ->
    //        model, Cmd.OfTask.perform findWorkByIdOrCreateTask work Msg.SetCurrentWorkIfNone
    //
    // | Msg.SetCurrentWorkIfNone res ->
    //    model |> setCurrentWorkIfNone cfg.UserSettings res