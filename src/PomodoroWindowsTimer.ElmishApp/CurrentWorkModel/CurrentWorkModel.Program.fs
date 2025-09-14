module PomodoroWindowsTimer.ElmishApp.CurrentWorkModel.Program

open System
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.CurrentWorkModel
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Types

/// Msg.SetCurrentWorkIfNone handler.
let private setCurrentWorkIfNone (userSettings: ICurrentWorkItemSettings) (workRes: Result<Work, string>) (model: CurrentWorkModel) =
    match workRes with
    | Ok work ->
        match model.Work with
        | None ->
            userSettings.CurrentWork <- work |> Some
            model |> withWork work |> withCmdNone
        | _ ->
            // skip 
            model, Cmd.none

    | Error err ->
        if model.Work.IsNone then userSettings.CurrentWork <- None
        model, Cmd.ofMsg (Msg.OnError err)

/// Msg.LooperMsg handler. When Work is Some.
let private mapLooperMsgWithWork (workEventStore: WorkEventStore) (telegramBot: ITelegramBot) (levt: LooperEvent) (model: CurrentWorkModel) =
    match levt with
    | LooperEvent.TimePointStarted ({ IsPlaying = false; OldActiveTimePoint = None; NewActiveTimePoint = atp }, _) ->
        model |> withIsPlaying false
        , Cmd.OfTask.attempt workEventStore.StoreActiveTimePointTask atp Msg.OnExn

    | LooperEvent.TimePointStarted ({ IsPlaying = true; NewActiveTimePoint = atp; SwitchingMode = switchinMode }, sentTime) ->
        let currentWork = model.Work.Value

        model |> withIsPlaying true
        , Cmd.batch [
            Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (currentWork.Id, sentTime, atp) Msg.OnExn

            if atp.Kind = Kind.Work && switchinMode = TimePointSwitchingMode.Auto then
                let telegramMsg =
                    $"""It's time to {atp.Name}!!
                    
Current work is [{currentWork.Number}] {currentWork.Title}."""
                Cmd.OfTask.attempt telegramBot.SendMessage telegramMsg Msg.OnExn
        ]

    | LooperEvent.TimePointStarted ({ IsPlaying = isPlaying }, _)
    | LooperEvent.TimePointTimeReduced ({ IsPlaying = isPlaying }, _) ->
        model |> withIsPlaying isPlaying, Cmd.none

    | LooperEvent.TimePointStopped (atp, sentTime) ->
        model |> withIsPlaying false, Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (model.Id.Value, sentTime, atp) Msg.OnExn


/// Msg.LooperMsg handler. When Work is Some.
let private mapLooperMsgWithoutWork (workEventStore: WorkEventStore) (telegramBot: ITelegramBot) (levt: LooperEvent) (model: CurrentWorkModel) =
    match levt with
    | LooperEvent.TimePointStarted ({ IsPlaying = false; OldActiveTimePoint = None; NewActiveTimePoint = atp }, _) ->
        model |> withIsPlaying false
        , Cmd.OfTask.attempt workEventStore.StoreActiveTimePointTask atp Msg.OnExn

    | LooperEvent.TimePointStarted ({ IsPlaying = true; NewActiveTimePoint = atp; SwitchingMode = switchinMode }, _) ->
        model |> withIsPlaying true
        , Cmd.batch [
            if atp.Kind = Kind.Work && switchinMode = TimePointSwitchingMode.Auto then
                Cmd.OfTask.attempt telegramBot.SendMessage $"It's time to {atp.Name}!!" Msg.OnExn
        ]

    | LooperEvent.TimePointStarted ({ IsPlaying = isPlaying }, _)
    | LooperEvent.TimePointTimeReduced ({ IsPlaying = isPlaying }, _) ->
        model |> withIsPlaying isPlaying, Cmd.none

    | LooperEvent.TimePointStopped _ ->
        model |> withIsPlaying false, Cmd.none


/// Msg.SetWork handler
let private setWork (looper: ILooper) (workEventStore: WorkEventStore) (timeProvider: TimeProvider) (work: Work) (model: CurrentWorkModel) =
    model |> withWork work
    , (
        match model.IsPlaying, model.Work, (looper.GetActiveTimePoint()) with
        | true, Some prevWork, Some atp ->
            Cmd.batch [
                Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (prevWork.Id, timeProvider.GetUtcNow(), atp) Msg.OnExn
                Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (work.Id, timeProvider.GetUtcNow(), atp) Msg.OnExn
            ]
        | true, None, Some atp ->
            Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (work.Id, timeProvider.GetUtcNow(), atp) Msg.OnExn

        | _ ->
            Cmd.none
    )

/// Msg.SetWork handler
let private unsetWork (looper: ILooper) (workEventStore: WorkEventStore) (timeProvider: TimeProvider) (model: CurrentWorkModel) =
    model |> withoutWork
    , (
        match model.IsPlaying, model.Work, (looper.GetActiveTimePoint()) with
        | true, Some prevWork, Some atp ->
            Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (prevWork.Id, timeProvider.GetUtcNow(), atp) Msg.OnExn

        | _ ->
            Cmd.none
    )

/// Update function.
let update
    (currentWorkSettings: ICurrentWorkItemSettings)
    (workEventStore: WorkEventStore)
    (looper: ILooper)
    (timeProvider: TimeProvider)
    (telegramBot: ITelegramBot)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<CurrentWorkModel>)
    (msg: CurrentWorkModel.Msg)
    (model: CurrentWorkModel)
    =
    match msg with
    | Msg.SetCurrentWorkIfNone workRes ->
        model |> setCurrentWorkIfNone currentWorkSettings workRes

    | Msg.SetWork w when model.Work |> Option.map (fun mw -> w.Id <> mw.Id) |> Option.defaultValue true ->
        model |> setWork looper workEventStore timeProvider w

    | Msg.SetWork _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none

    | Msg.UnsetWork when model.Work.IsSome ->
        model |> unsetWork looper workEventStore timeProvider

    | Msg.UnsetWork ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none

    | Msg.LooperMsg levt when model.Work.IsSome ->
        model |> mapLooperMsgWithWork workEventStore telegramBot levt

    | Msg.LooperMsg levt (* when model.Work.IsNone *) ->
        model |> mapLooperMsgWithoutWork workEventStore telegramBot levt

    | Msg.OnError err ->
        logger.LogError err
        errorMessageQueue.EnqueueError err
        model, Cmd.none

    | Msg.OnExn ex ->
        logger.LogError(ex, "Exception has been thrown in PlayerModel Program.")
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none


