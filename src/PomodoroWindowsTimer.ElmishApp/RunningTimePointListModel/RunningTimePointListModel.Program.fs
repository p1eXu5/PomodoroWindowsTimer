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

let private mapLooperMsg levt (model: RunningTimePointListModel) =
    match levt with
    | LooperEvent.TimePointStarted ({ NewActiveTimePoint = atp }, _)
    | LooperEvent.TimePointStopped (atp, _)
    | LooperEvent.TimePointTimeReduced ({ ActiveTimePoint = atp }, _) ->
        model |> withActiveTimePointId (atp.OriginalId |> Some)
        , Cmd.none
        , Intent.None

let update
    (playerUserSettings: IPlayerUserSettings)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<RunningTimePointListModel>)
    msg model
    =
    match msg with
    | Msg.SetActiveTimePointId tpId ->
        model |> withActiveTimePointId tpId
        , Cmd.none
        , Intent.None

    | Msg.LooperMsg levt ->
        model |> mapLooperMsg levt

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

    | Msg.TimePointQueueMsg (timePoints, timePointIdOpt) ->
        model |> withTimePoints timePoints timePointIdOpt
        , Cmd.none
        , Intent.None

    | Msg.RequestTimePointGenerator ->
        model, Cmd.none, Intent.RequestTimePointGenerator

    | Msg.OnExn ex ->
        logger.LogProgramExn ex
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none, Intent.None

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none, Intent.None

