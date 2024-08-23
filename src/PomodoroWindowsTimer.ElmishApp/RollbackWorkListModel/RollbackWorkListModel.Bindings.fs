namespace PomodoroWindowsTimer.ElmishApp.RollbackWorkListModel

open Elmish.WPF
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.RollbackWorkListModel

type private Binding = Binding<RollbackWorkListModel, RollbackWorkListModel.Msg>

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

    member val ApplyAndCloseCommand : Binding =
        nameof __.ApplyAndCloseCommand |> Binding.cmd Msg.ApplyAndClose

    member val CloseCommand : Binding =
        nameof __.CloseCommand |> Binding.cmd Msg.Close

    member val RollbackWorkModels : Binding =
        nameof __.RollbackWorkModels
            |> Binding.subModelSeq (
                _.RollbackList,
                snd,
                _.WorkId,
                Msg.RollbackWorkModelMsg,
                RollbackWorkModel.Bindings.ToList
            )
