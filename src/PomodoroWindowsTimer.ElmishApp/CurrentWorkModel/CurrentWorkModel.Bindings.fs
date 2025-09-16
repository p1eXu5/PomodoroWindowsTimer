namespace PomodoroWindowsTimer.ElmishApp.CurrentWorkModel

open System
open Elmish.WPF

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models

type IBindings =
    interface
        abstract Id: WorkId option
        abstract Number: string option
        abstract Title: string option
        abstract LastEventCreatedAt: DateTimeOffset option
        abstract UpdatedAt: DateTimeOffset option
    end

module Bindings =
    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding<CurrentWorkModel, CurrentWorkModel.Msg> list =
        [
            nameof __.Id |> Binding.oneWayOpt (_.Work >> Option.map _.Id)
            nameof __.Number |> Binding.oneWayOpt (_.Work >> Option.map _.Number)
            nameof __.Title |> Binding.oneWayOpt (_.Work >> Option.map _.Title)
            nameof __.LastEventCreatedAt |> Binding.oneWayOpt (_.Work >> Option.bind _.LastEventCreatedAt)
            nameof __.UpdatedAt |> Binding.oneWayOpt (_.Work >> Option.map _.UpdatedAt)
        ]
