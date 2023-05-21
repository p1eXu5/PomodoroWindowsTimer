module PomodoroWindowsTimer.ElmishApp.SettingsModel.Program

open Elmish
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.SettingsModel

let update botConfiguration timePointPrototypeStore patternSettings (msg: Msg) model =
    match msg with
    | SetSelectedSettingsIndex ind ->
        match ind with
        | 0 when model.TimePointsGenerator |> Option.isNone ->
            let (m, cmd) = TimePointsGenerator.init timePointPrototypeStore patternSettings
            { model with TimePointsGenerator = m |> Some } |> setSelectedSettingsIndex 0 , Cmd.map TimePointsSettingsModelMsg cmd

        | 1 when model.BotSettingsModel |> Option.isNone ->
            let botSettingsModel = BotSettingsModel.init botConfiguration
            { model with  BotSettingsModel = botSettingsModel |> Some } |> setSelectedSettingsIndex 1, Cmd.none

        | ind -> model |> setSelectedSettingsIndex ind, Cmd.none

    | BotSettingsModelMsg bmsg ->
        let botSettingsModel = BotSettingsModel.Program.update botConfiguration bmsg (model.BotSettingsModel |> Option.get)
        { model with BotSettingsModel = botSettingsModel |> Some }, Cmd.none

    | TimePointsSettingsModelMsg tmsg ->
        let (timePointsSettingsModel, timePointsSettingsModelCmd, _) = TimePointsGenerator.Program.update tmsg (model.TimePointsGenerator |> Option.get)
        { model with TimePointsGenerator = timePointsSettingsModel |> Some }, Cmd.map TimePointsSettingsModelMsg timePointsSettingsModelCmd
