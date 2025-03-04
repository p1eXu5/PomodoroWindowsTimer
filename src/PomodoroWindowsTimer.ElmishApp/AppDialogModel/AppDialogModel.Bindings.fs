namespace PomodoroWindowsTimer.ElmishApp.AppDialogModel

open Elmish.WPF
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.AppDialogModel

type private Binding = Binding<AppDialogModel, AppDialogModel.Msg>

[<Sealed>]
type Bindings(dialogErrorMessageQueue: IErrorMessageQueue) =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance(dialogErrorMessageQueue: IErrorMessageQueue) =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings(dialogErrorMessageQueue)
            __
        else __

    static member ToList dialogErrorMessageQueue =
        Utils.bindings<Binding>
            (Bindings.Instance(dialogErrorMessageQueue))
            props

    member val IsDialogOpened : Binding =
        nameof __.IsDialogOpened
            |> Binding.twoWay ((<>) AppDialogModel.NoDialog, Msg.SetIsDialogOpened)

    member val AppDialogId : Binding =
        nameof __.AppDialogId
            |> Binding.oneWayOpt appDialogId

    // -------------------------------------------------
    member val OpenBotSettingsDialogCommand : Binding =
        nameof __.OpenBotSettingsDialogCommand
            |> Binding.cmdIf (function AppDialogModel.NoDialog -> Msg.LoadBotSettingsDialogModel |> Some | _ -> None)

    member val BotSettingsDialog : Binding =
        nameof __.BotSettingsDialog
            |> Binding.SubModel.opt (BotSettingsModel.Bindings.ToList)
            |> Binding.mapModel (tryBotSettingsModel)
            |> Binding.mapMsg (Msg.BotSettingsModelMsg)

    // -------------------------------------------------

    member val ShowOpenDatabaseSettingsDialogCommand : Binding =
        nameof __.ShowOpenDatabaseSettingsDialogCommand
            |> Binding.oneWay (fun _ -> true)

    member val OpenDatabaseSettingsDialogCommand : Binding =
        nameof __.OpenDatabaseSettingsDialogCommand
            |> Binding.cmdIf (function AppDialogModel.NoDialog -> Msg.LoadDatabaseSettingsDialogModel |> Some | _ -> None)

    member val DatabaseSettingsDialog : Binding =
        nameof __.DatabaseSettingsDialog
            |> Binding.SubModel.opt (DatabaseSettingsModel.Bindings.ToList)
            |> Binding.mapModel (tryDatabaseSettingsModel)
            |> Binding.mapMsg (Msg.DatabaseSettingsModelMsg)

    // -------------------------------------------------

    member val RollbackWorkDialog : Binding =
        nameof __.RollbackWorkDialog
            |> Binding.SubModel.opt (RollbackWorkModel.Bindings.ToList)
            |> Binding.mapModel (tryRollbackWorkModel)
            |> Binding.mapMsg (Msg.RollbackWorkModelMsg)

    // -------------------------------------------------

    member val SkipOrApplyMissingTimeDialog : Binding =
        nameof __.SkipOrApplyMissingTimeDialog
            |> Binding.SubModel.opt (RollbackWorkModel.Bindings.ToList)
            |> Binding.mapModel (trySkipOrApplyMissingTimeModel)
            |> Binding.mapMsg (Msg.SkipOrApplyMissingTimeModelMsg)

    // -------------------------------------------------

    member val RollbackWorkListDialog : Binding =
        nameof __.RollbackWorkListDialog
            |> Binding.SubModel.opt (RollbackWorkListModel.Bindings.ToList)
            |> Binding.mapModel (tryRollbackWorkListModel)
            |> Binding.mapMsg (Msg.RollbackWorkListModelMsg)

    // -------------------------------------------------

    // -------------------------------------------------

    member val UnloadDialogModelCommand : Binding =
        nameof __.UnloadDialogModelCommand |> Binding.cmd Msg.Unload

