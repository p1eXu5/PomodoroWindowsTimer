namespace PomodoroWindowsTimer.ElmishApp.StatisticMainModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.StatisticMainModel

type private Binding = Binding<StatisticMainModel, StatisticMainModel.Msg>

[<Sealed>]
type Bindings(dialogErrorMessageQueue: IErrorMessageQueue) =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance(dialogErrorMessageQueue: IErrorMessageQueue) =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings(dialogErrorMessageQueue)
            __
        else __

    static member ToList (dialogErrorMessageQueue: IErrorMessageQueue) =
        Utils.bindings<Binding>
            (Bindings.Instance(dialogErrorMessageQueue))
            props

    member val ErrorMessageQueue : Binding =
        nameof __.ErrorMessageQueue |> Binding.oneWay (fun _ -> dialogErrorMessageQueue)

    member val DailyStatistics : Binding =
        nameof __.DailyStatistics
            |> Binding.SubModel.opt (fun () -> DailyStatisticListModel.Bindings.ToList(dialogErrorMessageQueue))
            |> Binding.mapModel _.DailyStatisticListModel
            |> Binding.mapMsg Msg.DailyStatisticListModelMsg

    member val WorkList : Binding =
        nameof __.WorkList
            |> Binding.SubModel.opt WorkListModel.Bindings.bindings
            |> Binding.mapModel _.WorkListModel
            |> Binding.mapMsg Msg.WorkListModelMsg

    member val SelectedTabInd : Binding =
        nameof __.SelectedTabInd |> Binding.oneWayToSource Msg.SetSelectedTabInd

    member val CloseCommand : Binding =
        nameof __.CloseCommand |> Binding.cmd Msg.CloseWindow


