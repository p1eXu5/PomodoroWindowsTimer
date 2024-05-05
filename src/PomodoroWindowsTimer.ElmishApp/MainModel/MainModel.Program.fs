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
open PomodoroWindowsTimer.Abstractions
open System


let storeStartedWorkEventTask (timeProvider: System.TimeProvider) (workEventRepository: IWorkEventRepository) (workId: uint64) (timePoint: TimePoint) =
    task {
        let workEvent =
            match timePoint.Kind with
            | Kind.Break
            | Kind.LongBreak ->
                (timeProvider.GetUtcNow(), timePoint.Name) |> WorkEvent.BreakStarted
            | Kind.Work ->
                (timeProvider.GetUtcNow(), timePoint.Name) |> WorkEvent.WorkStarted

        let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

        match res with
        | Ok _ -> ()
        | Error err -> raise (InvalidOperationException(err))
    }

let storeStoppedWorkEventTask (timeProvider: System.TimeProvider) (workEventRepository: IWorkEventRepository) (workId: uint64) (timePoint: TimePoint) =
    task {
        let workEvent =
            (timeProvider.GetUtcNow()) |> WorkEvent.Stopped

        let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

        match res with
        | Ok _ -> ()
        | Error err -> raise (InvalidOperationException(err))
    }

let updateOnWindowsMsg (cfg: MainModeConfig) (logger: ILogger<MainModel>) (msg: WindowsMsg) (model: MainModel) =
    match msg with
    | WindowsMsg.MinimizeWindows when not model.IsMinimized ->
        { model with IsMinimized = true }, Cmd.OfTask.either cfg.WindowsMinimizer.MinimizeOtherAsync () (fun _ -> WindowsMsg.SetIsMinimized true |> Msg.WindowsMsg) Msg.OnExn

    | WindowsMsg.RestoreWindows when model.IsMinimized ->
        { model with IsMinimized = false }, Cmd.OfFunc.either cfg.WindowsMinimizer.Restore () (fun _ -> WindowsMsg.SetIsMinimized false |> Msg.WindowsMsg) Msg.OnExn

    | WindowsMsg.RestoreMainWindow ->
        model, Cmd.OfFunc.attempt cfg.WindowsMinimizer.RestoreMainWindow () Msg.OnExn

    | WindowsMsg.SetIsMinimized v ->
        { model with IsMinimized = v }, Cmd.none

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none


