module PomodoroWindowsTimer.ElmishApp.CurrentWorkModel.Program

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.CurrentWorkModel
open PomodoroWindowsTimer.ElmishApp

let update
    (msg: CurrentWorkModel.Msg)
    (model: CurrentWorkModel)
    =
    match msg with
    | _ -> model, Cmd.none
