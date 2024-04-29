module PomodoroWindowsTimer.ElmishApp.WorkSelectorModel.Bindings

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkSelectorModel

type private Binding = Binding<WorkSelectorModel, WorkSelectorModel.Msg>

[<Sealed>]
type Bindings() =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance() =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings()
            __
        else __

    static member ToList () =
        Utils.bindings<Binding>
            (Bindings.Instance())
            props

    member val SubmodelId : Binding =
        nameof __.SubmodelId |> Binding.oneWay submodelId

    member val WorkListModel : Binding =
        nameof __.WorkListModel
            |> Binding.SubModel.opt WorkListModel.Bindings.ToList
            |> Binding.mapModel workListModel
            |> Binding.mapMsg Msg.WorkListModelMsg

    member val CreatingWorkModel : Binding =
        nameof __.CreatingWorkModel
            |> Binding.SubModel.opt WorkModel.Bindings.ToList
            |> Binding.mapModel creatingWorkModel
            |> Binding.mapMsg Msg.CreatingWorkModelMsg

    member val UpdatingWorkModel : Binding =
        nameof __.UpdatingWorkModel
            |> Binding.SubModel.opt WorkModel.Bindings.ToList
            |> Binding.mapModel updatingWorkModel
            |> Binding.mapMsg Msg.UpdatingWorkModelMsg


