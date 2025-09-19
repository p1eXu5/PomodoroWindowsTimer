namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types

type TimePointPrototypeModel =
    {
        Prototype: TimePointPrototype
    }

module TimePointPrototypeModel =

    type Msg =
        | SetTimeSpan of string
        | SetName of string

    let init prototype =
        {
            Prototype = prototype
        }


namespace PomodoroWindowsTimer.ElmishApp.TimePointPrototypeModel

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointPrototypeModel

module Program =

    let update msg model =
        match msg with
        | SetTimeSpan v ->
            { model with Prototype.TimeSpan = System.TimeSpan.Parse(v) }
        | SetName v ->
            { model with Prototype.Name = v }

type IBindings =
    interface
        abstract Name: string
        abstract Kind: Kind
        abstract KindAlias: string // TODO: get rid, move to views
        abstract TimeSpan: string // TODO: change to TimeSpan
    end

module Bindings =

    open Elmish.WPF

    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding<TimePointPrototypeModel, TimePointPrototypeModel.Msg> list =
        [
            nameof __.Name |> Binding.oneWay (fun m -> m.Prototype.Name)
            nameof __.Kind |> Binding.oneWay (fun m -> m.Prototype.Kind)
            nameof __.KindAlias |> Binding.oneWay (_.Prototype.Alias >> Alias.value)
            nameof __.TimeSpan |> Binding.twoWay ((fun m -> m.Prototype.TimeSpan.ToString("h':'mm")), Msg.SetTimeSpan)
        ]


