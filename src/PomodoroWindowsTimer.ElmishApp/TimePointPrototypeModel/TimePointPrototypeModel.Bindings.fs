module PomodoroWindowsTimer.ElmishApp.TimePointPrototypeModel.Bindings

open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointPrototypeModel


let bindings () : Binding<TimePointPrototype, TimePointPrototypeModel.Msg> list =
    [
        "Kind" |> Binding.oneWay (fun m -> m.Kind)
        "Alias" |> Binding.oneWay (fun m -> m.Kind)
        "TimeSpan" |> Binding.twoWay ((fun m -> m.TimeSpan), Msg.SetTimeSpan)
    ]

