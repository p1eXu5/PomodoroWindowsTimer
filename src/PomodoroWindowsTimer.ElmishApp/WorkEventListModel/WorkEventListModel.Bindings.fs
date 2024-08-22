namespace PomodoroWindowsTimer.ElmishApp.WorkEventListModel

open Elmish.Extensions
open Elmish.WPF

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkEventListModel


type private Binding = Binding<WorkEventListModel, WorkEventListModel.Msg>

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

    member val WorkEvents : Binding =
        nameof __.WorkEvents
            |> Binding.subModelSeq (WorkEventModel.Bindings.ToList, WorkEventModel.createdAt)
            |> Binding.mapModel (workEventModels >> List.toSeq)
            |> Binding.mapMsg (fun _ -> Msg.NoMsg)
