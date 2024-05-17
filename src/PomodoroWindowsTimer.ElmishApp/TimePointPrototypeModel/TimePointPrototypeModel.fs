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

open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointPrototypeModel

module Program =

    let update msg model =
        match msg with
        | SetTimeSpan v ->
            { model with Prototype.TimeSpan = System.TimeSpan.Parse(v) }
        | SetName v ->
            { model with Prototype.Name = v }

module Bindings =

    open Elmish.WPF
    open PomodoroWindowsTimer.Types

    let bindings () : Binding<TimePointPrototypeModel, TimePointPrototypeModel.Msg> list =
        [
            "Name" |> Binding.oneWay (fun m -> m.Prototype.Name)
            "Kind" |> Binding.oneWay (fun m -> m.Prototype.Kind)
            "KindAlias" |> Binding.oneWay (_.Prototype.KindAlias >> Alias.value)
            "TimeSpan" |> Binding.twoWay ((fun m -> m.Prototype.TimeSpan.ToString("h':'mm")), Msg.SetTimeSpan)
        ]


