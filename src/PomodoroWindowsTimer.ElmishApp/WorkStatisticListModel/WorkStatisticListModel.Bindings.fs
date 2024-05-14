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
        nameof __.OverallTotalTime |> Binding.oneWayOpt overallTotalTime

    member val WorkTotalTime : Binding =
        nameof __.WorkTotalTime |> Binding.oneWayOpt workTotalTime

    member val BreakTotalTime : Binding =
        nameof __.BreakTotalTime |> Binding.oneWayOpt breakTotalTime

    member val OverallTimeRemains : Binding =
        nameof __.OverallTimeRemains |> Binding.oneWayOpt overallTimeRemains

    member val WorkTimeRemains : Binding =
        nameof __.WorkTimeRemains |> Binding.oneWayOpt workTimeRemains

    member val BreakTimeRemains : Binding =
        nameof __.BreakTimeRemains |> Binding.oneWayOpt breakTimeRemains

    member val OverallAtParTime : Binding =
        nameof __.OverallAtParTime |> Binding.oneWay overallAtParTime

    member val WorkAtParTime : Binding =
        nameof __.WorkAtParTime |> Binding.oneWay workAtParTime

    member val BreakAtParTime : Binding =
        nameof __.BreakAtParTime |> Binding.oneWay breakAtParTime

    member val LoadAddWorkTimeModelCommand : Binding =
        nameof __.LoadAddWorkTimeModelCommand
            |> Binding.cmdParam (fun p ->
                Msg.LoadAddWorkTimeModel (p :?> uint64)
            )

    member val AddWorkTimeDialog : Binding =
        nameof __.AddWorkTimeDialog
            |> Binding.SubModel.opt (AddWorkTimeModel.Bindings.ToList)
            |> Binding.mapModel _.AddWorkTime
            |> Binding.mapMsg Msg.AddWorkTimeModelMsg

    member val UnloadAddWorkTimeModelCommand : Binding =
        nameof __.UnloadAddWorkTimeModelCommand
            |> Binding.cmdIf (_.AddWorkTime >> Option.map (fun _ -> Msg.UnloadAddWorkTimeModel))

    member val AddWorkTimeOffsetCommand : Binding =
        nameof __.AddWorkTimeOffsetCommand |> Binding.cmdIf (_.AddWorkTime >> Option.map (fun _ -> Msg.AddWorkTimeOffset))

