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


let storeStartedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (timePoint: TimePoint) =
    task {
        let workEvent =
            match timePoint.Kind with
            | Kind.Break
            | Kind.LongBreak ->
                (time, timePoint.Name) |> WorkEvent.BreakStarted
            | Kind.Work ->
                (time, timePoint.Name) |> WorkEvent.WorkStarted

        let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

        match res with
        | Ok _ -> ()
        | Error err -> raise (InvalidOperationException(err))
    }

let storeStoppedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (_: TimePoint) =
    task {
        let workEvent =
            time |> WorkEvent.Stopped

        let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

        match res with
        | Ok _ -> ()
        | Error err -> raise (InvalidOperationException(err))
    }

let storeWorkReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (offset: TimeSpan) =
    task {
        let workEvent =
            WorkEvent.WorkReduced (time, offset)

        let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

        match res with
        | Ok _ -> ()
        | Error err -> raise (InvalidOperationException(err))
    }

let storeBreakIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (offset: TimeSpan) =
    task {
        let workEvent =
            WorkEvent.BreakIncreased (time, offset)

        let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

        match res with
        | Ok _ -> ()
        | Error err -> raise (InvalidOperationException(err))
    }

let updateOnWindowsMsg (cfg: MainModeConfig) (logger: ILogger<MainModel>) (msg: WindowsMsg) (model: MainModel) =
    match msg with
    | WindowsMsg.MinimizeAllRestoreApp when not model.IsMinimized ->
        { model with IsMinimized = true }
        , Cmd.OfTask.either cfg.WindowsMinimizer.MinimizeAllRestoreAppWindowAsync () (fun _ -> WindowsMsg.SetIsMinimized true |> Msg.WindowsMsg) Msg.OnExn

    | WindowsMsg.RestoreAllMinimized when model.IsMinimized ->
        { model with IsMinimized = false }
        , Cmd.OfFunc.either cfg.WindowsMinimizer.RestoreAllMinimized () (fun _ -> WindowsMsg.SetIsMinimized false |> Msg.WindowsMsg) Msg.OnExn

    | WindowsMsg.RestoreAppWindow ->
        model, Cmd.OfFunc.attempt cfg.WindowsMinimizer.RestoreAppWindow () Msg.OnExn

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
        storeStartedWorkEventTask cfg.WorkEventRepository

    let storeStoppedWorkEventTask =
        storeStoppedWorkEventTask cfg.WorkEventRepository

    let storeWorkReducedEventTask =
        storeWorkReducedEventTask cfg.WorkEventRepository

    let storeBreakIncreasedEventTask =
        storeBreakIncreasedEventTask cfg.WorkEventRepository

    match msg with
    | ControllerMsg.Next ->
        cfg.Looper.Next()
        model |> withLooperState Playing |> setLastAtpWhenLooperNextWasCalled, Cmd.none

    | ControllerMsg.Play ->
        cfg.Looper.Next()
        model |> withLooperState Playing |> setLastAtpWhenLooperNextWasCalled, Cmd.none

    | ControllerMsg.Resume when model.ActiveTimePoint |> Option.isSome && model.LooperState = LooperState.Stopped ->
        let time = cfg.TimeProvider.GetUtcNow()
        cfg.Looper.Resume() // next looper event is TimePointReduced

        let model = model |> withLooperState Playing

        model
        , Cmd.batch [
            match model.CurrentWork with
            | Some work ->
                Cmd.OfTask.attempt (storeStartedWorkEventTask work.Work.Id time) model.ActiveTimePoint.Value Msg.OnExn
            | _ ->
                Cmd.none

            match model.ActiveTimePoint.Value with
            | { Kind = Kind.Break } when not model.IsMinimized ->
                Cmd.ofMsg (WindowsMsg.MinimizeAllRestoreApp |> Msg.WindowsMsg)
            | { Kind = Kind.Work } when model.IsMinimized ->
                Cmd.ofMsg (WindowsMsg.RestoreAllMinimized |> Msg.WindowsMsg)
            | _ ->
                Cmd.none
        ]

    | ControllerMsg.Stop when model.LooperState = LooperState.Playing ->
        cfg.Looper.Stop()
        let time = cfg.TimeProvider.GetUtcNow()
        model |> withLooperState Stopped
        , Cmd.batch [
            Cmd.ofMsg (WindowsMsg.RestoreAllMinimized |> Msg.WindowsMsg)
            Cmd.ofMsg (WindowsMsg.RestoreAppWindow |> Msg.WindowsMsg)

            match model.CurrentWork, model.ActiveTimePoint with
            | Some work, Some tp ->
                Cmd.OfTask.attempt (storeStoppedWorkEventTask work.Work.Id time) tp Msg.OnExn
            | _ ->
                Cmd.none
        ]

    | ControllerMsg.Replay when model.ActiveTimePoint |> Option.isSome ->
        let cmd =
            Cmd.batch [
                Cmd.ofMsg (ControllerMsg.PreChangeActiveTimeSpan |> Msg.ControllerMsg);
                Cmd.ofMsg (ControllerMsg.ChangeActiveTimeSpan 0.0 |> Msg.ControllerMsg);
                Cmd.ofMsg (ControllerMsg.PostChangeActiveTimeSpan |> Msg.ControllerMsg);
                if model.LooperState = LooperState.Stopped then
                    Cmd.ofMsg (ControllerMsg.Resume |> Msg.ControllerMsg);
            ]
        model, cmd

    | ControllerMsg.LooperMsg evt ->
        match evt with
        | LooperEvent.TimePointTimeReduced tp ->
            model
            |> withActiveTimePoint (tp |> Some)
            |> withNoneLastAtpWhenLooperNextIsCalled
            , Cmd.none

        | LooperEvent.TimePointStarted (nextTp, oldTp) ->
            let time = cfg.TimeProvider.GetUtcNow()
            let model =
                model
                |> withActiveTimePoint (nextTp |> Some)
                |> withNoneLastAtpWhenLooperNextIsCalled

            let switchThemeCmd = Cmd.OfFunc.attempt cfg.ThemeSwitcher.SwitchTheme (model |> timePointKindEnum) Msg.OnExn

            let storeStartedWorkEventCmd =
                match model.CurrentWork, model.LooperState with
                | Some work, LooperState.Playing ->
                    Cmd.OfTask.attempt (storeStartedWorkEventTask work.Work.Id time) nextTp Msg.OnExn
                | _ ->
                    Cmd.none

            match nextTp.Kind with
            | LongBreak
            | Break when model.LooperState <> LooperState.Playing ->
                model, switchThemeCmd

            | LongBreak
            | Break when oldTp |> Option.isSome && (oldTp.Value.Kind = Kind.Break || oldTp.Value.Kind = Kind.LongBreak) ->
                model
                , Cmd.batch [
                    storeStartedWorkEventCmd
                ]

            | LongBreak
            | Break ->
                model
                , Cmd.batch [
                    switchThemeCmd;
                    storeStartedWorkEventCmd
                    Cmd.ofMsg (WindowsMsg.MinimizeAllRestoreApp |> Msg.WindowsMsg)
                ]

            // initialized
            | Work when model.LooperState <> LooperState.Playing ->
                model, switchThemeCmd

            // do not send a notification if the user manually presses the Next button or the Play button
            | Work when isLastAtpWhenLooperNextWasCalled oldTp model || oldTp |> Option.isNone ->
                model
                , Cmd.batch [
                    switchThemeCmd;
                    storeStartedWorkEventCmd
                    Cmd.ofMsg (WindowsMsg.RestoreAllMinimized |> Msg.WindowsMsg)
                ]

            | Work when oldTp |> Option.isSome && (oldTp.Value.Kind = Kind.Work) ->
                model
                , Cmd.batch [
                    storeStartedWorkEventCmd
                    Cmd.ofMsg (Msg.SendToChatBot $"It's time to {nextTp.Name}!!")
                ]

            | Work ->
                model
                , Cmd.batch [
                    switchThemeCmd;
                    storeStartedWorkEventCmd
                    Cmd.ofMsg (WindowsMsg.RestoreAllMinimized |> Msg.WindowsMsg);
                    Cmd.ofMsg (Msg.SendToChatBot $"It's time to {nextTp.Name}!!")
                ]

    // --------------------
    // Active time changing
    // --------------------
    | ControllerMsg.PreChangeActiveTimeSpan when model.ActiveTimePoint |> Option.isSome ->

        match model.LooperState with
        | Playing ->
            cfg.Looper.Stop()
            let time = cfg.TimeProvider.GetUtcNow()
            model |> withPreShiftActiveTimePointTimeSpan
            , Cmd.batch [
                if model.CurrentWork |> Option.isSome then
                    Cmd.OfTask.attempt (storeStoppedWorkEventTask model.CurrentWork.Value.Id time) model.ActiveTimePoint.Value Msg.OnExn
            ]
        | _ ->
            { model with LooperState = TimeShiftingAfterNotPlaying model.LooperState }, Cmd.none

    | ControllerMsg.ChangeActiveTimeSpan v when model.ActiveTimePoint |> Option.isSome ->
        let duration = model |> getActiveTimeDuration
        let shiftTime = (duration - v) * 1.0<sec>
        cfg.Looper.Shift(shiftTime)

        model |> withShiftTime shiftTime |> withCmdNone

    | ControllerMsg.PostChangeActiveTimeSpan when model.ActiveTimePoint |> Option.isSome ->
        // TODO: check model.ShiftTime and ActiveTimePoint
        match model.LooperState with
        | TimeShiftingAfterNotPlaying s ->
            model |> withLooperState s |> withoutShiftAndPreShiftTimes |> withCmdNone
        | _ ->
            let time = cfg.TimeProvider.GetUtcNow()
            cfg.Looper.Resume()

            let atp = model.ActiveTimePoint.Value
            let rollbackWorkStrategy = cfg.UserSettings.RollbackWorkStrategy

            let cmds = [
                if model.CurrentWork |> Option.isSome then
                    Cmd.OfTask.attempt (storeStartedWorkEventTask model.CurrentWork.Value.Id time) atp Msg.OnExn
            ]

            if atp.Kind = Kind.Work && model.NewActiveTimeSpan > model.PreShiftActiveTimeSpan && model.CurrentWork |> Option.isSome then
                let diff = TimeSpan.FromSeconds(float (model.NewActiveTimeSpan - model.PreShiftActiveTimeSpan))
                match rollbackWorkStrategy with
                | RollbackWorkStrategy.SubstractWorkAddBreak ->
                    model |> withoutShiftAndPreShiftTimes
                    , Cmd.batch [
                        Cmd.OfTask.attempt (storeWorkReducedEventTask model.CurrentWork.Value.Id (time.AddMilliseconds(-2))) diff Msg.OnExn
                        Cmd.OfTask.attempt (storeBreakIncreasedEventTask model.CurrentWork.Value.Id (time.AddMilliseconds(-1))) diff Msg.OnExn
                        yield! cmds
                    ]
                | RollbackWorkStrategy.UserChoiceIsRequired ->
                    model |> withoutShiftAndPreShiftTimes
                    , Cmd.batch [
                        Cmd.ofMsg (Msg.AppDialogModelMsg (AppDialogModel.Msg.LoadRollbackWorkDialogModel (model.CurrentWork.Value.Id, time, diff)))
                        yield! cmds
                    ]
                | RollbackWorkStrategy.Default ->
                    model |> withoutShiftAndPreShiftTimes
                    , Cmd.batch cmds

            else
                model |> withoutShiftAndPreShiftTimes
                , Cmd.batch cmds
    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none


