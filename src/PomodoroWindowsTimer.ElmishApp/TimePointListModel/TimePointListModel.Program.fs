module PomodoroWindowsTimer.ElmishApp.TimePointListModel.Program

open System
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointListModel

let update msg model =
    match msg with
    | Msg.SetActiveTimePointId tpId ->
        model |> withActiveTimePointId tpId
    | _ ->
        model

