namespace PomodoroWindowsTimer.ElmishApp.Models

[<RequireQualifiedAccess>]
type WorkEventModel =
    | Work of WorkEventDetailsModel
    | Break of WorkEventDetailsModel


module WorkEventModel =

    open System
    open PomodoroWindowsTimer.Types

    type Msg = NoMsg

    [<Struct; RequireQualifiedAccess>]
    type WorkEventModelId =
        | WorkId
        | BreakId

    let init (workEvent: WorkEvent) (offsetTime: TimeSpan option) (prevIsWork: bool) lastTimePointName =
        match workEvent with
        | WorkStarted (dt, tpName) -> WorkEventDetailsModel.init "started" (tpName |> Some) dt offsetTime |> WorkEventModel.Work
        | BreakStarted (dt, tpName) -> WorkEventDetailsModel.init "started" (tpName |> Some) dt offsetTime |> WorkEventModel.Break
        | Stopped (createdAt = dt) ->
            if prevIsWork then
                WorkEventDetailsModel.init "stopped" lastTimePointName dt offsetTime |> WorkEventModel.Work
            else
                WorkEventDetailsModel.init "stopped" lastTimePointName dt offsetTime |> WorkEventModel.Break

        | WorkReduced (createdAt = dt) -> WorkEventDetailsModel.init "time reduced" None dt offsetTime |> WorkEventModel.Work
        | WorkIncreased (createdAt = dt) -> WorkEventDetailsModel.init "time increased" None dt offsetTime |> WorkEventModel.Work
        | BreakReduced (createdAt = dt) -> WorkEventDetailsModel.init "time reduced" None dt offsetTime |> WorkEventModel.Break
        | BreakIncreased (createdAt = dt) -> WorkEventDetailsModel.init "time increased" None dt offsetTime |> WorkEventModel.Break


    let workEventModelId = function
        | WorkEventModel.Work _ -> WorkEventModelId.WorkId
        | WorkEventModel.Break _ -> WorkEventModelId.BreakId


    let workEventModel = function
        | WorkEventModel.Work m -> m |> Some
        | WorkEventModel.Break _ -> None

    let breakEventModel = function
        | WorkEventModel.Break m -> m |> Some
        | WorkEventModel.Work _ -> None

    let createdAt = function
        | WorkEventModel.Break m
        | WorkEventModel.Work m -> m.CreatedAt

    let timePointName = function
        | WorkEventModel.Break m
        | WorkEventModel.Work m -> m.TimePoitName


namespace PomodoroWindowsTimer.ElmishApp.WorkEventModel

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkEventModel
open System

open Elmish.WPF

open Elmish.Extensions

type private Binding = Binding<WorkEventModel, WorkEventModel.Msg>

[<Sealed>]
type Bindings() =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance() =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings()
            __
        else __

    static member ToList () =
        Utils.bindings<Binding>
            (Bindings.Instance())
            props

    member val WorkEventModelId : Binding =
        nameof __.WorkEventModelId |> Binding.oneWay workEventModelId

    member val Work : Binding =
        nameof __.Work
            |> Binding.SubModel.opt WorkEventDetailsModel.Bindings.ToList
            |> Binding.mapModel workEventModel
            |> Binding.mapMsg (fun _ -> Msg.NoMsg)

    member val Break : Binding =
        nameof __.Break
            |> Binding.SubModel.opt WorkEventDetailsModel.Bindings.ToList
            |> Binding.mapModel breakEventModel
            |> Binding.mapMsg (fun _ -> Msg.NoMsg)