let updateOnPlayerMsg
    (cfg: MainModeConfig)
    (logger: ILogger<MainModel>)
    (msg: ControllerMsg)
    (model: MainModel)
    =
    let storeStartedWorkEventTask =
        storeStartedWorkEventTask cfg.TimeProvider cfg.WorkEventRepository

    let storeStoppedWorkEventTask =
        storeStoppedWorkEventTask cfg.TimeProvider cfg.WorkEventRepository

    match msg with
    | ControllerMsg.Next ->
        cfg.Looper.Next()
        model |> setLooperState Playing |> setLastAtpWhenLooperNextWasCalled, Cmd.none

    | ControllerMsg.Play ->
        cfg.Looper.Next()
        model |> setLooperState Playing |> setLastAtpWhenLooperNextWasCalled, Cmd.none

    | ControllerMsg.Resume when model.ActiveTimePoint |> Option.isSome ->
        cfg.Looper.Resume()
        let model' = model |> setLooperState Playing

        match model.ActiveTimePoint with
        | Some ({ Kind = Kind.Break }) when not model.IsMinimized ->
            model', Cmd.ofMsg (WindowsMsg.MinimizeWindows |> Msg.WindowsMsg)

        | Some ({ Kind = Kind.Work }) when model.IsMinimized ->
            model', Cmd.ofMsg (WindowsMsg.RestoreWindows |> Msg.WindowsMsg)

        | _ ->
            model', Cmd.none

    | ControllerMsg.Stop when model.LooperState = LooperState.Playing ->
        cfg.Looper.Stop()
        model |> setLooperState Stopped
        , Cmd.batch [
            Cmd.ofMsg (WindowsMsg.RestoreWindows |> Msg.WindowsMsg)
            Cmd.ofMsg (WindowsMsg.RestoreMainWindow |> Msg.WindowsMsg)

            match model.CurrentWork, model.ActiveTimePoint with
            | Some work, Some tp ->
                Cmd.OfTask.attempt (storeStoppedWorkEventTask work.Work.Id) tp Msg.OnExn
            | _ ->
                Cmd.none
        ]

    | ControllerMsg.Replay when model.ActiveTimePoint |> Option.isSome ->
        let cmd =
            Cmd.batch [Cmd.ofMsg (ControllerMsg.PreChangeActiveTimeSpan |> Msg.ControllerMsg); Cmd.ofMsg (ControllerMsg.ChangeActiveTimeSpan 0.0 |> Msg.ControllerMsg); Cmd.ofMsg (ControllerMsg.Resume |> Msg.ControllerMsg)]
        model, cmd

    | ControllerMsg.LooperMsg evt ->
        match evt with
        | LooperEvent.TimePointTimeReduced tp ->
            model
            |> withActiveTimePoint (tp |> Some)
            |> withNoneLastAtpWhenLooperNextIsCalled
            , Cmd.none

        | LooperEvent.TimePointStarted (nextTp, oldTp) ->
            let model =
                model
                |> withActiveTimePoint (nextTp |> Some)
                |> withNoneLastAtpWhenLooperNextIsCalled

            let switchThemeCmd = Cmd.OfFunc.attempt cfg.ThemeSwitcher.SwitchTheme (model |> timePointKindEnum) Msg.OnExn

            let startedWorkEventCmd =
                match model.CurrentWork, model.LooperState with
                | Some work, LooperState.Playing ->
                    Cmd.OfTask.attempt (storeStartedWorkEventTask work.Work.Id) nextTp Msg.OnExn
                | _ ->
                    Cmd.none

            match nextTp.Kind with
            | LongBreak
            | Break when model.LooperState <> LooperState.Playing ->
                model, switchThemeCmd

            | LongBreak
            | Break ->
                model
                , Cmd.batch [
                    switchThemeCmd;
                    startedWorkEventCmd
                    Cmd.ofMsg (WindowsMsg.MinimizeWindows |> Msg.WindowsMsg)
                ]

            // initialized
            | Work when model.LooperState <> LooperState.Playing ->
                model, switchThemeCmd

            // do not send a notification if the user manually presses the Next button or the Play button
            | Work when isLastAtpWhenLooperNextWasCalled oldTp model || oldTp |> Option.isNone ->
                model
                , Cmd.batch [
                    switchThemeCmd;
                    startedWorkEventCmd
                    Cmd.ofMsg (WindowsMsg.RestoreWindows |> Msg.WindowsMsg)
                ]

            | Work ->
                model
                , Cmd.batch [
                    switchThemeCmd;
                    startedWorkEventCmd
                    Cmd.ofMsg (WindowsMsg.RestoreWindows |> Msg.WindowsMsg);
                    Cmd.ofMsg (Msg.SendToChatBot $"It's time to {nextTp.Name}!!")
                ]

    // --------------------
    // Active time changing
    // --------------------
    | ControllerMsg.PreChangeActiveTimeSpan ->
        match model.LooperState with
        | Playing ->
            cfg.Looper.Stop()
            model, Cmd.none
        | _ ->
            { model with LooperState = TimeShiftingAfterNotPlaying model.LooperState }, Cmd.none

    | ControllerMsg.ChangeActiveTimeSpan v ->
        let duration = model |> getActiveTimeDuration
        cfg.Looper.Shift((duration - v) * 1.0<sec>)
        model, Cmd.none

    | ControllerMsg.PostChangeActiveTimeSpan ->
        match model.LooperState with
        | TimeShiftingAfterNotPlaying s ->
            { model with LooperState = s }, Cmd.none
        | _ ->
            cfg.Looper.Resume()
            model, Cmd.none


    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none


