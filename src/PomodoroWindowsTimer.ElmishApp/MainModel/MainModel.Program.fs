module PomodoroWindowsTimer.ElmishApp.MainModel.Program

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

let private withUpdatedPlayerModel updatef pmsg (model: MainModel) =
    let (playerModel, playerCmd, playerIntent) =
        model.Player
        |> updatef (model.CurrentWork |> Option.map _.Work) pmsg

    let cmd = Cmd.map Msg.PlayerModelMsg playerCmd

    match playerIntent with
    | PlayerModel.Intent.None ->
        model |> withPlayerModel playerModel
        , cmd

    | PlayerModel.Intent.RollbackTime (workSpentTime, kind, atpId, time) ->
        model |> withPlayerModel playerModel
        , Cmd.batch [
            cmd
            Cmd.ofMsg (Msg.AppDialogModelMsg (AppDialogModel.Msg.LoadRollbackWorkDialogModel (workSpentTime, kind, atpId, time)))
        ]

    | PlayerModel.Intent.MultipleRollbackTime (workSpentTime, kind, atpId, time) ->
        model |> withPlayerModel playerModel
        , Cmd.batch [
            cmd
            Cmd.ofMsg (Msg.AppDialogModelMsg (AppDialogModel.Msg.LoadRollbackWorkListDialogModel (workSpentTime, kind, atpId, time)))
        ]

    | PlayerModel.Intent.SkipOrApplyMissingTime (work, kind, atpId, diff, time) ->
        model |> withPlayerModel playerModel
        , Cmd.batch [
            cmd
            Cmd.ofMsg (Msg.AppDialogModelMsg (AppDialogModel.Msg.LoadSkipOrApplyMissingTimeDialogModel (work, kind, atpId, diff, time)))
        ]

let private withUpdatedTimePointListModel updatef tplMsg (model: MainModel) =
    let (tplModel) = model.TimePointList |> updatef tplMsg
    model |> withTimePointListModel tplModel |> withCmdNone

let private chain f (model, cmd) =
    let (model', cmd') = f model
    model', Cmd.batch [ cmd; cmd' ]

