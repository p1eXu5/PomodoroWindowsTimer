namespace PomodoroWindowsTimer.ElmishApp.CreatingWorkModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.CreatingWorkModel


type private Binding = Binding<CreatingWorkModel, CreatingWorkModel.Msg>

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
        nameof __.Number |> Binding.twoWayOpt (_.Number, Msg.SetNumber)

    member val Title : Binding =
        nameof __.Title |> Binding.twoWay (_.Title, Msg.SetTitle)

    member val CreateCommand : Binding =
        nameof __.CreateCommand
            |> Binding.cmdIf (fun m ->
                match m.CreatingState with
                | AsyncDeferred.NotRequested ->
                    AsyncOperation.startUnit Msg.CreateWork |> Some
                | _ -> None
            )

    member val Cancel : Binding =
        nameof __.Cancel |> Binding.cmd Msg.Cancel

