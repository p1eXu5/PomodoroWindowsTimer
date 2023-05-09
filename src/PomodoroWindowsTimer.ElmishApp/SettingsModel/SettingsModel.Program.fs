module PomodoroWindowsTimer.ElmishApp.SettingsModel.Program

open Elmish
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.SettingsModel

let update botConfiguration timePointPrototypeStore patternSettings (msg: Msg) model =
    match msg with
    | SetSelectedSettingsIndex ind ->
        match ind with
        | 0 when model.TimePointsSettingsModel |> Option.isNone ->
            let (m, cmd) = TimePointsSettingsModel.init timePointPrototypeStore patternSettings
            { model with TimePointsSettingsModel = m |> Some } |> setSelectedSettingsIndex 0 , Cmd.map TimePointsSettingsModelMsg cmd

        | 1 when model.BotSettingsModel |> Option.isNone ->
            let botSettingsModel = BotSettingsModel.init botConfiguration
            { model with  BotSettingsModel = botSettingsModel |> Some } |> setSelectedSettingsIndex 1, Cmd.none

        | ind -> model |> setSelectedSettingsIndex ind, Cmd.none

    | BotSettingsModelMsg bmsg ->
        let botSettingsModel = BotSettingsModel.Program.update botConfiguration bmsg (model.BotSettingsModel |> Option.get)
        { model with BotSettingsModel = botSettingsModel |> Some }, Cmd.none

    | TimePointsSettingsModelMsg tmsg ->
        let (timePointsSettingsModel, timePointsSettingsModelCmd) = TimePointsSettingsModel.Program.update tmsg (model.TimePointsSettingsModel |> Option.get)
        { model with TimePointsSettingsModel = timePointsSettingsModel |> Some }, Cmd.map TimePointsSettingsModelMsg timePointsSettingsModelCmd
