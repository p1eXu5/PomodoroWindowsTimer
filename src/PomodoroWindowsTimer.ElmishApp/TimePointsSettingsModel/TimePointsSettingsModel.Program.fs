module PomodoroWindowsTimer.ElmishApp.TimePointsSettingsModel.Program

open Elmish
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsSettingsModel

let update msg model =
    match msg with
    | ParsePattern s ->
        model, Cmd.none

