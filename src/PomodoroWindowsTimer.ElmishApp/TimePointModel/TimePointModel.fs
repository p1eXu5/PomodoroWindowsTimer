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

open System
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointModel

module Program =

    let update msg model =
        match msg with
        | SetName v ->
            { model with TimePoint.Name = v }


open PomodoroWindowsTimer.Types

/// Design time bindings
type IBindings =
    interface
        abstract Id: TimePointId
        abstract Name: string
        abstract TimeSpan: TimeSpan
        abstract Kind: Kind
        // TODO: move to presentation
        abstract KindAlias: string
        abstract IsSelected: bool
    end

module Bindings =

    open Elmish.WPF

    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding<TimePointModel, TimePointModel.Msg> list =
        [
            nameof __.Id        |> Binding.oneWay _.TimePoint.Id
            nameof __.Name      |> Binding.twoWay (_.TimePoint.Name, Msg.SetName)
            nameof __.TimeSpan  |> Binding.oneWay _.TimePoint.TimeSpan.ToString("h':'mm")
            nameof __.Kind      |> Binding.oneWay _.TimePoint.Kind
            nameof __.KindAlias |> Binding.oneWay _.TimePoint.KindAlias
        ]
