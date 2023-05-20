module PomodoroWindowsTimer.ElmishApp.SettingsModel.Bindings

open Elmish.WPF
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.SettingsModel

let bindings () =
    [
        "BotSettingsModel"
            |> Binding.SubModel.opt BotSettingsModel.Bindings.bindings
            |> Binding.mapModel (fun m ->
                m.BotSettingsModel
            )
            |> Binding.mapMsg Msg.BotSettingsModelMsg

        "TimePointsGenerator"
            |> Binding.SubModel.opt TimePointsGenerator.Bindings.bindings
            |> Binding.mapModel (fun m ->
                m.TimePointsGenerator
            )
            |> Binding.mapMsg Msg.TimePointsSettingsModelMsg

        "SelectedSettingsIndex" |> Binding.twoWay (getSelectedSettingsIndex, Msg.SetSelectedSettingsIndex)

        "SetTimePointsSettingsModelIndexCommand" |> Binding.cmd (Msg.SetSelectedSettingsIndex 0)
    ]