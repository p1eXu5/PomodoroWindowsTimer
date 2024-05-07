namespace PomodoroWindowsTimer.ElmishApp.WorkStatisticListModel

open System

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkStatisticListModel
open PomodoroWindowsTimer.ElmishApp.Abstractions

type private Binding = Binding<WorkStatisticListModel, WorkStatisticListModel.Msg>

[<Sealed>]
type Bindings(dialogErrorMessageQueue: IErrorMessageQueue) =
    let time (timeSpan: TimeSpan) =
        // let hours = MathF.Floor(float32 timeSpan.TotalHours)
        let minutes =
            if timeSpan.Seconds <> 0 then
                timeSpan.Minutes + 1
            else
                timeSpan.Minutes
        $"{timeSpan.Hours} h  {minutes} m"

    let timeRemains (timeSpan: TimeSpan) =
        // let hours = MathF.Floor(float32 timeSpan.TotalHours)
        let minutes =
            Math.Abs(timeSpan.Minutes)

        $"{timeSpan.Hours} h  {minutes} m"

    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance(dialogErrorMessageQueue: IErrorMessageQueue) =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings(dialogErrorMessageQueue)
            __
        else __

    static member ToList dialogErrorMessageQueue =
        Utils.bindings<Binding>
            (Bindings.Instance(dialogErrorMessageQueue))
            props

    member val ErrorMessageQueue : Binding =
        nameof __.ErrorMessageQueue |> Binding.oneWay (fun _ -> dialogErrorMessageQueue)

    member val StartDate : Binding =
        nameof __.StartDate |> Binding.twoWay (_.StartDate, Msg.SetStartDate)

    member val SetStartDateCommand : Binding =
        nameof __.SetStartDateCommand |> Binding.cmdParamIf (fun o model ->
            match o with
            | :? DateTime as dt ->
                Msg.SetStartDate (DateOnly.FromDateTime(dt)) |> Some
            | _ -> None
        )

    member val EndDate : Binding =
        nameof __.EndDate |> Binding.twoWay (_.EndDate, Msg.SetEndDate)

    member val IsByDay : Binding =
        nameof __.IsByDay |> Binding.twoWay (_.IsByDay, Msg.SetIsByDay)

    member val WorkStatisticsDeff : Binding =
        nameof __.WorkStatisticsDeff |> Binding.oneWay _.WorkStatistics

    member val WorkStatistics : Binding =
        nameof __.WorkStatistics
            |> Binding.subModelSeq (WorkStatisticModel.Bindings.ToList, _.WorkId)
            |> Binding.mapModel (workStatisticModels >> List.toSeq)
            |> Binding.mapMsg Msg.WorkStatisticMsg

    member val CloseCommand : Binding =
        nameof __.CloseCommand |> Binding.cmd Msg.Close

    member val OverallTotalTime : Binding =
        nameof __.OverallTotalTime |> Binding.oneWayOpt (overallTotalTime >> Option.map time)

    member val WorkTotalTime : Binding =
        nameof __.WorkTotalTime |> Binding.oneWayOpt (workTotalTime >> Option.map time)

    member val BreakTotalTime : Binding =
        nameof __.BreakTotalTime |> Binding.oneWayOpt (breakTotalTime >> Option.map time)

    member val OverallTotalTimeRemains : Binding =
        nameof __.OverallTotalTimeRemains |> Binding.oneWayOpt (overallTotalTimeRemains >> Option.map timeRemains)

    member val WorkTotalTimeRemains : Binding =
        nameof __.WorkTotalTimeRemains |> Binding.oneWayOpt (workTotalTimeRemains >> Option.map timeRemains)

    member val BreakTotalTimeRemains : Binding =
        nameof __.BreakTotalTimeRemains |> Binding.oneWayOpt (breakTotalTimeRemains >> Option.map timeRemains)

