namespace PomodoroWindowsTimer.ElmishApp.TimePointsDrawerModel

open System
open Elmish.WPF

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsDrawerModel


type IBindings =
    interface
        abstract IsRunningTimePointListLoaded: bool
        abstract RunningTimePointList: RunningTimePointListModel.IBindings option

        abstract IsTimePointsGeneratorLoaded: bool
        abstract TimePointsGenerator:TimePointsGeneratorModel.IBindings option
    end

module Bindings =
    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding<TimePointsDrawerModel, TimePointsDrawerModel.Msg> list =
        [
            nameof __.IsRunningTimePointListLoaded
                |> Binding.oneWay (function TimePointsDrawerModel.RunningTimePoints _ -> true | _ -> false)

            nameof __.RunningTimePointList
                |> Binding.SubModel.opt RunningTimePointListModel.Bindings.bindings
                |> Binding.mapModel runningTimePoints
                |> Binding.mapMsg Msg.RunningTimePointsMsg

            nameof __.IsTimePointsGeneratorLoaded
                |> Binding.oneWay (function TimePointsDrawerModel.TimePointsGenerator _ -> true | _ -> false)

            nameof __.TimePointsGenerator
                |> Binding.SubModel.opt TimePointsGeneratorModel.Bindings.bindings
                |> Binding.mapModel timePointsGenerator
                |> Binding.mapMsg Msg.TimePointsGeneratorMsg
        ]

