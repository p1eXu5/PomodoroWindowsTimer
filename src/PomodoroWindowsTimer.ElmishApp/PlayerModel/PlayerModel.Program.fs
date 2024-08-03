﻿module PomodoroWindowsTimer.ElmishApp.PlayerModel.Program

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
open PomodoroWindowsTimer.ElmishApp.Models.PlayerModel

let private withNoCmdAndIntent m = (m, Cmd.none, Intent.None)

let private withNoIntent (m, cmd) = (m, cmd, Intent.None)

let update
    (looper: ILooper)
    (windowsMinimizer: IWindowsMinimizer)
    (timeProvider: System.TimeProvider)
    (workEventStore: WorkEventStore)
    (themeSwitcher: IThemeSwitcher)
    (telegramBot: ITelegramBot)
    (userSettings: IUserSettings)
    (timePointQueue: ITimePointQueue)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<PlayerModel>)
    (currentWorkOpt: Work option) msg model
    =
    let minimizeAllRestoreAppWindowCmd () =
        if model.DisableMinimizeMaximizeWindows then
            Cmd.none
        else
            Cmd.OfFunc.attempt windowsMinimizer.MinimizeAllRestoreAppWindowAsync () Msg.OnExn

    let restoreAllMinimizedCmd () =
        if model.DisableMinimizeMaximizeWindows then
            Cmd.none
        else
            Cmd.OfFunc.attempt windowsMinimizer.RestoreAllMinimized () Msg.OnExn

    let restoreAppWindowCmd () =
        if model.DisableMinimizeMaximizeWindows then
            Cmd.none
        else
            Cmd.OfFunc.attempt windowsMinimizer.RestoreAppWindow () Msg.OnExn

    let switchThemeCmd timePointKind =
        Cmd.OfFunc.attempt themeSwitcher.SwitchTheme timePointKind Msg.OnExn

    match msg with
    | Msg.SetDisableSkipBreak v ->
        model |> withDisableSkipBreak v
        , Cmd.OfFunc.attempt (fun() -> userSettings.DisableSkipBreak <- v) () Msg.OnExn
        , Intent.None

    | Msg.SetDisableMinimizeMaximizeWindows v ->
        model |> withDisableMinimizeMaximizeWindows v |> withNoCmdAndIntent

    | Msg.StartTimePoint op ->
        match op with
        | Operation.Start id ->
            model
            , Cmd.batch [
                Cmd.ofMsg Msg.Stop
                Cmd.OfFunc.either timePointQueue.ScrollTo id (Operation.Finish >> Msg.StartTimePoint) Msg.OnExn
            ]
            , Intent.None

        | Operation.Finish _ ->
            model
            , Cmd.ofMsg Msg.Play
            , Intent.None

    | Msg.Next ->
        looper.Next()
        model
        |> withLooperState Playing
        |> withLastAtpWhenPlayOrNextIsManuallyPressed
        |> withNoCmdAndIntent

    // After that app is enter into the stop-resume loop.
    // Next Play msg is possible after reloading the time point list.
    | Msg.Play ->
        looper.Next()
        model
        |> withLooperState Playing
        |> withLastAtpWhenPlayOrNextIsManuallyPressed
        |> withNoCmdAndIntent

    | Msg.Resume when model.ActiveTimePoint |> Option.isSome && model.LooperState = LooperState.Stopped ->
        let time = timeProvider.GetUtcNow()
        looper.Resume() // next looper event is TimePointReduced

        let model = model |> withLooperState Playing
        let storeCmd =
            match currentWorkOpt with
            | Some work ->
                Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (work.Id, time, model.ActiveTimePoint.Value) Msg.OnExn
            | _ ->
                Cmd.none

        let isMinimized = windowsMinimizer.GetIsMinimized()

        let winCmd =
            match model.ActiveTimePoint.Value with
            | { Kind = Kind.Break } when not isMinimized -> minimizeAllRestoreAppWindowCmd ()
            | { Kind = Kind.Work } when isMinimized -> restoreAllMinimizedCmd ()
            | _ -> Cmd.none

        model, Cmd.batch [ storeCmd; winCmd ], Intent.None

    | Msg.Stop when model.LooperState = LooperState.Playing ->
        looper.Stop()
        let time = timeProvider.GetUtcNow()
        let model = model |> withLooperState Stopped
        let cmd =
            match currentWorkOpt, model.ActiveTimePoint with
            | Some work, Some tp ->
                Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (work.Id, time, tp) Msg.OnExn
            | _ ->
                Cmd.none
        let winCmd =
            Cmd.batch [
                restoreAllMinimizedCmd ()
                // restoreAppWindowCmd ()
            ]

        model, Cmd.batch [ cmd; winCmd ], Intent.None

    | Msg.Replay when model.ActiveTimePoint |> Option.isSome ->
        let cmd =
            Cmd.batch [
                Cmd.ofMsg (Msg.PreChangeActiveTimeSpan);
                Cmd.ofMsg (Msg.ChangeActiveTimeSpan 0.0);
                Cmd.ofMsg (AsyncOperation.startUnit Msg.PostChangeActiveTimeSpan);
                if model.LooperState = LooperState.Stopped then
                    Cmd.ofMsg (Msg.Resume);
            ]
        model, cmd, Intent.None

    | Msg.LooperMsg evt ->
        match evt with
        | LooperMsg.TimePointTimeReduced tp ->
            model
            |> withActiveTimePoint (tp |> Some)
            |> withNoneLastAtpWhenPlayOrNextIsManuallyPressed
            |> withNoCmdAndIntent

        | LooperMsg.TimePointStarted ({NewActiveTimePoint = nextTp; OldActiveTimePoint = oldTp }) ->
            let time = timeProvider.GetUtcNow()
            let model =
                model
                |> withActiveTimePoint (nextTp |> Some)
                |> withNoneLastAtpWhenPlayOrNextIsManuallyPressed

            let timePointKind = model |> timePointKindEnum

            let storeStartedWorkEventCmd =
                match currentWorkOpt, model.LooperState with
                | Some work, LooperState.Playing ->
                    Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (work.Id, time, nextTp) Msg.OnExn
                | _ ->
                    Cmd.none

            let sendToChatBotCmd message =
                Cmd.OfTask.attempt telegramBot.SendMessage message Msg.OnExn

            match nextTp.Kind with
            | LongBreak
            | Break when model.LooperState <> LooperState.Playing ->
                model, switchThemeCmd timePointKind, Intent.None

            | LongBreak
            | Break when oldTp |> Option.isSome && (oldTp.Value.Kind = Kind.Break || oldTp.Value.Kind = Kind.LongBreak) ->
                model, storeStartedWorkEventCmd, Intent.None

            | LongBreak
            | Break ->
                model
                , Cmd.batch [
                    storeStartedWorkEventCmd
                    switchThemeCmd timePointKind
                    minimizeAllRestoreAppWindowCmd ()
                ]
                , Intent.None

            // initialized
            | Work when model.LooperState <> LooperState.Playing ->
                model, switchThemeCmd timePointKind, Intent.None

            // do not send a notification if the user manually presses the Next button or the Play button
            | Work when isLastAtpWhenPlayOrNextIsManuallyPressed oldTp model || oldTp |> Option.isNone ->
                model
                , Cmd.batch [
                    storeStartedWorkEventCmd
                    switchThemeCmd timePointKind
                    restoreAllMinimizedCmd ()
                ]
                , Intent.None

            | Work when oldTp |> Option.isSome && (oldTp.Value.Kind = Kind.Work) ->
                model
                , Cmd.batch [
                    storeStartedWorkEventCmd
                    sendToChatBotCmd $"It's time to {nextTp.Name}!!"
                ]
                , Intent.None

            | Work ->
                model
                , Cmd.batch [
                    storeStartedWorkEventCmd
                    switchThemeCmd timePointKind
                    restoreAllMinimizedCmd ()
                    sendToChatBotCmd $"It's time to {nextTp.Name}!!"
                ]
                , Intent.None

    // --------------------
    // Active time changing
    // --------------------
    | Msg.PreChangeActiveTimeSpan when model.ActiveTimePoint |> Option.isSome ->
        match model.LooperState with
        | Playing ->
            looper.Stop()
            let time = timeProvider.GetUtcNow()
            model |> withPreShiftState
            , Cmd.batch [
                if currentWorkOpt |> Option.isSome then
                    Cmd.OfTask.attempt workEventStore.StoreStoppedWorkEventTask (currentWorkOpt.Value.Id, time, model.ActiveTimePoint.Value) Msg.OnExn
            ]
            , Intent.None
        | _ ->
            model |> withPreShiftState |> withNoCmdAndIntent

    | Msg.ChangeActiveTimeSpan v when model.ActiveTimePoint |> Option.isSome ->
        let duration = model |> getActiveTimeDuration
        let remainingSeconds = (duration - v) * 1.0<sec>
        looper.Shift(remainingSeconds)

        model |> withNewActiveRemainingSeconds remainingSeconds |> withCmdNone |> withNoIntent

    |  MsgWith.``Start of PostChangeActiveTimeSpan`` model (deff, cts, atp, shiftTimes)->
        let withResumeState model =
            match model.LooperState with
            | LooperState.TimeShifting prevState ->
                if prevState = LooperState.Playing then
                    looper.Resume()
                model |> withLooperState prevState
            | _ -> model

        match currentWorkOpt with
        | None ->
            model
            |> withResumeState
            |> withoutShiftAndPreShiftTimes
            |> withNoCmdAndIntent
        | Some currentWork ->
            if Math.Abs(float (shiftTimes.NewActiveRemainingSeconds - shiftTimes.PreShiftActiveRemainingSeconds)) < 1.0 then
                model
                |> withResumeState
                |> withoutShiftAndPreShiftTimes
                |> withNoCmdAndIntent
            // shifting forward
            elif shiftTimes.NewActiveRemainingSeconds < shiftTimes.PreShiftActiveRemainingSeconds then
                    model
                    |> withResumeState
                    |> withoutShiftAndPreShiftTimes
                    , Cmd.none
                    , Intent.SkipOrApplyMissingTime (currentWork.Id, TimeSpan.FromSeconds(float (shiftTimes.PreShiftActiveRemainingSeconds - shiftTimes.NewActiveRemainingSeconds)), atp.Kind)
            // shifting backward
            else
                model
                |> withResumeState
                |> withoutShiftAndPreShiftTimes
                |> withRetreiveWorkSpentTimesState deff
                , Cmd.OfTask.perform
                    workEventStore.WorkSpentTimeListTask
                    (atp.Id, timeProvider.GetUtcNow(), (shiftTimes.NewActiveRemainingSeconds - shiftTimes.PreShiftActiveRemainingSeconds), cts.Token)
                    (AsyncOperation.finishWithin Msg.PostChangeActiveTimeSpan cts)
                , Intent.None

    | MsgWith.``Finish of PostChangeActiveTimeSpan`` model res ->
        match res with
        | Error err ->
            model
            |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
            , Cmd.ofMsg (Msg.OnError err)
            , Intent.None

        | Ok (_, [], _) ->
             model
            |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
            , Cmd.ofMsg (Msg.OnError "Work spent time list is unexpected empty!")
            , Intent.None

        | Ok (_, [ workSpentTime ], atp) ->
            model
            |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
            , Cmd.none
            , Intent.RollbackTime (workSpentTime, timeProvider.GetUtcNow(), atp.Kind)

        | Ok (_, workSpentTimeList, atp) ->
            model
            |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
            , Cmd.none
            , Intent.MultipleRollbackTime (workSpentTimeList, timeProvider.GetUtcNow(), atp.Kind)
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

    | Msg.OnError err ->
        errorMessageQueue.EnqueueError err
        model, Cmd.none, Intent.None

    | Msg.OnExn ex ->
        errorMessageQueue.EnqueueError ex.Message
        model |> withCmdNone |> withNoIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent

