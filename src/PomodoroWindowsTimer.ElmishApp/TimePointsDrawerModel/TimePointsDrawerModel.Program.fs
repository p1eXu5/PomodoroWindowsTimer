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

let runningTimePointListIntentCmd runningTimePointListIntent =
    match runningTimePointListIntent with
    | RunningTimePointListModel.Intent.None -> Cmd.none
    | RunningTimePointListModel.Intent.RequestTimePointGenerator ->
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


/// Msg.TimePointGeneratorMsg handler
let private mapTimePointsGeneratorMsg updateGenModel smsg genModel =
    genModel |> updateGenModel smsg
    |> fun (genModel', cmd) ->
        genModel' |> TimePointsDrawerModel.TimePointsGenerator
        , Cmd.map Msg.TimePointsGeneratorMsg cmd

let update
    (logger: ILogger<TimePointsDrawerModel>)
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

    | MsgWith.TimePointGeneratorMsg model (smsg, genModel) ->
        genModel |> mapTimePointsGeneratorMsg updateGenModel smsg

    | Msg.LooperMsg _ ->
        // skip logging of unhandled looper evt
        model, Cmd.none

    | Msg.InitTimePointGenerator ->
        initGenModel ()
        |> fun (genModel, cmd) ->
            genModel |> TimePointsDrawerModel.TimePointsGenerator
            , Cmd.map Msg.TimePointsGeneratorMsg cmd

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none

