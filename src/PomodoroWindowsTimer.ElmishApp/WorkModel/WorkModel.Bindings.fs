namespace PomodoroWindowsTimer.ElmishApp.WorkModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkModel

type private Binding = Binding<WorkModel, WorkModel.Msg>

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

    member val Number : Binding =
        nameof __.Number |> Binding.oneWayOpt (_.Work >> _.Number)

    member val Title : Binding =
        nameof __.Title |> Binding.oneWay (_.Work >> _.Title)

    member val EditNumber : Binding =
        nameof __.EditNumber |> Binding.twoWayOpt (_.Number, Msg.SetNumber)

    member val EditTitle : Binding =
        nameof __.EditTitle |> Binding.twoWay (_.Title, Msg.SetTitle)

    member val UpdateCommand : Binding =
        nameof __.UpdateCommand
            |> Binding.cmdIf (fun m ->
                match m |> isModified, m.CreateNewState with
                | true, AsyncDeferred.NotRequested ->
                    AsyncOperation.startUnit Msg.Update |> Some
                | _ -> None
            )

    member val CreateCommand : Binding =
        nameof __.CreateCommand
            |> Binding.cmdIf (fun m ->
                match m |> isModified, m.UpdateState with
                | true, AsyncDeferred.NotRequested ->
                    AsyncOperation.startUnit Msg.Update |> Some
                | _ -> None
            )

    member val SelectCommand : Binding =
        nameof __.SelectCommand |> Binding.cmd Msg.Select

    member val EditCommand : Binding =
        nameof __.EditCommand |> Binding.cmd Msg.Edit

    member val CancelEditCommand : Binding =
        nameof __.CancelEditCommand |> Binding.cmd Msg.CancelEdit