let update
    (cfg: MainModeConfig)
    updateWorkModel
    updateAppDialogModel
    updateWorkSelectorModel
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

    | Msg.StartTimePoint (Operation.Start id) ->
        model, Cmd.batch [ Cmd.ofMsg (ControllerMsg.Stop |> Msg.ControllerMsg); Cmd.OfFunc.either cfg.Looper.TimePointQueue.ScrollTo id (Operation.Finish >> Msg.StartTimePoint) Msg.OnExn ]

    | Msg.StartTimePoint (Operation.Finish _) ->
        model, Cmd.ofMsg (ControllerMsg.Play |> Msg.ControllerMsg)

    // --------------------
    // Work
    // --------------------
    | Msg.LoadCurrentWork ->
        match cfg.CurrentWorkItemSettings.CurrentWork with
        | None -> model, Cmd.none
        | Some work ->
            model, Cmd.OfTask.perform (cfg.WorkRepository.FindByIdOrCreateAsync work) CancellationToken.None Msg.SetCurrentWorkIfNone

    | Msg.SetCurrentWorkIfNone res ->
        match res with
        | Ok work when model.CurrentWork |> Option.isNone ->
            { model with CurrentWork = work |> WorkModel.init |> Some}, Cmd.none
        | Error err -> model, Cmd.ofMsg (Msg.OnError err)
        | _ -> model, Cmd.none

    | Msg.WorkModelMsg wmsg ->
        match model.CurrentWork with
        | Some wmodel ->
            let (wmodel, wcmd, _) = updateWorkModel wmsg wmodel
            model |> withWorkModel (wmodel |> Some)
            , Cmd.map Msg.WorkModelMsg wcmd
        | None ->
            model |> withCmdNone

    // --------------------
    // Player, Windows
    // --------------------

    | Msg.ControllerMsg pmsg -> updateOnPlayerMsg logger pmsg model

    

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
    *)

    | Msg.SendToChatBot message ->
        model, Cmd.OfTask.attempt cfg.SendToBot.SendMessage message Msg.OnExn
    
    // --------------------

    | Msg.AppDialogModelMsg smsg ->
        let (m, cmd) = updateAppDialogModel smsg model.AppDialog
        model |> withAppDialogModel m
        , Cmd.map Msg.AppDialogModelMsg cmd

    | Msg.SetIsWorkSelectorLoaded v ->
        if v then
            let (m, cmd) = WorkSelectorModel.init (model.CurrentWork |> Option.map (_.Work >> _.Id))
            model |> withWorkSelectorModel (m |> Some) |> withIsTimePointsShown false
            , Cmd.map Msg.WorkSelectorModelMsg cmd
        else
            model |> withoutWorkSelectorModel |> withCmdNone

    | Msg.SetIsTimePointsShown v ->
        if v then
            model |> withoutWorkSelectorModel |> withIsTimePointsShown v |> withCmdNone
        else
            model |> withIsTimePointsShown v |> withCmdNone

    | MsgWith.WorkSelectorModelMsg model (smsg, m) ->
        let (m, cmd, intent) = updateWorkSelectorModel smsg m
        match intent with
        | WorkSelectorModel.Intent.None ->
            model |> withWorkSelectorModel (m |> Some)
            , Cmd.map Msg.WorkSelectorModelMsg cmd
        | WorkSelectorModel.Intent.Select workModel ->
            cfg.CurrentWorkItemSettings.CurrentWork <- workModel |> Option.map _.Work
            model |> withWorkSelectorModel (m |> Some) |> withWorkModel workModel
            , Cmd.map Msg.WorkSelectorModelMsg cmd
        | WorkSelectorModel.Intent.Close ->
            model |> withWorkSelectorModel None |> withCmdNone


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
