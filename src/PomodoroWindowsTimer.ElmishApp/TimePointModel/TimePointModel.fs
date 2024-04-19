namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types

type TimePointModel =
    {
        TimePoint: TimePoint
    }

module TimePointModel =

    type Msg =
        | SetName of string

    let init timePoint =
        {
            TimePoint = timePoint
        }


namespace PomodoroWindowsTimer.ElmishApp.TimePointModel

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointModel

module Program =

    let update msg model =
        match msg with
        | SetName v ->
            { model with TimePoint.Name = v }

module Bindings =

    open Elmish.WPF
    open PomodoroWindowsTimer.Types

    let bindings () : Binding<TimePointModel, TimePointModel.Msg> list =
        [
            "Name" |> Binding.twoWay (_.TimePoint.Name, Msg.SetName)
            "TimeSpan" |> Binding.oneWay _.TimePoint.TimeSpan.ToString("h':'mm")
            "Kind" |> Binding.oneWay _.TimePoint.Kind
            "KindAlias" |> Binding.oneWay _.TimePoint.KindAlias
            "Id" |> Binding.oneWay _.TimePoint.Id
        ]
