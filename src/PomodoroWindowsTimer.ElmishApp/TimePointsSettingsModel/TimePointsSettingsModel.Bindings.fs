module PomodoroWindowsTimer.ElmishApp.TimePointsSettingsModel.Bindings

open System
open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsSettingsModel

let bindings ()  : Binding<TimePointsSettingsModel, TimePointsSettingsModel.Msg> list =
    [
        "TimePointPrototypes" |> Binding.subModelSeq (
            (fun m -> m.TimePointPrototypes),
            (fun tp -> tp.Kind),
            Msg.TimePointPrototypeMsg,
            (fun () -> [
                "Kind" |> Binding.oneWay (fun (_, m) -> m.Kind)
                "Alias" |> Binding.oneWay (fun (_, m) -> m.Alias |> Alias.value)
                "TimeSpan" |> Binding.twoWay ((fun (_, m) -> m.TimeSpan), (fun ts _ -> TimePointPrototypeMsg.SetTimeSpan ts))
            ])
        )

        // TODO: copy from LogParser
        "Patterns" |> Binding.oneWaySeq (getPatterns, (=), id)
        "SelectedPattern" |> Binding.oneWayToSourceOpt (SetSelectedPatters)
    ]

