module PomodoroWindowsTimer.ElmishApp.SettingsModel.Bindings

open Elmish.WPF
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

let bindings () =
    [
        "BotSettingsModel"
            |> Binding.SubModel.required BotSettingsModel.Bindings.bindings
            |> Binding.mapModel (fun m ->
                m.BotSettingsModel
            )
            |> Binding.mapMsg SettingsModel.Msg.BotSettingsModelMsg

        "TimePointsSettingsModel"
            |> Binding.SubModel.required TimePointsSettingsModel.Bindings.bindings
            |> Binding.mapModel (fun m ->
                m.TimePointsSettingsModel
            )
            |> Binding.mapMsg SettingsModel.Msg.TimePointsSettingsModelMsg
    ]