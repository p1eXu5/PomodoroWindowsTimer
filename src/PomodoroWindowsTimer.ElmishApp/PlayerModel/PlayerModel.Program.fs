module PomodoroWindowsTimer.ElmishApp.PlayerModel.Program

open System
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.PlayerModel

let inline private withNoCmdAndIntent m = (m, Cmd.none, Intent.None)

let inline private withNoIntent (m, cmd) = (m, cmd, Intent.None)

let inline private minimizeAllRestoreAppWindowCmd (windowsMinimizer: IWindowsMinimizer) =
    Cmd.OfFunc.attempt windowsMinimizer.MinimizeAllRestoreAppWindowAsync () Msg.OnExn

let inline private restoreAppWindowCmd (windowsMinimizer: IWindowsMinimizer) =
    Cmd.OfFunc.attempt windowsMinimizer.RestoreAppWindow () Msg.OnExn

let inline private restoreAllMinimizedCmd (windowsMinimizer: IWindowsMinimizer) =
    Cmd.OfFunc.attempt windowsMinimizer.RestoreAllMinimized () Msg.OnExn

let inline private switchThemeCmd (themeSwitcher: IThemeSwitcher) timePointKind =
    Cmd.OfFunc.attempt themeSwitcher.SwitchTheme timePointKind Msg.OnExn


/// Msg.SetDisableSkipBreak handler.
let private setDisableSkipBreak (settings: IPlayerUserSettings) v (model: PlayerModel) =
    model |> withDisableSkipBreak v
    , Cmd.OfFunc.attempt (fun() -> settings.DisableSkipBreak <- v) () Msg.OnExn
    , Intent.None


/// Msg.StartTimePoint handler.
///
/// Starts ITimePointQueue.ScrollTo operation and finishes it with Msg.Play.
let private startTimePoint (timePointQueue: ITimePointQueue) op (model: PlayerModel) =
    match op with
    | Operation.Start timePointId ->
        model
        , Cmd.batch [
            Cmd.ofMsg Msg.Stop
            Cmd.OfFunc.either
                (fun tpId ->
                    if timePointQueue.ScrollTo tpId then
                        () |> Operation.Finish |> Msg.StartTimePoint
                    else
                        Msg.OnError $"Time point {tpId} has not been found in ITimePointQueue"
                )
                timePointId
                id
                Msg.OnExn
        ]
        , Intent.None

    | Operation.Finish _ ->
        model, Cmd.ofMsg Msg.Play, Intent.None


/// Msg.Play and Msg.Next handler.
let private next (looper: ILooper) (model: PlayerModel) =
    looper.Next()
    model |> withLooperState Playing |> withLastAtpWhenPlayOrNextIsManuallyPressed
    , Cmd.none
    , Intent.None


/// Msg.Resume handler.
let private resume (looper: ILooper) (windowsMinimizer: IWindowsMinimizer) (model: PlayerModel) =
    looper.Resume() // next looper event is TimePointStarted

    let winCmd =
        if model.DisableMinimizeMaximizeWindows then Cmd.none
        else
            let isMinimized = windowsMinimizer.GetIsMinimized()
            match model.ActiveTimePoint.Value with
            | { Kind = Kind.Break } when not isMinimized -> windowsMinimizer |> minimizeAllRestoreAppWindowCmd
            | { Kind = Kind.Work } when isMinimized -> windowsMinimizer |> restoreAllMinimizedCmd
            | _ -> Cmd.none

    model |> withLooperState Playing
    , winCmd
    , Intent.None


/// Msg.Stop handler.
let private stop (looper: ILooper) (windowsMinimizer: IWindowsMinimizer) (model: PlayerModel) =
    looper.Stop()

    let winCmd =
        if model.DisableMinimizeMaximizeWindows || (not <| windowsMinimizer.GetIsMinimized()) then Cmd.none
        else windowsMinimizer |> restoreAllMinimizedCmd

    model |> withLooperState Stopped
    , winCmd
    , Intent.None


