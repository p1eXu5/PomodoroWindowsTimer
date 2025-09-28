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
open PomodoroWindowsTimer.Abstractions


/// Msg.SetIsTimePointsShown handler.
let private setIsTimePointsDrawerShown initDrawerModel (v: bool) (model: MainModel) =
    if v
    then
        model |> withTimePointsDrawerModel (model.TimePointsDrawer |> initDrawerModel)
    else model
    |> withIsTimePointsDrawerShown v
    , Cmd.none


/// Msg.TimePointsDrawerMsg handler
let private mapTimePointsDrawerMsg updateTimePointsDrawerModel smsg (model: MainModel) =
    let (drawerModel', drawerCmd) = model.TimePointsDrawer |> updateTimePointsDrawerModel smsg
    model |> withTimePointsDrawer drawerModel'
    , Cmd.map Msg.TimePointsDrawerMsg drawerCmd


/// Msg.TimePointsChangedQueueMsg handler
let private mapTimePointsChangedQueueMsg updateTimePointsDrawerModel (timePointsAndId: TimePoint list * TimePointId option) (model: MainModel) =
    model
    |> mapc _.TimePointsDrawer withTimePointsDrawer Msg.TimePointsDrawerMsg (
        updateTimePointsDrawerModel (TimePointsDrawerModel.Msg.RunningTimePointsMsg (
            RunningTimePointListModel.Msg.TimePointsChangedQueueMsg timePointsAndId)))


/// Msg.TimePointsLoopComplettedQueueMsg handler
let private mapTimePointsLoopComplettedQueueMsg updateTimePointsDrawerModel (model: MainModel) =
    model
    |> mapc _.TimePointsDrawer withTimePointsDrawer Msg.TimePointsDrawerMsg (
        updateTimePointsDrawerModel (TimePointsDrawerModel.Msg.RunningTimePointsMsg (
            RunningTimePointListModel.Msg.TimePointsLoopComplettedQueueMsg
        ))
    )


/// SetIsWorkSelectorLoaded handler
let private setIsWorkSelectorLoaded initRunningTimePoints initWorkSelectorModel (v: bool) (model: MainModel) =
    if v then
        let (m, cmd) = initWorkSelectorModel (model.CurrentWork.Id)
        model |> withWorkSelectorModel m |> withIsTimePointsDrawerShown false
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


let private mapCurentWorkModelMsg updateCurrentWorkModel msg (model: MainModel) =
    model |> mapc _.CurrentWork withCurrentWorkModel Msg.CurrentWorkModelMsg (updateCurrentWorkModel msg)


/// MainModel.Program update function.
let update
    (telegramBot: ITelegramBot)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<MainModel>)
    updatePlayerModel
    updateCurrentWorkModel
    initTimePointsDrawerModel
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
        model |> setIsTimePointsDrawerShown initTimePointsDrawerModel v

    | Msg.TimePointsDrawerMsg smsg ->
        model |> mapTimePointsDrawerMsg updateTimePointsDrawerModel smsg

    | Msg.TimePointsChangedQueueMsg (timePoints, timePointIdOpt) ->
        model |> mapTimePointsChangedQueueMsg updateTimePointsDrawerModel (timePoints, timePointIdOpt)

    | Msg.TimePointsLoopComplettedQueueMsg  ->
        model |> mapTimePointsLoopComplettedQueueMsg updateTimePointsDrawerModel

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
        model |> mapCurentWorkModelMsg updateCurrentWorkModel currWorkMsg

    | Msg.SetIsWorkSelectorLoaded v ->
        model |> setIsWorkSelectorLoaded initTimePointsDrawerModel initWorkSelectorModel v

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
