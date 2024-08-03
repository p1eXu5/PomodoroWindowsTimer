module PomodoroWindowsTimer.ElmishApp.RollbackWorkModel.Bindings

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.RollbackWorkModel
open PomodoroWindowsTimer.ElmishApp.Abstractions


open Elmish.WPF

let bindings () =
    [
        // "RememberChoice" |> Binding.twoWay (_.RememberChoice, Msg.SetRememberChoice)
        "SubstractWorkAddBreakCommand" |> Binding.cmd Msg.SubstractWorkAddBreak
        "CloseCommand" |> Binding.cmd Msg.Close
    ]

