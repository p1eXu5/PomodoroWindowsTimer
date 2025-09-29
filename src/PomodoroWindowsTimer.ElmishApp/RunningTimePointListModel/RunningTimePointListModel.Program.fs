module PomodoroWindowsTimer.ElmishApp.RunningTimePointListModel.Program

open System
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.RunningTimePointListModel

/// Msg.LooperMsg handler.
let private mapLooperMsg updateTimePointModel levt (model: RunningTimePointListModel) =
    match levt with
    // preload, stopped
    | LooperEvent.TimePointStopped (atp, _)
    | LooperEvent.TimePointReady (atp, _) ->
        model.TimePoints
        |> List.mapFirstCmd
            (fun tp -> tp.Id = atp.Id && not (tp.IsSelected && not tp.IsPlaying))
            (updateTimePointModel (TimePointModel.Msg.SetIsSelectedNotPlaying))
        |> fun (tpList, tpCmd) ->
            match model.ActiveTimePointId with
            | Some oldTpId when oldTpId <> atp.Id ->
                tpList
                |> List.mapFirstCmd
                    (fun tp -> tp.Id = oldTpId && tp.IsSelected)
                    (updateTimePointModel (TimePointModel.Msg.SetIsSelected false))
                |> fun (tpList', tpCmd') ->
                    { model with TimePoints = tpList' }
                    |> withActiveTimePointId (atp.OriginalId |> Some)
                    , Cmd.batch [
                        Cmd.map (fun m -> Msg.TimePointModelMsg (atp.Id, m)) tpCmd
                        Cmd.map (fun m -> Msg.TimePointModelMsg (oldTpId, m)) tpCmd'
                    ]
                    , Intent.None
            | _ ->
                { model with TimePoints = tpList }
                |> withActiveTimePointId (atp.OriginalId |> Some)
                , Cmd.map (fun m -> Msg.TimePointModelMsg (atp.Id, m)) tpCmd
                , Intent.None

    // started
    | LooperEvent.TimePointStarted ({ NewActiveTimePoint = atp; }, _) ->
        model.TimePoints
        |> List.mapFirstCmd
            (fun tp -> tp.Id = atp.Id && not (tp.IsSelected && tp.IsPlaying))
            (updateTimePointModel (TimePointModel.Msg.SetIsSelectedIsPlaying))
        |> fun (tpList, tpCmd) ->
            match model.ActiveTimePointId with
            | Some oldTpId when oldTpId <> atp.Id ->
                tpList
                |> List.mapFirstCmd
                    (fun tp -> tp.Id = oldTpId && tp.IsSelected)
                    (updateTimePointModel (TimePointModel.Msg.SetIsNotSelectedIsPlayed))
                |> fun (tpList', tpCmd') ->
                    { model with TimePoints = tpList' }
                    |> withActiveTimePointId (atp.OriginalId |> Some)
                    , Cmd.batch [
                        Cmd.map (fun m -> Msg.TimePointModelMsg (atp.Id, m)) tpCmd
                        Cmd.map (fun m -> Msg.TimePointModelMsg (oldTpId, m)) tpCmd'
                    ]
                    , Intent.None
            | _ ->
                { model with TimePoints = tpList }
                |> withActiveTimePointId (atp.OriginalId |> Some)
                , Cmd.map (fun m -> Msg.TimePointModelMsg (atp.Id, m)) tpCmd
                , Intent.None

    | LooperEvent.TimePointTimeReduced ({ ActiveTimePoint = atp; IsPlaying = isPlaying }, _) when model.ActiveTimePointId.IsNone ->
        if isPlaying then
            model.TimePoints
            |> List.mapFirstCmd
                (fun tp -> tp.Id = atp.Id && not (tp.IsSelected && tp.IsPlaying))
                (updateTimePointModel (TimePointModel.Msg.SetIsSelectedIsPlaying))
        else
            model.TimePoints
            |> List.mapFirstCmd
                (fun tp -> tp.Id = atp.Id && not (tp.IsSelected && not tp.IsPlaying))
                (updateTimePointModel (TimePointModel.Msg.SetIsSelectedIsStopped))
        |> fun (tpList, tpCmd) ->
            { model with TimePoints = tpList }
            |> withActiveTimePointId (atp.OriginalId |> Some)
            , Cmd.map (fun m -> Msg.TimePointModelMsg (atp.Id, m)) tpCmd
            , Intent.None

    | LooperEvent.TimePointTimeReduced _ ->
        model, Cmd.none, Intent.None

/// Msg.TimePointModelMsg handler.
let private mapTimePointModelMsg updateTimePointModel tpId tpMsg (model: RunningTimePointListModel) =
    model.TimePoints
    |> List.mapFirstCmd (_.Id >> (=) tpId) (updateTimePointModel tpMsg)
    |> fun (listModel, cmd) ->
        { model with TimePoints = listModel }
        , Cmd.map (fun smsg -> Msg.TimePointModelMsg (tpId, smsg)) cmd
        , Intent.None

let update
    (playerUserSettings: IPlayerUserSettings)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<RunningTimePointListModel>)
    updateTimePointModel
    msg model
    =
    match msg with
    | Msg.SetActiveTimePointId tpId when tpId <> model.ActiveTimePointId ->
        model |> withActiveTimePointId tpId
        , Cmd.none
        , Intent.None

    | Msg.TimePointModelMsg (tpId, tpMsg) ->
        model |> mapTimePointModelMsg updateTimePointModel tpId tpMsg

    | Msg.LooperMsg levt ->
        model |> mapLooperMsg updateTimePointModel levt

    | Msg.SetDisableSkipBreak v ->
        model
        , Cmd.OfFunc.attempt (fun v' -> playerUserSettings.DisableSkipBreak <- v') v Msg.OnExn
        , Intent.None

    | Msg.SetDisableMinimizeMaximizeWindows v ->
        model
        , Cmd.OfFunc.attempt (fun v' -> playerUserSettings.DisableMinimizeMaximizeWindows <- v') v Msg.OnExn
        , Intent.None

    | Msg.PlayerUserSettingsChanged ->
        model
        |> withDisableSkipBreak playerUserSettings.DisableSkipBreak
        |> withDisableMinimizeMaximizeWindows playerUserSettings.DisableMinimizeMaximizeWindows
        , Cmd.none
        , Intent.None

    | Msg.TimePointsChangedQueueMsg (timePoints, timePointIdOpt) ->
        model |> withTimePointQueueTimePoints timePoints timePointIdOpt
        , Cmd.none
        , Intent.None

    | Msg.TimePointsLoopComplettedQueueMsg ->
        model |> withNotPlayedTimePoints
        , Cmd.none
        , Intent.None

    | Msg.RequestTimePointGenerator ->
        model, Cmd.none, Intent.RequestTimePointGenerator

    | Msg.OnExn ex ->
        logger.LogProgramExn ex
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none, Intent.None

    | _ ->
        logger.LogNonProcessedMessage(msg, model)
        model, Cmd.none, Intent.None