let update
    (cfg: MainModeConfig)
    updateWorkModel
    updateAppDialogModel
    updateWorkSelectorModel
    initWorkStatisticListModel
    updateWorkStatisticListModel
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<MainModel>)
    (msg: Msg)
    (model: MainModel)
    =
    let updateOnPlayerMsg = updateOnPlayerMsg cfg
    let updateOnWindowsMsg = updateOnWindowsMsg cfg

    let storeStartedWorkEventTask =
        storeStartedWorkEventTask cfg.WorkEventRepository

    let storeStoppedWorkEventTask =
        storeStoppedWorkEventTask cfg.WorkEventRepository

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
    | Msg.SetIsTimePointsShown v ->
        if v then
            model |> withoutWorkSelectorModel |> withIsTimePointsShown v |> withCmdNone
        else
            model |> withIsTimePointsShown v |> withCmdNone

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
        model
        , Cmd.batch [
            Cmd.ofMsg (ControllerMsg.Stop |> Msg.ControllerMsg);
            Cmd.OfFunc.either cfg.TimePointQueue.ScrollTo id (Operation.Finish >> Msg.StartTimePoint) Msg.OnExn
        ]

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

    | Msg.SetIsWorkSelectorLoaded v ->
        if v then
            let (m, cmd) = WorkSelectorModel.init (model.CurrentWork |> Option.map (_.Work >> _.Id))
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

            if model.LooperState = LooperState.Playing then
                let time = cfg.TimeProvider.GetUtcNow()
                match model.CurrentWork with
                | Some currWork when currWork.Id <> workModel.Id ->
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel (workModel |> Some)
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt (storeStoppedWorkEventTask currWork.Id time) model.ActiveTimePoint.Value Msg.OnExn
                        Cmd.OfTask.attempt (storeStartedWorkEventTask workModel.Id time) model.ActiveTimePoint.Value Msg.OnExn
                    ]
                | Some _ ->
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel (workModel |> Some)
                    , cmd
                | None ->
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel (workModel |> Some)
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt (storeStartedWorkEventTask workModel.Id time) model.ActiveTimePoint.Value Msg.OnExn
                    ]
            else
                model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel (workModel |> Some)
                , cmd


        | WorkSelectorModel.Intent.UnselectCurrentWork ->
            cfg.CurrentWorkItemSettings.CurrentWork <- None

            if model.LooperState = LooperState.Playing then
                match model.CurrentWork with
                | Some currWork ->
                    let time = cfg.TimeProvider.GetUtcNow()
                    model |> withWorkSelectorModel (workSelectorModel |> Some) |> withWorkModel None
                    , Cmd.batch [
                        cmd
                        Cmd.OfTask.attempt (storeStoppedWorkEventTask currWork.Id time) model.ActiveTimePoint.Value Msg.OnExn
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

    | Msg.ControllerMsg pmsg -> updateOnPlayerMsg logger pmsg model

    | Msg.WindowsMsg wmsg when not model.DisableMinimizeMaximizeWindows -> updateOnWindowsMsg logger wmsg model

    | Msg.SetIsWorkStatisticShown v ->
        if v then
            let (m, cmd) = initWorkStatisticListModel ()
            model |> MainModel.withWorkStatistic (m |> Some)
            , Cmd.map Msg.WorkStatisticListModelMsg cmd
        else
            model |> MainModel.withWorkStatistic None
            , Cmd.none


    | MsgWith.WorkStatisticListModelMsg model (smsg, sm) ->
        let (m, cmd, intent) = updateWorkStatisticListModel smsg sm
        match intent with
        | WorkStatisticListModel.Intent.None ->
            model |> MainModel.withWorkStatistic (m |> Some)
            , Cmd.map Msg.WorkStatisticListModelMsg cmd
        | WorkStatisticListModel.Intent.CloseDialogRequested ->
            model |> MainModel.withWorkStatistic None
            , Cmd.none

    // --------------------
    
    | Msg.SendToChatBot message ->
        model, Cmd.OfTask.attempt cfg.SendToBot.SendMessage message Msg.OnExn

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
