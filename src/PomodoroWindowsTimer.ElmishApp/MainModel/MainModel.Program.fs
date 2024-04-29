module PomodoroWindowsTimer.ElmishApp.MainModel.Program

open System.Threading
open Microsoft.Extensions.Logging
open Elmish
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Logging

let updateOnPlayerMsg
    (cfg: MainModeConfig)
    (logger: ILogger<MainModel>)
    (msg: PlayerMsg)
    (model: MainModel)
    =
    match msg with
    | PlayerMsg.Next ->
        cfg.Looper.Next()
        model |> setLooperState Playing |> setUIInitiator, Cmd.none

    | PlayerMsg.Play ->
        cfg.Looper.Next()
        model |> setLooperState Playing |> setUIInitiator, Cmd.none

    // TODO: logic can be moved into Msg.LooperMsg handler
    | PlayerMsg.Resume when model.ActiveTimePoint |> Option.isSome ->
        cfg.Looper.Resume()
        let model' = model |> setLooperState Playing

        match model.ActiveTimePoint with
        | Some ({ Kind = Kind.Break }) when not model.IsMinimized ->
            model', Cmd.ofMsg (WindowsMsg.MinimizeWindows |> Msg.WindowsMsg)

        | Some ({ Kind = Kind.Work }) when model.IsMinimized ->
            model', Cmd.ofMsg (WindowsMsg.RestoreWindows |> Msg.WindowsMsg)

        | _ ->
            model', Cmd.none

    | PlayerMsg.Stop when model.LooperState = LooperState.Playing ->
        cfg.Looper.Stop()
        model |> setLooperState Stopped
        , Cmd.batch [
            Cmd.ofMsg (WindowsMsg.RestoreWindows |> Msg.WindowsMsg)
            Cmd.ofMsg (WindowsMsg.RestoreMainWindow |> Msg.WindowsMsg)
        ]

    | PlayerMsg.Replay when model.ActiveTimePoint |> Option.isSome ->
        let cmd =
            Cmd.batch [Cmd.ofMsg Msg.PreChangeActiveTimeSpan; Cmd.ofMsg (ChangeActiveTimeSpan 0.0); Cmd.ofMsg (PlayerMsg.Resume |> Msg.PlayerMsg)]
        model, cmd

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none


let updateOnWindowsMsg (cfg: MainModeConfig) (logger: ILogger<MainModel>) (msg: WindowsMsg) (model: MainModel) =
    match msg with
    | WindowsMsg.MinimizeWindows when not model.IsMinimized ->
        { model with IsMinimized = true }, Cmd.OfAsync.either cfg.WindowsMinimizer.MinimizeOther () (fun _ -> WindowsMsg.SetIsMinimized true |> Msg.WindowsMsg) Msg.OnExn

    | WindowsMsg.RestoreWindows when model.IsMinimized ->
        { model with IsMinimized = false }, Cmd.OfAsync.either cfg.WindowsMinimizer.Restore () (fun _ -> WindowsMsg.SetIsMinimized false |> Msg.WindowsMsg) Msg.OnExn

    | WindowsMsg.RestoreMainWindow ->
        model, Cmd.OfAsync.attempt cfg.WindowsMinimizer.RestoreMainWindow () Msg.OnExn

    | WindowsMsg.SetIsMinimized v ->
        { model with IsMinimized = v }, Cmd.none

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none


