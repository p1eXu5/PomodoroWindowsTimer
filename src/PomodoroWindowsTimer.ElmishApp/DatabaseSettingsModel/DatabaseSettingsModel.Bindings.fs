namespace PomodoroWindowsTimer.ElmishApp.DatabaseSettingsModel

open System

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.DatabaseSettingsModel

type private Binding = Binding<DatabaseSettingsModel, DatabaseSettingsModel.Msg>

[<Sealed>]
type Bindings() =
    let validateDatabase (s: string) =
        [
            if String.IsNullOrWhiteSpace(s) then
                "Must be set"
            if s.Length < 4 then
                "Must has min length 4"
        ]

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

    member val Database : Binding =
        nameof __.Database
            |> Binding.twoWay ((fun m -> m.Database), SetDatabase)
            |> Binding.addValidation (fun m -> m.Database |> validateDatabase)

    member val SelectDatabaseFileCommand : Binding =
        nameof __.SelectDatabaseFileCommand |> Binding.cmd Msg.SelectDatabaseFile


    member val ApplyCommand : Binding =
        nameof __.ApplyCommand
            |> Binding.cmdIf (fun _ -> Msg.Apply |> Some
            )

    member val CancelCommand : Binding =
        nameof __.CancelCommand |> Binding.cmd Msg.Cancel


