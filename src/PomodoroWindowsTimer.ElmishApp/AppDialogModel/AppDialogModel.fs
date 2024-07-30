namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Models
open System
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Types


type AppDialogModel =
    | NoDialog
    | BotSettingsDialog of BotSettingsModel
    | RollbackWorkDialog of RollbackWorkModel


module AppDialogModel =

    type Cfg =
        {
            UserSettings: IUserSettings
            WorkEventRepository: IWorkEventRepository
            MainErrorMessageQueue: IErrorMessageQueue
        }

    type Msg =
        | SetIsDialogOpened of bool

        | LoadBotSettingsDialogModel
        | BotSettingsModelMsg of BotSettingsModel.Msg

        | LoadRollbackWorkDialogModel of WorkSpentTime * DateTimeOffset
        | RollbackWorkModelMsg of RollbackWorkModel.Msg

        | Unload

        | EnqueueError of string
        | EnqueueExn of exn

    module MsgWith =
        let (|BotSettingsModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.BotSettingsModelMsg msg, AppDialogModel.BotSettingsDialog m ->
                (msg, m) |> Some
            | _ -> None

        let (|RollbackWorkModelMsg|_|) (model: AppDialogModel) (msg: Msg) =
            match msg, model with
            | Msg.RollbackWorkModelMsg msg, AppDialogModel.RollbackWorkDialog m ->
                (msg, m) |> Some
            | _ -> None


    [<Struct>]
    [<RequireQualifiedAccess>]
    type AppDialogId =
        | BotSettingsDialogId
        | TimePointsGeneratorDialogId
        | WorkStatisticsDialogId
        | RollbackWorkDialogId

    let appDialogId = function
        | AppDialogModel.NoDialog -> None
        | AppDialogModel.BotSettingsDialog _ -> AppDialogId.BotSettingsDialogId |> Some
        | AppDialogModel.RollbackWorkDialog _ -> AppDialogId.RollbackWorkDialogId |> Some

    let botSettingsModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.BotSettingsDialog m -> m |> Some
        | _ -> None

    let rollbackWorkModel (model: AppDialogModel) =
        match model with
        | AppDialogModel.RollbackWorkDialog m -> m |> Some
        | _ -> None


