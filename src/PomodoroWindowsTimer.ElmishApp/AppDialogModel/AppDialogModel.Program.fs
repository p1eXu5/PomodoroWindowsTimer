module PomodoroWindowsTimer.ElmishApp.AppDialogModel.Program

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.AppDialogModel
open PomodoroWindowsTimer.Types


let private ofBotSettingsIntent botSettingsModel intent =
    match intent with
    | BotSettingsModel.Intent.None ->
        botSettingsModel |> AppDialogModel.BotSettings |> withCmdNone

    | BotSettingsModel.Intent.CloseDialogRequested ->
        AppDialogModel.NoDialog |> withCmdNone


let private ofRollbackWorkModelIntent (workEventStore: WorkEventStore) (userSettings: IUserSettings) dialogCtor rollbackWorkModel intent =
    match intent with
    | RollbackWorkModel.Intent.None ->
        rollbackWorkModel |> dialogCtor |> withCmdNone

    | RollbackWorkModel.Intent.Close ->
        // TODO:
        // if rollbackWorkModel.RememberChoice then
        //     userSettings.RollbackWorkStrategy <- RollbackWorkStrategy.Default
        AppDialogModel.NoDialog |> withCmdNone
        
    | RollbackWorkModel.Intent.CorrectAndClose ->
        // TODO:
        // if rollbackWorkModel.RememberChoice then
        //     userSettings.RollbackWorkStrategy <- RollbackWorkStrategy.SubstractWorkAddBreak

        match rollbackWorkModel.RollbackStrategy with
        | LocalRollbackStrategy.DoNotCorrect ->
            AppDialogModel.NoDialog |> withCmdNone

        | LocalRollbackStrategy.InvertSpentTime when rollbackWorkModel.Kind = Kind.Work ->
            AppDialogModel.NoDialog
            , Cmd.batch [
                // we are adding 1 ms cause in Player we are storing strt event with addind 1ms too because this event must not be included into the work spent time list
                Cmd.OfTask.attempt workEventStore.StoreWorkReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(1), rollbackWorkModel.Difference) Msg.EnqueueExn
                Cmd.OfTask.attempt workEventStore.StoreBreakIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(2), rollbackWorkModel.Difference) Msg.EnqueueExn
            ]
        | LocalRollbackStrategy.InvertSpentTime ->
            AppDialogModel.NoDialog
            , Cmd.batch [
                // we are adding 1 ms cause in Player we are storing strt event with addind 1ms too because this event must not be included into the work spent time list
                Cmd.OfTask.attempt workEventStore.StoreBreakReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(1), rollbackWorkModel.Difference) Msg.EnqueueExn
                Cmd.OfTask.attempt workEventStore.StoreWorkIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(2), rollbackWorkModel.Difference) Msg.EnqueueExn
            ]

        | LocalRollbackStrategy.SubstractSpentTime ->
            match rollbackWorkModel.Kind with
            | Kind.Work ->
                // we are adding 1 ms cause in Player we are storing strt event with addind 1ms too because this event must not be included into the work spent time list
                AppDialogModel.NoDialog
                ,Cmd.OfTask.attempt workEventStore.StoreWorkReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(1), rollbackWorkModel.Difference) Msg.EnqueueExn
            | Kind.Break | Kind.LongBreak ->
                // we are adding 1 ms cause in Player we are storing strt event with addind 1ms too because this event must not be included into the work spent time list
                AppDialogModel.NoDialog
                ,Cmd.OfTask.attempt workEventStore.StoreBreakReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(2), rollbackWorkModel.Difference) Msg.EnqueueExn

        | LocalRollbackStrategy.ApplyAsWorkTime ->
            AppDialogModel.NoDialog
            , Cmd.OfTask.attempt workEventStore.StoreWorkIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time, rollbackWorkModel.Difference) Msg.EnqueueExn

        | LocalRollbackStrategy.ApplyAsBreakTime ->
            AppDialogModel.NoDialog
            , Cmd.OfTask.attempt workEventStore.StoreBreakIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time, rollbackWorkModel.Difference) Msg.EnqueueExn


let private ofRollbackWorkListModelIntent (workEventStore: WorkEventStore) rollbackWorkListModel (intent: RollbackWorkListModel.Intent) =
    rollbackWorkListModel |> AppDialogModel.RollbackWorkList |> withCmdNone


let update
    (workEventStore: WorkEventStore)
    (userSettings: IUserSettings)
    (errorMessageQueue: IErrorMessageQueue)
    initBotSettingsModel
    updateBotSettingsModel
    updateRollbackWorkModel
    updateRollbackWorkListModel
    (msg: AppDialogModel.Msg) (model: AppDialogModel)
    =
    match msg with
    | Msg.LoadBotSettingsDialogModel ->
        initBotSettingsModel () |> AppDialogModel.BotSettings
        , Cmd.none

    | MsgWith.BotSettingsModelMsg model (bmsg, bm) ->
        updateBotSettingsModel bmsg bm ||> ofBotSettingsIntent

    // ------------------------------------
    | Msg.LoadRollbackWorkDialogModel (workSpentTime, kind, time) ->
        RollbackWorkModel.init workSpentTime kind time |> AppDialogModel.RollbackWork |> withCmdNone

    | MsgWith.RollbackWorkModelMsg model (rmsg, rm) ->
        updateRollbackWorkModel rmsg rm ||> ofRollbackWorkModelIntent workEventStore userSettings AppDialogModel.RollbackWork

    // ------------------------------------
    | Msg.LoadRollbackWorkListDialogModel (workSpentTimeList, kind, time) ->
        RollbackWorkListModel.init workSpentTimeList kind time |> AppDialogModel.RollbackWorkList |> withCmdNone

    | MsgWith.RollbackWorkListModelMsg model (rmsg, rm) ->
        updateRollbackWorkListModel rmsg rm ||> ofRollbackWorkListModelIntent workEventStore

    // ------------------------------------
    | Msg.LoadSkipOrApplyMissingTimeDialogModel (workId, kind, difference, time) ->
        RollbackWorkModel.initWithMissingTime workId kind difference time |> AppDialogModel.SkipOrApplyMissingTime |> withCmdNone

    | MsgWith.SkipOrApplyMissingTimeModelMsg model (smsg, sm) ->
        updateRollbackWorkModel smsg sm ||> ofRollbackWorkModelIntent workEventStore userSettings AppDialogModel.SkipOrApplyMissingTime

    // ------------------------------------

    | Msg.Unload ->
        AppDialogModel.NoDialog, Cmd.none

    | Msg.EnqueueExn e ->
        errorMessageQueue.EnqueueError (sprintf "%A" e)
        model, Cmd.none

    | _ ->
        model, Cmd.none


