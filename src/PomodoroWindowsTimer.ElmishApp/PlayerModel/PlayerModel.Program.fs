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
            let timePointKind = nextTp |> TimePointKind.ofActiveTimePoint

            let storeStartedWorkEventOrActiveTimePointCmd =
                match currentWorkOpt, model.LooperState with
                | Some work, LooperState.Playing ->
                    let time = timeProvider.GetUtcNow()
                    Cmd.OfTask.attempt workEventStore.StoreStartedWorkEventTask (work.Id, time, nextTp) Msg.OnExn
                | _ when oldTp |> Option.isNone && model.LooperState <> LooperState.Playing ->
                    Cmd.OfTask.attempt workEventStore.StoreActiveTimePointTask nextTp Msg.OnExn
                | _ ->
                    Cmd.none

            let sendToChatBotCmd =
                match currentWorkOpt with
                | None ->
                    $"It's time to {nextTp.Name}!!"
                | Some wm ->
                    $"""It's time to {nextTp.Name}!!
                    
Current work is [{wm.Number}] {wm.Title}."""
                |> fun message ->
                    Cmd.OfTask.attempt telegramBot.SendMessage message Msg.OnExn

            let cmd =
                match nextTp.Kind with
                | LongBreak | Break when model.LooperState <> LooperState.Playing ->
                    Cmd.batch [
                        switchThemeCmd timePointKind
                        storeStartedWorkEventOrActiveTimePointCmd
                    ]

                | LongBreak | Break when oldTp |> Option.isSome && (oldTp.Value.Kind = Kind.Break || oldTp.Value.Kind = Kind.LongBreak) ->
                    storeStartedWorkEventOrActiveTimePointCmd

                | LongBreak | Break ->
                    Cmd.batch [
                        storeStartedWorkEventOrActiveTimePointCmd
                        switchThemeCmd timePointKind
                        minimizeAllRestoreAppWindowCmd ()
                    ]

                // initialized, not playing, just switch theme
                | Work when model.LooperState <> LooperState.Playing ->
                    Cmd.batch [
                        switchThemeCmd timePointKind
                        storeStartedWorkEventOrActiveTimePointCmd
                    ]

                // do not send a message to the Telegram if the user manually presses the Next button or the Play button
                | Work when oldTp |> Option.isNone || isLastAtpWhenPlayOrNextIsManuallyPressed oldTp model ->
                    Cmd.batch [
                        storeStartedWorkEventOrActiveTimePointCmd
                        switchThemeCmd timePointKind
                        restoreAllMinimizedCmd ()
                    ]

                | Work when oldTp |> Option.isSome && (oldTp.Value.Kind = Kind.Work) ->
                    Cmd.batch [
                        storeStartedWorkEventOrActiveTimePointCmd
                        sendToChatBotCmd
                    ]

                | Work ->
                    Cmd.batch [
                        storeStartedWorkEventOrActiveTimePointCmd
                        switchThemeCmd timePointKind
                        restoreAllMinimizedCmd ()
                        sendToChatBotCmd
                    ]

            model
            |> withActiveTimePoint (nextTp |> Some)
            |> withNoneLastAtpWhenPlayOrNextIsManuallyPressed
            , cmd
            , Intent.None

    // --------------------
    // Active time changing
    // --------------------
    | Msg.PreChangeActiveTimeSpan when model.ActiveTimePoint |> Option.isSome ->
        let time = timeProvider.GetUtcNow()
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

    | MsgWith.``Start of PostChangeActiveTimeSpan`` model (deff, cts, atp, shiftTimes) ->
        let now = timeProvider.GetUtcNow()

        let prevState, storeWorkStartedCmd =
            match model.LooperState with
            | LooperState.TimeShifting prevState ->
                match prevState with
                | LooperState.Playing ->
                    looper.Resume()
                    prevState
                    , Cmd.batch [
                        if currentWorkOpt |> Option.isSome then
                            Cmd.OfTask.attempt
                                workEventStore.StoreStartedWorkEventTask
                                (
                                    currentWorkOpt.Value.Id
                                    , now.AddMilliseconds(1.0) // started event must not be included int work spent time list
                                    , model.ActiveTimePoint.Value
                                )
                                Msg.OnExn
                        ]
                | _ ->
                    prevState, Cmd.none
            | _ -> model.LooperState, Cmd.none

        match currentWorkOpt with
        | None ->
            model
            |> withLooperState prevState
            |> withoutShiftAndPreShiftTimes
            |> withNoCmdAndIntent
        | Some currentWork ->
            // no shifting
            if Math.Abs(float (shiftTimes.NewActiveRemainingSeconds - shiftTimes.PreShiftActiveRemainingSeconds)) < 1.0 then
                model
                |> withLooperState prevState
                |> withoutShiftAndPreShiftTimes
                , storeWorkStartedCmd
                , Intent.None

            // shifting forward
            elif shiftTimes.NewActiveRemainingSeconds < shiftTimes.PreShiftActiveRemainingSeconds then
                    model
                    |> withLooperState prevState
                    |> withoutShiftAndPreShiftTimes
                    , storeWorkStartedCmd
                    , Intent.SkipOrApplyMissingTime (
                        currentWork.Id,
                        atp.Kind,
                        atp.Id,
                        TimeSpan.FromSeconds(float (shiftTimes.PreShiftActiveRemainingSeconds - shiftTimes.NewActiveRemainingSeconds)),
                        now
                    )
            // shifting backward
            else
                model
                |> withLooperState prevState
                |> withoutShiftAndPreShiftTimes
                |> withRetreiveWorkSpentTimesState deff
                , Cmd.batch [
                    storeWorkStartedCmd
                    Cmd.OfTask.perform
                        workEventStore.WorkSpentTimeListTask
                        (
                            atp.Id,
                            atp.Kind,
                            now,
                            (shiftTimes.NewActiveRemainingSeconds - shiftTimes.PreShiftActiveRemainingSeconds),
                            cts.Token
                        )
                        (AsyncOperation.finishWithin Msg.PostChangeActiveTimeSpan cts)
                ]
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
            , Cmd.none
            , Intent.None

        | Ok (_, [ workSpentTime ], atp) ->
            model
            |> withRetreiveWorkSpentTimesState AsyncDeferredState.NotRequested
            , Cmd.none
            , Intent.RollbackTime (workSpentTime, atp.Kind, atp.Id, timeProvider.GetUtcNow())

        | Ok (_, workSpentTimeList, atp) ->
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

    | Msg.OnError err ->
        logger.LogError err
        errorMessageQueue.EnqueueError err
        model, Cmd.none, Intent.None

    | Msg.OnExn ex ->
        logger.LogError(ex, "Exception has been thrown in PlayerModel Program.")
        errorMessageQueue.EnqueueError ex.Message
        model |> withCmdNone |> withNoIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent

