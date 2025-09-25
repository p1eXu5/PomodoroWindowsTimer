module PomodoroWindowsTimer.ElmishApp.TimePointsDrawerModel.Program

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
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsDrawerModel

let private runningTimePointListIntentCmd runningTimePointListIntent =
    match runningTimePointListIntent with
    | RunningTimePointListModel.Intent.None -> Cmd.none
    | RunningTimePointListModel.Intent.RequestTimePointGenerator ->
        // we need msg because sub model can emit message too.
        Cmd.ofMsg Msg.InitTimePointGenerator

/// Msg.RunningTimePointsMsg handler
let private mapRunningTimePointsMsg updateRtpListModel smsg rtpListModel =
    rtpListModel |> updateRtpListModel smsg
    |> fun (rtpListModel', cmd, intent) ->
        rtpListModel' |> TimePointsDrawerModel.RunningTimePoints
        , Cmd.batch [
            Cmd.map Msg.RunningTimePointsMsg cmd
            intent |> runningTimePointListIntentCmd
        ]

let private timePointsGeneratorIntentCmd runningTimePointListIntent =
    match runningTimePointListIntent with
    | TimePointsGeneratorModel.Intent.None -> Cmd.none
    | TimePointsGeneratorModel.Intent.ApplyGeneratedTimePoints
    | TimePointsGeneratorModel.Intent.CancelTimePointGeneration ->
        // we need msg because sub model can emit message too.
        Cmd.ofMsg Msg.InitRunningTimePoints

/// Msg.TimePointGeneratorMsg handler
let private mapTimePointsGeneratorMsg updateGenModel smsg (genModel, tpStates) =
    genModel |> updateGenModel smsg
    |> fun (genModel', cmd, intent) ->
        (genModel', tpStates) |> TimePointsDrawerModel.TimePointsGenerator
        , Cmd.batch [
            Cmd.map Msg.TimePointsGeneratorMsg cmd
            intent |> timePointsGeneratorIntentCmd
        ]

let update
    (logger: ILogger<TimePointsDrawerModel>)
    initRtpListModel
    updateRtpListModel
    initGenModel
    updateGenModel
    msg model
    =
    match msg with
    | MsgWith.RunningTimePointsMsg model (smsg, rtpListModel) ->
        rtpListModel |> mapRunningTimePointsMsg updateRtpListModel smsg

    | MsgWith.LooperMsg model (lmsg, rtpListModel) ->
        rtpListModel |> mapRunningTimePointsMsg updateRtpListModel (RunningTimePointListModel.Msg.LooperMsg lmsg)

    | MsgWith.TimePointGeneratorMsg model (smsg, genModel, tpStates) ->
        (genModel, tpStates) |> mapTimePointsGeneratorMsg updateGenModel smsg

    | Msg.LooperMsg _ ->
        // skip logging of unhandled looper evt
        model, Cmd.none

    | Msg.InitTimePointGenerator ->
        model |> initWithTimePointsGenerator initGenModel

    | Msg.InitRunningTimePoints ->
        model |> initWithRunningTimePoints initRtpListModel
        , Cmd.none

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none

