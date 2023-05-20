module PomodoroWindowsTimer.ElmishApp.TimePointsGenerator.Bindings

open System
open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsGenerator

let bindings ()  : Binding<TimePointsGenerator, TimePointsGenerator.Msg> list =
    [
        "TimePointPrototypes" |> Binding.subModelSeq (
            (fun m -> m.TimePointPrototypes),
            (fun tp -> tp.Kind),
            Msg.TimePointPrototypeMsg,
            (fun () -> [
                "Name" |> Binding.twoWay ((fun (_, e: TimePointPrototype) -> e.Name), TimePointPrototypeMsg.SetName)
                "Kind" |> Binding.oneWay (fun (_, m) -> m.Kind)
                "KindAlias" |> Binding.oneWay (fun (_, m) -> m.KindAlias)
                "TimeSpan" |> Binding.twoWay ((fun (_, m) -> m.TimeSpan.ToString("h':'mm")), (fun ts _ -> TimePointPrototypeMsg.SetTimeSpan ts))
            ])
        )

        "TimePoints" |> Binding.subModelSeq (
            (fun m -> m.TimePoints),
            (fun tp -> tp.Id),
            Msg.TimePointMsg,
            (fun () -> [
                "Name" |> Binding.twoWay ((fun (_, e: TimePoint) -> e.Name), TimePointMsg.SetName)
                "TimeSpan" |> Binding.oneWay (fun (_, e) -> e.TimeSpan.ToString("h':'mm"))
                "Kind" |> Binding.oneWay (fun (_, e) -> e.Kind)
                "KindAlias" |> Binding.oneWay (fun (_, m) -> m.KindAlias)
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

