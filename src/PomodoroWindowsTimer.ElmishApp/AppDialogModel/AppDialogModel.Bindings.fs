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

    member val ErrorMessageQueue =
        nameof __.ErrorMessageQueue |> Binding.oneWay (fun _ -> dialogErrorMessageQueue) : Binding

    member val IsDialogOpened : Binding =
        nameof __.IsDialogOpened
            |> Binding.twoWay (_.IsOpened, Msg.SetIsDialogOpened)

    member val OpenBotSettingsDialogCommand : Binding =
        nameof __.OpenBotSettingsDialogCommand
            |> Binding.cmdIf (_.Dialog >> function NoDialog -> Msg.LoadBotSettingsDialogModel |> Some | _ -> None)

    member val BotSettingsDialog : Binding =
        nameof __.BotSettingsDialog
            |> Binding.SubModel.opt (BotSettingsModel.Bindings.ToList)
            |> Binding.mapModel (botSettingsModel)
            |> Binding.mapMsg (Msg.BotSettingsModelMsg)

    member val ApplyBotSettingsCommand : Binding =
        nameof __.ApplyBotSettingsCommand |> Binding.cmd Msg.ApplyBotSettings

    member val OpenTimePointsGeneratorDialogCommand : Binding =
        nameof __.OpenTimePointsGeneratorDialogCommand
            |> Binding.cmdIf (_.Dialog >> function NoDialog -> Msg.LoadTimePointsGeneratorDialogModel |> Some | _ -> None)

    member val TimePointsGeneratorDialog : Binding =
        nameof __.TimePointsGeneratorDialog
            |> Binding.SubModel.opt (TimePointsGeneratorModel.Bindings.bindings)
            |> Binding.mapModel (timePointsGeneratorModel)
            |> Binding.mapMsg (Msg.TimePointsGeneratorModelMsg)

    member val OpenWorkStatisticsDialogCommand : Binding =
        nameof __.OpenWorkStatisticsDialogCommand
            |> Binding.cmdIf (_.Dialog >> function NoDialog -> Msg.LoadWorkStatisticsDialogModel |> Some | _ -> None)

    member val WorkStatisticsDialog : Binding =
        nameof __.WorkStatisticsDialog
            |> Binding.SubModel.opt (fun () -> WorkStatisticListModel.Bindings.ToList(dialogErrorMessageQueue))
            |> Binding.mapModel (workStatisticListModel)
            |> Binding.mapMsg (Msg.WorkStatisticListModelMsg)

    member val UnloadDialogModelCommand : Binding =
        nameof __.UnloadDialogModelCommand |> Binding.cmd Msg.Unload

