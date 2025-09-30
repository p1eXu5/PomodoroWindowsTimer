namespace PomodoroWindowsTimer.ElmishApp.CreatingWorkModel

open System.Windows.Input

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.CreatingWorkModel

type IBindings =
    interface
        abstract EditNumber: string with get, set
        abstract EditTitle: string with get, set
        abstract CreateCommand: ICommand
        abstract CancelCommand: ICommand
        abstract CanBeCancelling: bool
    end

module Bindings =
    let private __ = Unchecked.defaultof<IBindings>

    let bindings () =
        [
            nameof __.EditNumber |> Binding.twoWay (_.Number, Msg.SetNumber)
            nameof __.EditTitle |> Binding.twoWay (_.Title, Msg.SetTitle)
            nameof __.CreateCommand
                |> Binding.cmdIf (fun m ->
                    match m.CreatingState with
                    | AsyncDeferred.NotRequested ->
                        AsyncOperation.startUnit Msg.CreateWork |> Some
                    | _ -> None
                )
            nameof __.CancelCommand |> Binding.cmd Msg.Cancel
            nameof __.CanBeCancelling |> Binding.oneWay _.CanBeCancelling
        ]