let update
    (cfg: MainModeConfig)
    updateWorkModel
    updateAppDialogModel
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<MainModel>)
    (msg: Msg)
    (model: MainModel)
    =
    let updateOnPlayerMsg = updateOnPlayerMsg cfg
    let updateOnWindowsMsg = updateOnWindowsMsg cfg

    match msg with
    // --------------------
    // Flags
    // --------------------
    | Msg.SetDisableSkipBreak v ->
        cfg.DisableSkipBreakSettings.DisableSkipBreak <- v
        { model with DisableSkipBreak = v }, Cmd.none

    | Msg.SetDisableMinimizeMaximizeWindows v ->
        { model with DisableMinimizeMaximizeWindows = v }, Cmd.none

    // --------------------
    // Time Points
    // --------------------
    | Msg.LoadTimePointsFromSettings ->
        let timePoints = cfg.TimePointStore.Read()
        cfg.TimePointQueue.Reload(timePoints)
        cfg.Looper.PreloadTimePoint()
        { model with TimePoints = timePoints }, Cmd.none

    | Msg.LoadTimePoints timePoints ->
        cfg.Looper.Stop()
        cfg.TimePointQueue.Reload(timePoints)
        cfg.TimePointStore.Write(timePoints)
        cfg.Looper.PreloadTimePoint()

        { model with TimePoints = timePoints }
        , Cmd.none

    | StartTimePoint (Operation.Start id) ->
        model, Cmd.batch [ Cmd.ofMsg (PlayerMsg.Stop |> Msg.PlayerMsg); Cmd.OfFunc.either cfg.Looper.TimePointQueue.ScrollTo id (Operation.Finish >> Msg.StartTimePoint) OnExn ]

    | StartTimePoint (Operation.Finish _) ->
        model, Cmd.ofMsg (PlayerMsg.Play |> Msg.PlayerMsg)

    // --------------------
    // Work
    // --------------------
    | Msg.LoadCurrentWork ->
        match cfg.CurrentWorkItemSettings.CurrentWork with
        | None -> model, Cmd.none
        | Some work ->
            model, Cmd.OfTask.perform (cfg.WorkRepository.FindByIdOrCreate work) CancellationToken.None Msg.SetCurrentWorkIfNone

    | Msg.SetCurrentWorkIfNone res ->
        match res with
        | Ok work when model.Work |> Option.isNone ->
            { model with Work = work |> WorkModel.init |> Some}, Cmd.none
        | Error err -> model, Cmd.ofMsg (Msg.OnError err)
        | _ -> model, Cmd.none

    | Msg.WorkModelMsg wmsg ->
        match model.Work with
        | Some wmodel ->
            let (wmodel, wcmd) = updateWorkModel wmsg wmodel
            model |> withWorkModel (wmodel |> Some)
            , Cmd.map Msg.WorkModelMsg wcmd
        | None ->
            model |> withCmdNone

    // --------------------
    // Player, Windows
    // --------------------

    | Msg.PlayerMsg pmsg -> updateOnPlayerMsg logger pmsg model

    | Msg.LooperMsg evt ->
        let (activeTimePoint, cmd, switchTheme) =
            match evt with
            | LooperEvent.TimePointTimeReduced tp -> (tp |> Some, Cmd.none, false)
            | LooperEvent.TimePointStarted (newTp, None) -> (newTp |> Some, Cmd.none, true)
            | LooperEvent.TimePointStarted (nextTp, Some oldTp) ->
                match nextTp.Kind with
                | LongBreak -> (nextTp |> Some, Cmd.ofMsg (WindowsMsg.MinimizeWindows |> Msg.WindowsMsg), true)
                | Break -> (nextTp |> Some, Cmd.ofMsg (WindowsMsg.MinimizeWindows |> Msg.WindowsMsg), true)
                | Work when model |> isUIInitiator oldTp -> (nextTp |> Some, Cmd.batch [ Cmd.ofMsg (WindowsMsg.RestoreWindows |> Msg.WindowsMsg) ], true)
                | Work -> (nextTp |> Some, Cmd.batch [ Cmd.ofMsg (WindowsMsg.RestoreWindows |> Msg.WindowsMsg); Cmd.ofMsg Msg.SendToChatBot ], true)

        let model = model |> setActiveTimePoint activeTimePoint
        model
        , Cmd.batch [
            cmd
            if switchTheme then
                Cmd.OfFunc.attempt cfg.ThemeSwitcher.SwitchTheme (model |> timePointKindEnum) Msg.OnExn
        ]

    | Msg.WindowsMsg wmsg when not model.DisableMinimizeMaximizeWindows -> updateOnWindowsMsg logger wmsg model

    (*
    // --------------------
    // TimePoint Generator
    // --------------------

    | Msg.InitializeTimePointsGeneratorModel ->
        let (genModel, genCmd) = initTimePointGeneratorModel ()
        { model with TimePointsGeneratorModel = genModel |> Some }
        , Cmd.map Msg.TimePointsGeneratorMsg genCmd

    | MsgWith.TimePointsGeneratorMsg model (genMsg, genModel) ->
        let (genModel, genCmd, intent) = updateTimePointGeneratorModel errorMessageQueue genMsg genModel
        let model' = { model with TimePointsGeneratorModel = genModel |> Some }
        let cmd' = Cmd.map TimePointsGeneratorMsg genCmd
        match intent with
        | Intent.Request (TimePointsGeneratorModel.Request.ApplyGeneratedTimePoints) ->
            model', Cmd.batch [cmd'; Cmd.ofMsg (Msg.LoadTimePoints (genModel.TimePoints |> List.map _.TimePoint))]
        | Intent.None ->
            model', cmd'

    | Msg.EraseTimePointsGeneratorModel isDrawerOpen when not isDrawerOpen ->
         {model with TimePointsGeneratorModel = None }, Cmd.none
    *)

    (*
    // --------------------
    // Telegram Bot
    // --------------------

    | Msg.LoadBotSettingsModel ->
        { model with BotSettingsModel = initBotSettingsModel () }
        , Cmd.none

    | MsgWith.BotSettingsMsg model (bmsg, bModel) ->
        let bModel, intent = updateBotSettingsModel bmsg bModel
        match intent with
        | BotSettingsModel.Intent.None ->
            { model with BotSettingsModel = bModel |> Some }, Cmd.none
        | BotSettingsModel.Intent.CloseDialogRequested ->
            { model with BotSettingsModel = None }, Cmd.none

    | Msg.SendToChatBot ->
        let messageText =
            model.ActiveTimePoint |> Option.map (fun tp -> $"It's time to {tp.Name}!!") |> Option.defaultValue "It's time!!"
        model, Cmd.OfTask.attempt cfg.SendToBot messageText Msg.OnExn
    *)

    // --------------------
    // Active time changing
    // --------------------
    | Msg.PreChangeActiveTimeSpan ->
        match model.LooperState with
        | Playing ->
            cfg.Looper.Stop()
            model, Cmd.none
        | _ ->
            { model with LooperState = TimeShiftOnStopped model.LooperState }, Cmd.none

    | Msg.ChangeActiveTimeSpan v ->
        let duration = model |> getActiveTimeDuration
        cfg.Looper.Shift((duration - v) * 1.0<sec>)
        model, Cmd.none

    | Msg.PostChangeActiveTimeSpan ->
        match model.LooperState with
        | TimeShiftOnStopped s ->
            { model with LooperState = s }, Cmd.none
        | _ ->
            cfg.Looper.Resume()
            model, Cmd.none

    // --------------------

    | Msg.AppDialogModelMsg smsg ->
        let (m, cmd) = updateAppDialogModel smsg model.AppDialog
        model |> withAppDialogModel m
        , Cmd.map Msg.AppDialogModelMsg cmd

    // --------------------

    | Msg.OnExn ex ->
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none

    | Msg.OnError err ->
        errorMessageQueue.EnqueueError err
        model, Cmd.none

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none