let update
    (cfg: MainModeConfig)
    (workEventStore: WorkEventStore)
    updateWorkModel
    updateAppDialogModel
    updateWorkSelectorModel
    initWorkStatisticListModel
    updateStatisticMainModel
    updatePlayerModel
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<MainModel>)
    (msg: Msg)
    (model: MainModel)
    =
    let findWorkByIdOrCreateTask work =
        task {
            let workRepo = cfg.WorkEventStore.GetWorkRepository ()
            return! workRepo.FindByIdOrCreateAsync work CancellationToken.None
        }

    match msg with
    // --------------------
    // Time Points
    // --------------------
    | Msg.SetIsTimePointsShown v ->
        if v then
            model |> withoutWorkSelectorModel |> withIsTimePointsShown v |> withCmdNone
        else
            model |> withIsTimePointsShown v |> withCmdNone

    | Msg.LoadTimePointsFromSettings ->
        let timePoints = cfg.TimePointStore.Read()
        cfg.TimePointQueue.Reload(timePoints)
        cfg.Looper.PreloadTimePoint()
        
        model |> withInitTimePointListModel timePoints |> withCmdNone

    | Msg.LoadTimePoints timePoints ->
        cfg.Looper.Stop()
        cfg.TimePointQueue.Reload(timePoints)
        cfg.TimePointStore.Write(timePoints)
        cfg.Looper.PreloadTimePoint()

        model |> withInitTimePointListModel timePoints |> withCmdNone

    | Msg.TimePointListModelMsg smsg ->
        model |> withUpdatedTimePointListModel TimePointListModel.Program.update smsg

    | Msg.StartTimePoint tpId ->
        model, Cmd.ofMsg (tpId |> Operation.Start |> PlayerModel.Msg.StartTimePoint |> Msg.PlayerModelMsg)

    | Msg.PlayStopCommand _ ->
        model, Cmd.ofMsg (model.Player |> PlayerModel.Msg.playStopResume |> Msg.PlayerModelMsg)

    | Msg.LooperMsg lmsg ->
        match lmsg with
        | LooperMsg.TimePointTimeReduced tp ->
            model
            |> withUpdatedPlayerModel
                updatePlayerModel
                (PlayerModel.LooperMsg.TimePointTimeReduced tp |> PlayerModel.Msg.LooperMsg)

        | LooperMsg.TimePointStarted args ->
            model
            |> withUpdatedPlayerModel
                updatePlayerModel
                (PlayerModel.LooperMsg.TimePointStarted args |> PlayerModel.Msg.LooperMsg)
            |> chain (
                withUpdatedTimePointListModel
                    TimePointListModel.Program.update
                    (TimePointListModel.Msg.SetActiveTimePointId (args.NewActiveTimePoint.OriginalId |> Some))
            )

    // --------------------
    // Work
    // --------------------
    | Msg.LoadCurrentWork ->
        match cfg.CurrentWorkItemSettings.CurrentWork with
        | None -> model, Cmd.none
        | Some work ->
            model, Cmd.OfTask.perform findWorkByIdOrCreateTask work Msg.SetCurrentWorkIfNone

    | Msg.SetCurrentWorkIfNone res ->
        match res with
        | Ok work ->
            cfg.CurrentWorkItemSettings.CurrentWork <- work |> Some
            if model.CurrentWork |> Option.isNone then
                { model with CurrentWork = work |> WorkModel.init |> Some}, Cmd.none
            else
                model, Cmd.none
        | Error err ->
            cfg.CurrentWorkItemSettings.CurrentWork <- None
            model, Cmd.ofMsg (Msg.OnError err)
   

    | Msg.WorkModelMsg wmsg ->
        match model.CurrentWork with
        | Some wmodel ->
            let (wmodel, wcmd, _) = updateWorkModel wmsg wmodel
            model |> withWorkModel (wmodel |> Some)
            , Cmd.map Msg.WorkModelMsg wcmd
        | None ->
            model |> withCmdNone

    | Msg.SetIsWorkSelectorLoaded v ->
        if v then
            let (m, cmd) = WorkSelectorModel.init cfg.UserSettings (model.CurrentWork |> Option.map (_.Work >> _.Id))
            model |> withWorkSelectorModel (m |> Some) |> withIsTimePointsShown false
            , Cmd.map Msg.WorkSelectorModelMsg cmd
        else
            model |> withoutWorkSelectorModel |> withCmdNone

    | MsgWith.WorkSelectorModelMsg model (smsg, m) ->
        let (workSelectorModel, workSelectorCmd, intent) = updateWorkSelectorModel smsg m
        let cmd =  Cmd.map Msg.WorkSelectorModelMsg workSelectorCmd

        match intent with
        | WorkSelectorModel.Intent.None ->
            model |> withWorkSelectorModel (workSelectorModel |> Some)
            , cmd
        | WorkSelectorModel.Intent.SelectCurrentWork workModel ->
            cfg.CurrentWorkItemSettings.CurrentWork <- workModel.Work |> Some

            if model.Player.LooperState = LooperState.Playing then
                let time = cfg.TimeProvider.GetUtcNow()
                match model.CurrentWork with
                | Some currWork when currWork.Id <> workModel.Id ->
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel (workModel |> Some)
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (currWork.Id, time.AddMilliseconds(-1), model.Player.ActiveTimePoint.Value) Msg.OnExn
                        Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (workModel.Id, time, model.Player.ActiveTimePoint.Value) Msg.OnExn
                    ]
                | Some _ ->
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel (workModel |> Some)
                    , cmd
                | None ->
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel (workModel |> Some)
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (workModel.Id, time, model.Player.ActiveTimePoint.Value) Msg.OnExn
                    ]
            else
                model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel (workModel |> Some)
                , cmd

        | WorkSelectorModel.Intent.UnselectCurrentWork ->
            cfg.CurrentWorkItemSettings.CurrentWork <- None

            if model.Player.LooperState = LooperState.Playing then
                match model.CurrentWork with
                | Some currWork ->
                    let time = cfg.TimeProvider.GetUtcNow()
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel None
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (currWork.Id, time, model.Player.ActiveTimePoint.Value) Msg.OnExn
                    ]
                | None ->
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel None
                    , cmd
            else
                model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel None
                , cmd

        | WorkSelectorModel.Intent.Close ->
            model |> withWorkSelectorModel None |> withCmdNone

    // --------------------
    // Player, Windows
    // --------------------

    | Msg.PlayerModelMsg pmsg ->
        model |> withUpdatedPlayerModel updatePlayerModel pmsg

    | Msg.SetIsWorkStatisticShown v ->
        if v then
            let (m, cmd) = initWorkStatisticListModel ()
            model |> MainModel.withDailyStatisticList (m |> Some)
            , Cmd.map Msg.StatisticMainModelMsg cmd
        else
            model |> MainModel.withDailyStatisticList None
            , Cmd.none


    | MsgWith.StatisticMainModelMsg model (smsg, sm) ->
        let (m, cmd, intent) = updateStatisticMainModel smsg sm
        match intent with
        | StatisticMainModel.Intent.None ->
            model |> MainModel.withDailyStatisticList (m |> Some)
            , Cmd.map Msg.StatisticMainModelMsg cmd
        | StatisticMainModel.Intent.CloseWindow ->
            model |> MainModel.withDailyStatisticList None
            , Cmd.none

    // --------------------
    
    | Msg.SendToChatBot message ->
        model, Cmd.OfTask.attempt cfg.TelegramBot.SendMessage message Msg.OnExn

    | Msg.AppDialogModelMsg smsg ->
        let (m, cmd) = updateAppDialogModel smsg model.AppDialog
        model |> withAppDialogModel m
        , Cmd.map Msg.AppDialogModelMsg cmd

    // --------------------

    | Msg.OnExn ex ->
        logger.LogError(ex, "Exception has been thrown in MainModel Program.")
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none

    | Msg.OnError err ->
        errorMessageQueue.EnqueueError err
        model, Cmd.none

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none
