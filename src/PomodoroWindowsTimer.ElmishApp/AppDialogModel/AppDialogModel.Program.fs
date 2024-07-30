module PomodoroWindowsTimer.ElmishApp.AppDialogModel.Program

open System
open System.Threading
open Elmish
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.AppDialogModel

let storeWorkReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (offset: TimeSpan) =
    task {
        let workEvent =
            WorkEvent.WorkReduced (time, offset)

        let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

        match res with
        | Ok _ -> ()
        | Error err -> raise (InvalidOperationException(err))
    }

let storeBreakIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (offset: TimeSpan) =
    task {
        let workEvent =
            WorkEvent.BreakIncreased (time, offset)

        let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

        match res with
        | Ok _ -> ()
        | Error err -> raise (InvalidOperationException(err))
    }

let update
    (cfg: AppDialogModel.Cfg)
    initBotSettingsModel
    updateBotSettingsModel
    updateRollbackWorkModel
    (msg: AppDialogModel.Msg)
    (model: AppDialogModel)
    =
    let storeWorkReducedEventTask =
        storeWorkReducedEventTask cfg.WorkEventRepository

    let storeBreakIncreasedEventTask =
        storeBreakIncreasedEventTask cfg.WorkEventRepository

    match msg with
    | Msg.LoadBotSettingsDialogModel ->
        initBotSettingsModel () |> AppDialogModel.BotSettingsDialog
        , Cmd.none

    | MsgWith.BotSettingsModelMsg model (bmsg, bm) ->
        let (m, intent) = updateBotSettingsModel bmsg bm
        match intent with
        | BotSettingsModel.Intent.None ->
            m |> AppDialogModel.BotSettingsDialog, Cmd.none

        | BotSettingsModel.Intent.CloseDialogRequested ->
            AppDialogModel.NoDialog, Cmd.none

    | Msg.LoadRollbackWorkDialogModel (workSpentTime, time) ->
        RollbackWorkModel.init workSpentTime time |> AppDialogModel.RollbackWorkDialog
        , Cmd.none

    | MsgWith.RollbackWorkModelMsg model (rmsg, rm) ->
        let (rm, intent) = updateRollbackWorkModel rmsg rm
        match intent with
        | RollbackWorkModel.Intent.None ->
            rm |> AppDialogModel.RollbackWorkDialog |> withCmdNone
        | RollbackWorkModel.Intent.DefaultedAndClose ->
            if rm.RememberChoice then
                cfg.UserSettings.RollbackWorkStrategy <- RollbackWorkStrategy.Default
            AppDialogModel.NoDialog |> withCmdNone
        | RollbackWorkModel.Intent.SubstractWorkAddBreakAndClose ->
            if rm.RememberChoice then
                cfg.UserSettings.RollbackWorkStrategy <- RollbackWorkStrategy.SubstractWorkAddBreak
            AppDialogModel.NoDialog
            , Cmd.batch [
                Cmd.OfTask.attempt (storeWorkReducedEventTask rm.WorkId (rm.Time.AddMilliseconds(-2))) rm.Difference Msg.EnqueueExn
                Cmd.OfTask.attempt (storeBreakIncreasedEventTask rm.WorkId (rm.Time.AddMilliseconds(-1))) rm.Difference Msg.EnqueueExn
            ]

    | Msg.Unload ->
        AppDialogModel.NoDialog, Cmd.none

    | Msg.EnqueueExn e ->
        cfg.MainErrorMessageQueue.EnqueueError (sprintf "%A" e)
        model, Cmd.none

    | _ ->
        model, Cmd.none