/// Msg.Replay handler.
let private replay (model: PlayerModel) =
    let cmd =
        Cmd.batch [
            Cmd.ofMsg (Msg.PreChangeActiveTimeSpan);
            Cmd.ofMsg (Msg.ChangeActiveTimeSpan 0.0);
            Cmd.ofMsg (AsyncOperation.startUnit Msg.PostChangeActiveTimeSpan);
            if model.LooperState = LooperState.Stopped then
                Cmd.ofMsg (Msg.Resume);
        ]
    model, cmd, Intent.None


/// Msg.LooperMsg handler.
let private mapLooperEvent (windowsMinimizer: IWindowsMinimizer) (themeSwitcher: IThemeSwitcher) levt (model: PlayerModel) =
    match levt with
    | LooperEvent.TimePointTimeReduced ({ ActiveTimePoint = atp }, _) ->
        model
        |> withActiveTimePoint (atp |> Some)
        |> withNoneLastAtpWhenPlayOrNextIsManuallyPressed
        |> withLooperState LooperState.Playing
        , Cmd.none, Intent.None

    | LooperEvent.TimePointStarted ({NewActiveTimePoint = nextTp; OldActiveTimePoint = oldTp }, _) ->
        let nextTimePointKind = nextTp |> TimePointKind.ofActiveTimePoint

        let cmd =
            match nextTimePointKind with
            | TimePointKind.Break when model.LooperState <> LooperState.Playing ->
                switchThemeCmd themeSwitcher nextTimePointKind

            //| LongBreak | Break when oldTp |> Option.isSome && (oldTp.Value.Kind = Kind.Break || oldTp.Value.Kind = Kind.LongBreak) ->
            //    storeStartedWorkEventOrActiveTimePointCmd

            | TimePointKind.Break ->
                Cmd.batch [
                    switchThemeCmd themeSwitcher nextTimePointKind
                    minimizeAllRestoreAppWindowCmd windowsMinimizer
                ]

            // initialized, not playing, just switch theme
            | TimePointKind.Work when model.LooperState <> LooperState.Playing ->
                switchThemeCmd themeSwitcher nextTimePointKind

            // do not send a message to the Telegram if the user manually presses the Next button or the Play button
            | TimePointKind.Work when oldTp |> Option.isNone || isLastAtpWhenPlayOrNextIsManuallyPressed oldTp model ->
                Cmd.batch [
                    switchThemeCmd themeSwitcher nextTimePointKind
                    restoreAllMinimizedCmd windowsMinimizer
                ]

            //| Work when oldTp |> Option.isSome && (oldTp.Value.Kind = Kind.Work) ->
            //    Cmd.batch [
            //        sendToChatBotCmd
            //    ]

            | TimePointKind.Work ->
                Cmd.batch [
                    switchThemeCmd themeSwitcher nextTimePointKind
                    restoreAllMinimizedCmd windowsMinimizer
                ]

            | _ -> Cmd.none

        model
        |> withActiveTimePoint (nextTp |> Some)
        |> withNoneLastAtpWhenPlayOrNextIsManuallyPressed
        |> withLooperState (if oldTp.IsNone then LooperState.Stopped else LooperState.Playing)
        , cmd
        , Intent.None

    | LooperEvent.TimePointStopped (atp, _) ->
        model
        |> withActiveTimePoint (atp |> Some)
        |> withNoneLastAtpWhenPlayOrNextIsManuallyPressed
        |> withLooperState LooperState.Stopped
        , Cmd.none, Intent.None


/// Msg.PreChangeActiveTimeSpan handler.
let private preChangeActiveTimeSpan (looper: ILooper) (model: PlayerModel) =
    if model.LooperState = Playing then
        looper.Stop()

    model |> withPreShiftState, Cmd.none, Intent.None


/// Msg.ChangeActiveTimeSpan handler.
let private changeActiveTimeSpan (looper: ILooper) (offsetInSeconds: float) (model: PlayerModel) =
    let duration = model |> getActiveTimeDuration
    let remainingSeconds = (duration - offsetInSeconds) * 1.0<sec>
    looper.Shift(remainingSeconds)

    model |> withNewActiveRemainingSeconds remainingSeconds
    , Cmd.none, Intent.None


