module PomodoroWindowsTimer.ElmishApp.SettingsModel.Program

open Elmish
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.SettingsModel

let update botConfiguration (msg: Msg) model =
    match msg with
    | BotSettingsModelMsg bmsg ->
        let botSettingsModel = BotSettingsModel.Program.update botConfiguration bmsg model.BotSettingsModel
        { model with BotSettingsModel = botSettingsModel }, Cmd.none

    | TimePointsSettingsModelMsg tmsg ->
        let (timePointsSettingsModel, timePointsSettingsModelCmd) = TimePointsSettingsModel.Program.update tmsg model.TimePointsSettingsModel
        { model with TimePointsSettingsModel = timePointsSettingsModel }, Cmd.map TimePointsSettingsModelMsg timePointsSettingsModelCmd