module PomodoroWindowsTimer.ElmishApp.TimePointsGeneratorModel.Bindings

open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsGeneratorModel

let bindings ()  : Binding<TimePointsGeneratorModel, TimePointsGeneratorModel.Msg> list =
    [
        "TimePointPrototypes"
            |> Binding.subModelSeq (TimePointPrototypeModel.Bindings.bindings, _.Prototype >> _.Kind)
            |> Binding.mapModel _.TimePointPrototypes
            |> Binding.mapMsg TimePointPrototypeMsg

        "TimePoints"
            |> Binding.subModelSeq (TimePointModel.Bindings.bindings, _.TimePoint >> _.Id)
            |> Binding.mapModel _.TimePoints
            |> Binding.mapMsg TimePointMsg

        // TODO: copy from LogParser
        "Patterns" |> Binding.oneWaySeq (getPatterns, (=), id)
        "SelectedPattern"
            |> Binding.twoWayOpt ((fun m -> m.SelectedPattern), SetSelectedPattern)
            |> Binding.addValidation (fun m -> if m.IsPatternWrong then ["Wrong pattern"] else [])

        "SelectedPatternIndex" |> Binding.twoWay (getSelectedPatternIndex, SetSelectedPatternIndex)
        "IsPatternCorrect" |> Binding.oneWay (fun m -> m.IsPatternWrong |> not)

        "ApplyCommand" |> Binding.cmdIf applyMsg
    ]

