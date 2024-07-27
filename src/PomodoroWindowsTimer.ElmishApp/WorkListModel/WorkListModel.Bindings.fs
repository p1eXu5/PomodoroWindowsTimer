namespace PomodoroWindowsTimer.ElmishApp.WorkListModel


open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkListModel

type private Binding = Binding<WorkListModel, WorkListModel.Msg>

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

    member val WorkModelList : Binding =
        nameof __.WorkModelList
            |> Binding.subModelSeq (WorkModel.Bindings.ToList, _.Work >> _.Id)
            |> Binding.mapModel (fun m  ->
                m.Works
                |> AsyncDeferred.chooseRetrieved
                |> Option.defaultValue List.empty
                : WorkModel list
            )
            |> Binding.mapMsg Msg.WorkModelMsg

    member val SelectedWorkModel : Binding =
        nameof __.SelectedWorkModel
            |> Binding.SubModel.opt WorkModel.Bindings.ToList
            |> Binding.mapModel selectedWorkModel
            |> Binding.mapMsgWithModel (fun msg model -> Msg.WorkModelMsg (model.SelectedWorkId.Value, msg))

    member val HasSelectedWork : Binding =
        nameof __.HasSelectedWork |> Binding.oneWay (selectedWorkModel >> Option.isSome)

    member val CreateWorkCommand : Binding =
        nameof __.CreateWorkCommand |> Binding.cmd Msg.CreateWork

    member val UnselectWorkCommand : Binding =
        nameof __.UnselectWorkCommand |> Binding.cmd Msg.UnselectWork

    member val LastDayCount : Binding =
        nameof __.LastDayCount |> Binding.twoWay (lastDayCountText, Msg.SetLastDayCount)


