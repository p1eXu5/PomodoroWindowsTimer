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
                "TimeSpan" |> Binding.twoWay ((fun (_, m) -> m.TimeSpan.ToString("h':'mm")), (fun ts _ -> TimePointPrototypeMsg.SetTimeSpan ts))
            ])
        )

        "TimePoints" |> Binding.subModelSeq (
            (fun m -> m.TimePoints),
            (fun tp -> tp.Id),
            (fun () -> [
                "Name" |> Binding.oneWay (fun (_, e) -> e.Name)
                "TimeSpan" |> Binding.oneWay (fun (_, e) -> e.TimeSpan)
                "Kind" |> Binding.oneWay (fun (_, e) -> e.Kind)
                "Id" |> Binding.oneWay (fun (_, e) -> e.Id)
            ])
        )

        // TODO: copy from LogParser
        "Patterns" |> Binding.oneWaySeq (getPatterns, (=), id)
        "SelectedPattern"
            |> Binding.twoWayOpt ((fun m -> m.SelectedPattern), SetSelectedPattern)
            |> Binding.addValidation (fun m -> if m.IsPatternWrong then ["Wrong pattern"] else [])

        "SelectedPatternIndex" |> Binding.twoWay (getSelectedPatternIndex, SetSelectedPatternIndex)
        "IsPatternCorrect" |> Binding.oneWay (fun m -> m.IsPatternWrong |> not)
    ]

