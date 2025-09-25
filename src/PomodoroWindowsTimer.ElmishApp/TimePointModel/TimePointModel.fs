namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types

type TimePointModel =
    {
        TimePoint: TimePoint
        IsSelected: bool
        IsPlayed: bool
    }
    member this.Id = this.TimePoint.Id

module TimePointModel =

    type Msg =
        | SetName of string
        | SetTimeSpan of string
        | SetIsSelected of bool
        | SetIsPlayed of bool
        | Play
        | OnExn of exn

    let init timePoint =
        {
            TimePoint = timePoint
            IsSelected = false
            IsPlayed = false
        }


namespace PomodoroWindowsTimer.ElmishApp.TimePointModel

open System
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointModel
open PomodoroWindowsTimer.Abstractions

module Program =

    open Microsoft.Extensions.Logging
    
    open Elmish

    open PomodoroWindowsTimer.ElmishApp.Abstractions
    open PomodoroWindowsTimer.ElmishApp.Logging

    let mapPlayMsg (timePointQueue: ITimePointQueue) (looper: ILooper) (model: TimePointModel) =
        model
        , Cmd.OfFunc.attempt
            (fun tpId ->
                if timePointQueue.ScrollTo tpId then
                    looper.Next()
                else
                    ()
            )
            model.Id
            Msg.OnExn

    let update (timePointQueue: ITimePointQueue) (looper: ILooper) (errorMessageQueue: IErrorMessageQueue) (logger: ILogger<TimePointModel>) msg model =
        match msg with
        | SetName v -> { model with TimePoint.Name = v }, Cmd.none
        | SetTimeSpan v -> 
            { model with TimePoint.TimeSpan = TimeSpan.ParseExact(v, "h':'mm", null) }, Cmd.none
        | SetIsSelected v ->
            { model with IsSelected = v }, Cmd.none
        | SetIsPlayed v ->
            { model with IsSelected = v }, Cmd.none
        | Play ->
            model |> mapPlayMsg timePointQueue looper
        | Msg.OnExn ex ->
            logger.LogProgramExn ex
            errorMessageQueue.EnqueueError ex.Message
            model, Cmd.none



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
        abstract PlayCommand: ICommand
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
            nameof __.PlayCommand |> Binding.cmd Msg.Play
        ]
