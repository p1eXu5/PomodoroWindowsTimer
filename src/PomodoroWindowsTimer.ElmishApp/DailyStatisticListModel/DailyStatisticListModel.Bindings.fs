namespace PomodoroWindowsTimer.ElmishApp.DailyStatisticListModel

open System

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.DailyStatisticListModel

type private Binding = Binding<DailyStatisticListModel, DailyStatisticListModel.Msg>

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

    member val CloseCommand : Binding =
        nameof __.CloseCommand |> Binding.cmd Msg.Close

    // ----------------------- statistics
    member val LoadDailyStatisticsCommand : Binding =
        nameof __.LoadDailyStatisticsCommand |> Binding.cmd (AsyncOperation.startUnit Msg.LoadDailyStatistics)

    member val DailyStatisticsDeff : Binding =
        nameof __.DailyStatisticsDeff |> Binding.oneWay _.DailyStatistics

    member val DailyStatistics : Binding =
        nameof __.DailyStatistics
            |> Binding.subModelSeq ((fun () -> DailyStatisticModel.Bindings.ToList(dialogErrorMessageQueue)), _.Day)
            |> Binding.mapModel (dailyStatistics >> List.toSeq)
            |> Binding.mapMsg Msg.DailyStatisticModelMsg

    // ----------------------- period
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

    // ---------------- add work time dialog

    member val AddWorkTimeDialog : Binding =
        nameof __.AddWorkTimeDialog
            |> Binding.SubModel.opt (AddWorkTimeModel.Bindings.ToList)
            |> Binding.mapModel _.AddWorkTime
            |> Binding.mapMsg (AddWorkTimeDialogMsg.AddWorkTimeModelMsg >> Msg.AddWorkTimeDialogMsg)

    member val UnloadAddWorkTimeModelCommand : Binding =
        nameof __.UnloadAddWorkTimeModelCommand
            |> Binding.cmdIf (_.AddWorkTime >> Option.map (fun _ -> AddWorkTimeDialogMsg.UnloadAddWorkTimeModel |> Msg.AddWorkTimeDialogMsg))

    member val AddWorkTimeOffsetCommand : Binding =
        nameof __.AddWorkTimeOffsetCommand |> Binding.cmdIf (_.AddWorkTime >> Option.map (fun _ -> AddWorkTimeDialogMsg.AddWorkTimeOffset |> Msg.AddWorkTimeDialogMsg))

    // ---------------- work event list dialog

    member val WorkEventListDialog : Binding =
        nameof __.WorkEventListDialog
            |> Binding.SubModel.opt (WorkEventListModel.Bindings.ToList)
            |> Binding.mapModel _.WorkEvents
            |> Binding.mapMsg (WorkEventListDialogMsg.WorkEventListModelMsg >> Msg.WorkEventListDialogMsg)

    member val UnloadWorkEventListModelCommand : Binding =
        nameof __.UnloadWorkEventListModelCommand
            |> Binding.cmd (WorkEventListDialogMsg.UnloadWorkEventListModel |> Msg.WorkEventListDialogMsg)


    // ----------------------------

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
                if m |> canAllocateBreakTime then Msg.AllocateBreakTime |> Some else None
            )

    member val RedoAllocateBreakTimeCommand : Binding =
        nameof __.RedoAllocateBreakTimeCommand
            |> Binding.cmdIf (fun m ->
                if m |> canRedoAllocateBreakTime then Msg.RedoAllocateBreakTime |> Some else None
            )

    member val CanAllocateBreakTime : Binding =
        nameof __.CanAllocateBreakTime |> Binding.oneWay canAllocateBreakTime

    member val CanRedoAllocateBreakTime : Binding =
        nameof __.CanRedoAllocateBreakTime |> Binding.oneWay canRedoAllocateBreakTime