/// MsgWith.``Start of PostChangeActiveTimeSpan`` handler.
let private startOfPostChangeActiveTimeSpan
    (looper: ILooper)
    (timeProvider: TimeProvider)
    (workEventStore: WorkEventStore)
    (shiftTimes: ShiftAndPreShiftTimes)
    (atp: ActiveTimePoint)
    (deff: AsyncDeferredState)
    (cts: Cts)
    (model: PlayerModel) =

    let prevState =
        match model.LooperState with
        | LooperState.TimeShifting prevState ->
            match prevState with
            | LooperState.Playing ->
                looper.Resume()
                prevState
            | _ ->
                prevState
        | _ -> model.LooperState

    // no shifting
    if Math.Abs(float (shiftTimes.NewActiveRemainingSeconds - shiftTimes.PreShiftActiveRemainingSeconds)) < 1.0 then
        model
        |> withLooperState prevState
        |> withoutShiftAndPreShiftTimes
        |> withNotRequestedRetrieveWorkSpentTimesState
        , Cmd.none
        , Intent.None

    // shifting forward
    elif shiftTimes.NewActiveRemainingSeconds < shiftTimes.PreShiftActiveRemainingSeconds then
        model
        |> withLooperState prevState
        |> withoutShiftAndPreShiftTimes
        |> withNotRequestedRetrieveWorkSpentTimesState
        , Cmd.none
        , Intent.SkipOrApplyMissingTime (
            atp.Kind,
            atp.Id,
            TimeSpan.FromSeconds(float (shiftTimes.PreShiftActiveRemainingSeconds - shiftTimes.NewActiveRemainingSeconds)),
            timeProvider.GetUtcNow()
        )
    // shifting backward
    else
        model
        |> withLooperState prevState
        |> withoutShiftAndPreShiftTimes
        |> withRetreiveWorkSpentTimesState deff
        , Cmd.batch [
            Cmd.OfTask.perform
                workEventStore.WorkSpentTimeListTask
                (
                    atp.Id,
                    atp.Kind,
                    timeProvider.GetUtcNow(),
                    (shiftTimes.NewActiveRemainingSeconds - shiftTimes.PreShiftActiveRemainingSeconds),
                    cts.Token
                )
                (AsyncOperation.finishWithin Msg.PostChangeActiveTimeSpan cts)
        ]
        , Intent.None


/// MsgWith.``Finish of PostChangeActiveTimeSpan`` handler.
let private finishOfPostChangeActiveTimeSpan
    (timeProvider: TimeProvider)
    (workSpentTimeList: WorkSpentTime list)
    (atp: ActiveTimePoint)
    (model: PlayerModel)
    =
    match workSpentTimeList with
    | [] ->
        model
        |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
        , Cmd.none
        , Intent.None

    | [ workSpentTime ] ->
        model
        |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
        , Cmd.none
        , Intent.RollbackTime (workSpentTime, atp.Kind, atp.Id, timeProvider.GetUtcNow())

    | _ ->
        model
        |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
        , Cmd.none
        , Intent.MultipleRollbackTime (workSpentTimeList, atp.Kind, atp.Id, timeProvider.GetUtcNow())
        (*
        // TODO
        match workSpentTimeList with
        | [] -> model |> withUpdatedModel Cmd.none Intent.None
        | _ ->
            model |> withUpdatedModel Cmd.none Intent.None

        // TODO: check model.ShiftTime and ActiveTimePoint
        match model.LooperState with
        | TimeShifting s ->
            model |> withLooperState s |> withoutShiftAndPreShiftTimes |> withCmdNone |> withNoIntent
        | _ ->
            let time = timeProvider.GetUtcNow()
            looper.Resume()

            let atp = model.ActiveTimePoint.Value
            let rollbackWorkStrategy = userSettings.RollbackWorkStrategy

            let cmds = [
                if currentWork |> Option.isSome then
                    Cmd.OfTask.attempt (workEventStore.StoreStartedWorkEventTask currentWork.Value.Id time) atp Msg.OnExn
            ]

            if atp.Kind = Kind.Work && model.NewActiveRemainingSeconds > model.PreShiftActiveRemainingSeconds && currentWork |> Option.isSome then
                let diff = TimeSpan.FromSeconds(float (model.NewActiveRemainingSeconds - model.PreShiftActiveRemainingSeconds))
                match rollbackWorkStrategy with
                | RollbackWorkStrategy.SubstractWorkAddBreak ->
                    model |> withoutShiftAndPreShiftTimes
                    , Cmd.batch [
                        Cmd.OfTask.attempt (workEventStore.StoreWorkReducedEventTask currentWork.Value.Id (time.AddMilliseconds(-2))) diff Msg.OnExn
                        Cmd.OfTask.attempt (workEventStore.StoreBreakIncreasedEventTask currentWork.Value.Id (time.AddMilliseconds(-1))) diff Msg.OnExn
                        yield! cmds
                    ]
                    , Intent.None

                | RollbackWorkStrategy.UserChoiceIsRequired ->
                    model |> withoutShiftAndPreShiftTimes
                    , Cmd.batch cmds
                    , Intent.ShowRollbackDialog (currentWork.Value.Id, time, diff)

                | RollbackWorkStrategy.Default ->
                    model |> withoutShiftAndPreShiftTimes
                    , Cmd.batch cmds
                    , Intent.None
            else
                model |> withoutShiftAndPreShiftTimes
                , Cmd.batch cmds
                , Intent.None
        *)

