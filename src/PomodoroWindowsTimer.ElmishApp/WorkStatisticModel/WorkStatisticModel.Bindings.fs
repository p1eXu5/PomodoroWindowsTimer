namespace PomodoroWindowsTimer.ElmishApp.WorkStatisticModel

open Elmish.WPF
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkStatisticModel

type private Binding = Binding<WorkStatisticModel, WorkStatisticModel.Msg>

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

    member val WorkId : Binding =
        nameof __.WorkId |> Binding.oneWay _.WorkId

    member val WorkNumber : Binding =
        nameof __.WorkNumber |> Binding.oneWay (_.Work >> _.Number)

    member val WorkTitle : Binding =
        nameof __.WorkTitle |> Binding.oneWay (_.Work >> _.Title)

