namespace PomodoroWindowsTimer.ElmishApp.WorkListModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkListModel

type private Binding = Binding<WorkListModel, WorkListModel.Msg>

type IBindings =
    interface
        abstract WorkModelList: WorkModel.IBindings seq
        abstract WorkListDeff: AsyncDeferred<WorkModel list>
        abstract SelectedWorkModel: WorkModel.IBindings option
        abstract HasSelectedWork: bool
        abstract CreateWorkCommand: unit -> unit
        abstract UnselectWorkCommand: unit -> unit
        abstract LastDayCount: string
        abstract SelectedWorkId: int option
    end

module Bindings =

    let private __ = Unchecked.defaultof<IBindings>

    let bindings () =
        [
            nameof __.WorkModelList
                |> Binding.subModelSeq (WorkModel.Bindings.bindings, _.Work >> _.Id)
                |> Binding.mapModel (fun m  ->
                    m.Works
                    |> AsyncDeferred.chooseRetrieved
                    |> Option.defaultValue List.empty
                    : WorkModel list
                )
                |> Binding.mapMsg Msg.WorkModelMsg

            nameof __.WorkListDeff |> Binding.oneWay _.Works

            nameof __.SelectedWorkModel
                |> Binding.SubModel.opt WorkModel.Bindings.bindings
                |> Binding.mapModel selectedWorkModel
                |> Binding.mapMsgWithModel (fun msg model -> Msg.WorkModelMsg (model.SelectedWorkId.Value, msg))

            nameof __.HasSelectedWork |> Binding.oneWay (selectedWorkModel >> Option.isSome)

            nameof __.CreateWorkCommand |> Binding.cmd Msg.CreateWork

            nameof __.UnselectWorkCommand |> Binding.cmd Msg.UnselectWork

            nameof __.LastDayCount |> Binding.twoWay (lastDayCountText, Msg.SetLastDayCount)

            nameof __.SelectedWorkId |> Binding.oneWayOpt _.SelectedWorkId
        ]

