module PomodoroWindowsTimer.ElmishApp.AppDialogModel.Program

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.AppDialogModel
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions


let private ofBotSettingsIntent botSettingsModel intent =
    match intent with
    | BotSettingsModel.Intent.None ->
        botSettingsModel |> AppDialogModel.BotSettings |> withCmdNone

    | BotSettingsModel.Intent.CloseDialogRequested ->
        AppDialogModel.NoDialog |> withCmdNone

let private ofDatabaseSettingsIntent databaseSettingsModel intent =
    match intent with
    | DatabaseSettingsModel.Intent.None ->
        databaseSettingsModel |> AppDialogModel.DatabaseSettings |> withCmdNone

    | DatabaseSettingsModel.Intent.CloseDialogRequested ->
        AppDialogModel.NoDialog |> withCmdNone


let private rollbackWorkIntentCmd (workEventStore: WorkEventStore) rollbackWorkModel =
    match rollbackWorkModel.RollbackStrategy with
    | LocalRollbackStrategy.DoNotCorrect -> Cmd.none
    | LocalRollbackStrategy.SubstractSpentTime ->
        match rollbackWorkModel.Kind with
        | Kind.Work ->
            Cmd.OfTask.attempt workEventStore.StoreWorkReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(1), rollbackWorkModel.Difference, rollbackWorkModel.ActiveTimePointId |> Some) Msg.EnqueueExn
        | Kind.Break | Kind.LongBreak ->
            Cmd.OfTask.attempt workEventStore.StoreBreakReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(2), rollbackWorkModel.Difference, rollbackWorkModel.ActiveTimePointId |> Some) Msg.EnqueueExn

    | LocalRollbackStrategy.ApplyAsWorkTime ->
        Cmd.OfTask.attempt workEventStore.StoreWorkIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time, rollbackWorkModel.Difference, rollbackWorkModel.ActiveTimePointId |> Some) Msg.EnqueueExn

    | LocalRollbackStrategy.ApplyAsBreakTime ->
        Cmd.OfTask.attempt workEventStore.StoreBreakIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time, rollbackWorkModel.Difference, rollbackWorkModel.ActiveTimePointId |> Some) Msg.EnqueueExn

    | LocalRollbackStrategy.InvertSpentTime when rollbackWorkModel.Kind = Kind.Work ->
        Cmd.batch [
            // we are adding 1 ms cause in Player we are storing strt event with addind 1ms too because this event must not be included into the work spent time list
            Cmd.OfTask.attempt workEventStore.StoreWorkReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(1), rollbackWorkModel.Difference, rollbackWorkModel.ActiveTimePointId |> Some) Msg.EnqueueExn
            Cmd.OfTask.attempt workEventStore.StoreBreakIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(2), rollbackWorkModel.Difference, rollbackWorkModel.ActiveTimePointId |> Some) Msg.EnqueueExn
        ]
    | LocalRollbackStrategy.InvertSpentTime ->
        Cmd.batch [
            // we are adding 1 ms cause in Player we are storing strt event with addind 1ms too because this event must not be included into the work spent time list
            Cmd.OfTask.attempt workEventStore.StoreBreakReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(1), rollbackWorkModel.Difference, rollbackWorkModel.ActiveTimePointId |> Some) Msg.EnqueueExn
            Cmd.OfTask.attempt workEventStore.StoreWorkIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(2), rollbackWorkModel.Difference, rollbackWorkModel.ActiveTimePointId |> Some) Msg.EnqueueExn
        ]

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

        let cmd = rollbackWorkIntentCmd workEventStore rollbackWorkModel
        AppDialogModel.NoDialog, cmd

let private ofRollbackWorkListModelIntent (workEventStore: WorkEventStore) rollbackWorkListModel (intent: RollbackWorkListModel.Intent) =
    match intent with
    | RollbackWorkListModel.Intent.None ->
        rollbackWorkListModel |> AppDialogModel.RollbackWorkList |> withCmdNone
    | RollbackWorkListModel.Intent.Close ->
        AppDialogModel.NoDialog |> withCmdNone
    | RollbackWorkListModel.Intent.ProcessRollbackAndClose ->
        let cmdList =
            rollbackWorkListModel.RollbackList
            |> List.map (fun m -> rollbackWorkIntentCmd workEventStore m)
            |> Cmd.batch

        AppDialogModel.NoDialog, cmdList


let update
    (workEventStore: WorkEventStore)
    (userSettings: IUserSettings)
    (errorMessageQueue: IErrorMessageQueue)
    initBotSettingsModel
    updateBotSettingsModel
    initDatabaseSettingsModel
    updateDatabaseSettingsModel
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
    | Msg.LoadDatabaseSettingsDialogModel ->
        initDatabaseSettingsModel () |> AppDialogModel.DatabaseSettings
        , Cmd.none

    | MsgWith.DatabaseSettingsModelMsg model (bmsg, bm) ->
        updateDatabaseSettingsModel bmsg bm ||> ofDatabaseSettingsIntent

    // ------------------------------------
    | Msg.LoadRollbackWorkDialogModel (workSpentTime, kind, atpId, time) ->
        RollbackWorkModel.init workSpentTime kind atpId time |> AppDialogModel.RollbackWork |> withCmdNone

    | MsgWith.RollbackWorkModelMsg model (rmsg, rm) ->
        updateRollbackWorkModel rmsg rm ||> ofRollbackWorkModelIntent workEventStore userSettings AppDialogModel.RollbackWork

    // ------------------------------------
    | Msg.LoadRollbackWorkListDialogModel (workSpentTimeList, kind, atpId, time) ->
        RollbackWorkListModel.init workSpentTimeList kind atpId time |> AppDialogModel.RollbackWorkList |> withCmdNone

    | MsgWith.RollbackWorkListModelMsg model (rmsg, rm) ->
        updateRollbackWorkListModel rmsg rm ||> ofRollbackWorkListModelIntent workEventStore

    // ------------------------------------
    | Msg.LoadSkipOrApplyMissingTimeDialogModel (workId, kind, atpId, difference, time) ->
        RollbackWorkModel.initWithMissingTime workId kind atpId difference time |> AppDialogModel.SkipOrApplyMissingTime |> withCmdNone

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


