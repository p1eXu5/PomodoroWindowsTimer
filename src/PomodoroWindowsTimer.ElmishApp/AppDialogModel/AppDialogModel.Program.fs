module PomodoroWindowsTimer.ElmishApp.AppDialogModel.Program

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.AppDialogModel


let private ofBotSettingsIntent botSettingsModel intent =
    match intent with
    | BotSettingsModel.Intent.None ->
        botSettingsModel |> AppDialogModel.BotSettings |> withCmdNone

    | BotSettingsModel.Intent.CloseDialogRequested ->
        AppDialogModel.NoDialog |> withCmdNone


let private ofRollbackWorkModelIntent (workEventStore: WorkEventStore) (userSettings: IUserSettings) rollbackWorkModel intent =
    match intent with
    | RollbackWorkModel.Intent.None ->
        rollbackWorkModel |> AppDialogModel.RollbackWork |> withCmdNone

    | RollbackWorkModel.Intent.DefaultedAndClose ->
        if rollbackWorkModel.RememberChoice then
            userSettings.RollbackWorkStrategy <- RollbackWorkStrategy.Default
        AppDialogModel.NoDialog |> withCmdNone
        
    | RollbackWorkModel.Intent.SubstractWorkAddBreakAndClose ->
        if rollbackWorkModel.RememberChoice then
            userSettings.RollbackWorkStrategy <- RollbackWorkStrategy.SubstractWorkAddBreak

        AppDialogModel.NoDialog
        , Cmd.batch [
            Cmd.OfTask.attempt workEventStore.StoreWorkReducedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(-2), rollbackWorkModel.Difference) Msg.EnqueueExn
            Cmd.OfTask.attempt workEventStore.StoreBreakIncreasedEventTask (rollbackWorkModel.WorkId, rollbackWorkModel.Time.AddMilliseconds(-1), rollbackWorkModel.Difference) Msg.EnqueueExn
        ]


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

    | Msg.LoadRollbackWorkDialogModel (workSpentTime, time, rollbackStrategy) ->
        RollbackWorkModel.init workSpentTime time rollbackStrategy |> AppDialogModel.RollbackWork |> withCmdNone

    | MsgWith.RollbackWorkModelMsg model (rmsg, rm) ->
        updateRollbackWorkModel rmsg rm ||> ofRollbackWorkModelIntent workEventStore userSettings

    | Msg.LoadRollbackWorkListDialogModel (workSpentTimeList, time, rollbackStrategy) ->
        RollbackWorkListModel.init workSpentTimeList time rollbackStrategy |> AppDialogModel.RollbackWorkList |> withCmdNone

    | MsgWith.RollbackWorkListModelMsg model (rmsg, rm) ->
        updateRollbackWorkListModel rmsg rm ||> ofRollbackWorkListModelIntent workEventStore

    | Msg.Unload ->
        AppDialogModel.NoDialog, Cmd.none

    | Msg.EnqueueExn e ->
        errorMessageQueue.EnqueueError (sprintf "%A" e)
        model, Cmd.none

    | _ ->
        model, Cmd.none


