namespace PomodoroWindowsTimer.ElmishApp.TimePointModel

open System
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointModel
open PomodoroWindowsTimer.Abstractions

module Program =

    open Microsoft.Extensions.Logging
    
    open Elmish

    open PomodoroWindowsTimer.ElmishApp.Abstractions
    open PomodoroWindowsTimer.ElmishApp.Logging

    let mapPlayMsg (timePointQueue: ITimePointQueue) (looper: ILooper) (model: TimePointModel) =
        model
        , Cmd.OfFunc.attempt
            (fun tpId ->
                if timePointQueue.ScrollTo tpId then
                    looper.Next()
                else
                    ()
            )
            model.Id
            Msg.OnExn

    let mapStopMsg (looper: ILooper) (model: TimePointModel) =
        model
        , Cmd.OfFunc.attempt
            looper.Stop
            ()
            Msg.OnExn

    let update (timePointQueue: ITimePointQueue) (looper: ILooper) (errorMessageQueue: IErrorMessageQueue) (logger: ILogger<TimePointModel>) msg model =
        match msg with
        | SetName v -> { model with TimePoint.Name = v }, Cmd.none

        | SetTimeSpan v -> 
            { model with TimePoint.TimeSpan = TimeSpan.ParseExact(v, "h':'mm", null) }, Cmd.none

        | SetIsSelected v ->
            { model with IsSelected = v }, Cmd.none

        | SetIsPlayed v ->
            { model with IsSelected = v }, Cmd.none

        | SetIsPlaying v ->
            { model with IsPlaying = v }, Cmd.none

        | Play ->
            model |> mapPlayMsg timePointQueue looper

        | Stop ->
            model |> mapStopMsg looper

        | Msg.OnExn ex ->
            logger.LogProgramExn ex
            errorMessageQueue.EnqueueError ex.Message
            model, Cmd.none


