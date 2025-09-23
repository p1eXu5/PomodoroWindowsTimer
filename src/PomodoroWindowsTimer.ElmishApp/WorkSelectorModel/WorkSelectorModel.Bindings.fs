namespace PomodoroWindowsTimer.ElmishApp.WorkSelectorModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkSelectorModel

type IBindings =
    interface
        abstract SubModelId: SubModelId
        abstract WorkListModel: WorkListModel.IBindings option
        abstract CreatingWorkModel: CreatingWorkModel.IBindings option
        abstract UpdatingWorkModel: WorkModel.IBindings option
    end

module Bindings =
    let private __ = Unchecked.defaultof<IBindings>

    let bindings () =
        [
            nameof __.SubModelId |> Binding.oneWay subModelId

            nameof __.WorkListModel
                |> Binding.SubModel.opt WorkListModel.Bindings.bindings
                |> Binding.mapModel workListModel
                |> Binding.mapMsg Msg.WorkListModelMsg

            nameof __.CreatingWorkModel
                |> Binding.SubModel.opt CreatingWorkModel.Bindings.bindings
                |> Binding.mapModel creatingWorkModel
                |> Binding.mapMsg Msg.CreatingWorkModelMsg

            nameof __.UpdatingWorkModel
                |> Binding.SubModel.opt WorkModel.Bindings.bindings
                |> Binding.mapModel updatingWorkModel
                |> Binding.mapMsg Msg.UpdatingWorkModelMsg
        ]

