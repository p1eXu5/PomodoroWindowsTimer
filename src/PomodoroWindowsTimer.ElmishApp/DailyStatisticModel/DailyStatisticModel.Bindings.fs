namespace PomodoroWindowsTimer.ElmishApp.DailyStatisticModel

open System

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.DailyStatisticModel
open PomodoroWindowsTimer.ElmishApp.Abstractions

type private Binding = Binding<DailyStatisticModel, DailyStatisticModel.Msg>

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

    member val Day : Binding =
        nameof __.Day |> Binding.oneWay _.Day

    // ----------------------- statistic
    member val RefreshStatisticCommand : Binding =
        nameof __.RefreshStatisticCommand |> Binding.cmd (AsyncOperation.startUnit Msg.LoadStatistics)

    member val WorkStatisticsDeff : Binding =
        nameof __.WorkStatisticsDeff |> Binding.oneWay _.WorkStatistics

    member val WorkStatistics : Binding =
        nameof __.WorkStatistics
            |> Binding.subModelSeq (WorkStatisticModel.Bindings.ToList, _.WorkId)
            |> Binding.mapModel (workStatisticModels >> List.toSeq)
            |> Binding.mapMsg Msg.WorkStatisticMsg

    // ----------------------- overall
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
                Msg.RequestAddWorkTimeDialog (p :?> uint64)
            )

    member val LoadWorkEventListModelCommand : Binding =
        nameof __.LoadWorkEventListModelCommand
            |> Binding.cmdParam (fun p ->
                Msg.RequestWorkEventListDialog (p :?> uint64)
            )

    // ------------------

    member val ExportToExcelCommand : Binding =
        nameof __.ExportToExcelCommand
            |> Binding.cmdIf (fun m ->
                match m.ExportToExcelState with
                | AsyncDeferred.InProgress _ -> None
                | _ -> AsyncOperation.startUnit Msg.ExportToExcel |> Some
            )

    member val AllocateBreakTimeCommand : Binding =
        nameof __.AllocateBreakTimeCommand
            |> Binding.cmdIf (fun m ->
                if m |> canAllocateBreakTime then
                    (AsyncOperation.startUnit Msg.AllocateBreakTime) |> Some
                else None
            )

    member val RedoAllocateBreakTimeCommand : Binding =
        nameof __.RedoAllocateBreakTimeCommand
            |> Binding.cmdIf (fun m ->
                if m |> canAllocateBreakTime then
                    None
                else (AsyncOperation.startUnit Msg.RedoAllocateBreakTime) |> Some
            )

    member val CanAllocateBreakTime : Binding =
        nameof __.CanAllocateBreakTime |> Binding.oneWay canAllocateBreakTime

    member val CanRedoAllocateBreakTime : Binding =
        nameof __.CanRedoAllocateBreakTime |> Binding.oneWay canRedoAllocateBreakTime

