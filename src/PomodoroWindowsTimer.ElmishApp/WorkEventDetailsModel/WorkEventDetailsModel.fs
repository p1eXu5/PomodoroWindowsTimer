namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Types.WorkEvent


type WorkEventDetailsModel =
    {
        EventName: string
        TimePoitName: string option
        CreatedAt: DateTimeOffset
        OffsetTime: TimeSpan option
    }


module WorkEventDetailsModel =

    type Msg = Msg

    let init (eventName: string) (timePointName: string option) (createdAt: DateTimeOffset) (offsetTime: TimeSpan option) =
        {
            EventName = eventName
            TimePoitName = timePointName
            CreatedAt = createdAt
            OffsetTime = offsetTime
        }

    let eventName (model: WorkEventDetailsModel) =
        match model.TimePoitName with
        | None -> model.EventName
        | Some n -> $"{model.EventName}: \"{n}\""


namespace PomodoroWindowsTimer.ElmishApp.WorkEventDetailsModel

open System
open Elmish
open Elmish.WPF
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkEventDetailsModel

open Elmish.Extensions

type private Binding = Binding<WorkEventDetailsModel, WorkEventDetailsModel.Msg>

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

    member val EventName : Binding =
        nameof __.EventName |> Binding.oneWay eventName

    member val CreatedAt : Binding =
        nameof __.CreatedAt |> Binding.oneWay _.CreatedAt

    member val OffsetTime : Binding =
        nameof __.OffsetTime |> Binding.oneWayOpt _.OffsetTime

