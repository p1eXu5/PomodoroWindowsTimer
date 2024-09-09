namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open PomodoroWindowsTimer.Types

type AddWorkTimeModel =
    {
        Work: Work
        Date: DateOnly
        TimeOffset: TimeSpan
        IsReduce: bool
        IsWork: bool
    }


module AddWorkTimeModel =

    type Msg =
        | SetTimeOffset of TimeSpan
        | SetDate of DateOnly
        | SetIsReduce of bool
        | SetIsWork of bool


    let init work date =
        {
            Work = work
            Date = date
            TimeOffset = TimeSpan.Zero
            IsReduce = false
            IsWork = true
        }

    let withTimeOffset v (model: AddWorkTimeModel) =
        { model with TimeOffset = v }

    let withDate v (model: AddWorkTimeModel) =
        { model with Date = v }

    let withIsReduce v (model: AddWorkTimeModel) =
        { model with IsReduce = v }

    let withIsWork v (model: AddWorkTimeModel) =
        { model with IsWork = v }


namespace PomodoroWindowsTimer.ElmishApp.AddWorkTimeModel

open System
open Elmish
open Elmish.WPF
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.AddWorkTimeModel

module Program =

    let update msg model =
        match msg with
        | Msg.SetTimeOffset v ->
            model |> withTimeOffset v
        | Msg.SetDate v ->
            model |> withDate v
        | Msg.SetIsReduce v ->
            model |> withIsReduce v
        | Msg.SetIsWork v ->
            model |> withIsWork v


type private Binding = Binding<AddWorkTimeModel, AddWorkTimeModel.Msg>

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

    member val Number : Binding =
        nameof __.Number |> Binding.oneWay (_.Work >> _.Number)

    member val Title : Binding =
        nameof __.Title |> Binding.oneWay (_.Work >> _.Title)

    member val LastEventCreatedAtOrUpdatedAt : Binding =
        nameof __.LastEventCreatedAtOrUpdatedAt |> Binding.oneWay (fun m -> m.Work |> _.LastEventCreatedAt |> Option.defaultValue m.Work.UpdatedAt)

    member val Date : Binding =
        nameof __.Date |> Binding.twoWay (_.Date, Msg.SetDate)

    member val TimeOffset : Binding =
        nameof __.TimeOffset |> Binding.twoWay (_.TimeOffset, Msg.SetTimeOffset)

    member val IsReduce : Binding =
        nameof __.IsReduce |> Binding.twoWay (_.IsReduce, Msg.SetIsReduce)

    member val IsIncrease : Binding =
        nameof __.IsIncrease |> Binding.twoWay (_.IsReduce >> not, not >> Msg.SetIsReduce)

    member val AsWork : Binding =
        nameof __.AsWork |> Binding.twoWay (_.IsWork, Msg.SetIsWork)

    member val AsBreak : Binding =
        nameof __.AsBreak |> Binding.twoWay (_.IsWork >> not, not >> Msg.SetIsWork)