/// PlayerModel update function.
let update
    (looper: ILooper)
    (windowsMinimizer: IWindowsMinimizer)
    (timeProvider: System.TimeProvider)
    (workEventStore: WorkEventStore)
    (themeSwitcher: IThemeSwitcher)
    (playerUserSettings: IPlayerUserSettings)
    (timePointQueue: ITimePointQueue)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<PlayerModel>)
    msg model
    =
    match msg with
    // ------------------
    // Settings
    // ------------------
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

    // ------------------

    | Msg.StartTimePoint op ->
        model |> startTimePoint timePointQueue op

    | Msg.Next ->
        model |> next looper

    // After that app is enter into the stop-resume loop.
    // Next Play msg is only possible after reloading the time point list.
    | Msg.Play ->
        model |> next looper

    | Msg.Resume when model.ActiveTimePoint |> Option.isSome && model.LooperState = LooperState.Stopped ->
        model |> resume looper windowsMinimizer

    | Msg.Stop when model.LooperState = LooperState.Playing ->
        model |> stop looper windowsMinimizer

    | Msg.Replay when model.ActiveTimePoint |> Option.isSome ->
        model |> replay

    | Msg.LooperMsg levt ->
        model |> mapLooperEvent windowsMinimizer themeSwitcher levt

    // --------------------
    // Active time changing
    // --------------------
    | Msg.PreChangeActiveTimeSpan when model.ActiveTimePoint |> Option.isSome ->
        model |> preChangeActiveTimeSpan looper

    | Msg.ChangeActiveTimeSpan v when model.ActiveTimePoint |> Option.isSome ->
        model |> changeActiveTimeSpan looper v

    | MsgWith.``Start of PostChangeActiveTimeSpan`` model (deff, cts, atp, shiftTimes) ->
        model |> startOfPostChangeActiveTimeSpan looper timeProvider workEventStore shiftTimes atp deff cts

    | MsgWith.``Finish of PostChangeActiveTimeSpan`` model (Ok (_, workSpentTimeList, atp)) ->
        model |> finishOfPostChangeActiveTimeSpan timeProvider workSpentTimeList atp

    | MsgWith.``Finish of PostChangeActiveTimeSpan`` model (Error err) ->
        model
        |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
        , Cmd.ofMsg (Msg.OnError err)
        , Intent.None

    | Msg.OnError err ->
        logger.LogProgramError err
        errorMessageQueue.EnqueueError err
        model, Cmd.none, Intent.None

    | Msg.OnExn ex ->
        logger.LogProgramExn ex
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none, Intent.None

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none, Intent.None

