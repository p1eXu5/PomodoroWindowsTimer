namespace PomodoroWindowsTimer.ElmishApp.TimePointModel

open System
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointModel
open PomodoroWindowsTimer.Abstractions


open PomodoroWindowsTimer.Types
open System.Windows.Input

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
        abstract IsPlayed: bool
        abstract IsPlaying: bool
        abstract PlayCommand: ICommand
        abstract StopCommand: ICommand
    end

module Bindings =

    open Elmish.WPF

    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding<TimePointModel, TimePointModel.Msg> list =
        [
            nameof __.Id          |> Binding.oneWay _.TimePoint.Id
            nameof __.Name        |> Binding.twoWay (_.TimePoint.Name, Msg.SetName)
            nameof __.TimeSpan    |> Binding.twoWay (_.TimePoint.TimeSpan.ToString("h':'mm"), Msg.SetTimeSpan) |> Binding.addLazy (fun m1 m2 -> m1.TimePoint.TimeSpan = m2.TimePoint.TimeSpan)
            nameof __.Kind        |> Binding.oneWay _.TimePoint.Kind
            nameof __.KindAlias   |> Binding.oneWay (_.TimePoint.KindAlias >> Alias.value) |> Binding.addLazy (fun m1 m2 -> m1.TimePoint.KindAlias = m2.TimePoint.KindAlias)
            nameof __.IsSelected  |> Binding.twoWay (_.IsSelected, Msg.SetIsSelected)
            nameof __.IsPlayed    |> Binding.twoWay (_.IsPlayed, Msg.SetIsPlayed)
            nameof __.IsPlaying    |> Binding.twoWay (_.IsPlayed, Msg.SetIsPlaying)
            nameof __.PlayCommand |> Binding.cmd Msg.Play
            nameof __.StopCommand |> Binding.cmd Msg.Stop
        ]