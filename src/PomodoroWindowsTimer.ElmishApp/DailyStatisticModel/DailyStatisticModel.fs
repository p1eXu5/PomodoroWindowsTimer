namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open System.Diagnostics

type DailyStatisticModel =
    {
        Day: DateOnly
        WorkStatistics: AsyncDeferred<WorkStatisticModel list>
        ExportToExcelState: AsyncDeferred<unit>
        AddWorkTime: AddWorkTimeModel option
        AllocateBreakTimeState: AsyncDeferred<unit>
        AllocatedBreaks: (WorkId * TimeSpan) list
    }

module DailyStatisticModel =

    [<RequireQualifiedAccess>]
    type Msg =
        | LoadStatistics of AsyncOperation<unit, Result<WorkStatistic list, string>>
        | ExportToExcel of AsyncOperation<unit, Result<unit, string>>
        | WorkStatisticMsg of workId: uint64 * WorkStatisticModel.Msg
        | RequestAddWorkTimeDialog of workId: uint64
        | RequestWorkEventListDialog of workId: uint64
        | AllocateBreakTime of AsyncOperation<unit, Result<(WorkId * TimeSpan) list, string>>
        | RedoAllocateBreakTime of AsyncOperation<unit, Result<(WorkId * TimeSpan) list, string>>
        | EnqueueExn of exn

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | OpenAddWorkTimeDialog of DateOnly * WorkId
        | OpenEventListDialog of DateOnly * WorkId

    
    [<Literal>]
    let private overallMinutesPerDayMax = 8 * 60

    [<Literal>]
    let private workMinutesPerDayMax = 25 * 4 * 3 + 25 + 25 + 15

    [<Literal>]
    let private breakMinutesPerDayMax = overallMinutesPerDayMax - workMinutesPerDayMax

    [<AutoOpen>]
    module Intent =
        let withNoIntent (model, cmd) =
            (model, cmd, Intent.None)

        let withOpenAddWorkTimeDialogIntent workId (model, cmd) =
            (model, cmd, Intent.OpenAddWorkTimeDialog (model.Day, workId))

        let withOpenEventListDialogIntent workId (model, cmd) =
            (model, cmd, Intent.OpenEventListDialog (model.Day, workId))

    module MsgWith =

        let (|``Start of LoadStatistics``|_|) (model: DailyStatisticModel) (msg: Msg) =
            match msg with
            | Msg.LoadStatistics (AsyncOperation.Start _) ->
                model.WorkStatistics |> AsyncDeferred.forceInProgressWithCancellation |> Some
            | _ -> None

        let (|``Finish of LoadStatistics``|_|) (model: DailyStatisticModel) (msg: Msg) =
            match msg with
            | Msg.LoadStatistics (AsyncOperation.Finish (res, cts)) ->
                model.WorkStatistics
                |> AsyncDeferred.chooseRetrievedResultWithin res cts
                |> Option.map (
                    Result.map (fun (deff, res) ->
                        (deff |> AsyncDeferred.map (List.map WorkStatisticModel.init))
                        , res
                    )
                )
            | _ -> None

        // -------------------------- export excel report
        let (|``Start of ExportToExcel``|_|) (model: DailyStatisticModel) (msg: Msg) =
            match msg with
            | Msg.ExportToExcel (AsyncOperation.Start _) ->
                model.ExportToExcelState |> AsyncDeferred.forceInProgressWithCancellation |> Some
            | _ -> None

        let (|``Finish of ExportToExcel``|_|) (model: DailyStatisticModel) (msg: Msg) =
            match msg with
            | Msg.ExportToExcel (AsyncOperation.Finish (res, cts)) ->
                model.ExportToExcelState
                |> AsyncDeferred.chooseRetrievedResultWithin res cts
                |> Option.map (Result.map fst)
            | _ -> None

        // ------------------------- break time alloc
        let (|``Start of AllocateBreakTime``|_|) (model: DailyStatisticModel) (msg: Msg) =
            match msg, model.WorkStatistics, model.AllocateBreakTimeState with
            | Msg.AllocateBreakTime (AsyncOperation.Start _), AsyncDeferred.Retrieved l, AsyncDeferred.NotRequested when model.AllocatedBreaks |> List.isEmpty ->
                let statistics =
                    l
                    |> List.choose (fun sm -> sm.Statistic |> Option.map (fun s -> (sm.WorkId, s)))

                let overall = statistics |> List.map (snd >> Statistic.total) |> List.reduce (+)

                if (Statistic.overallMinutesPerDayMax - overall) > TimeSpan.FromMinutes(1) then
                    model.AllocateBreakTimeState
                    |> AsyncDeferred.tryInProgressWithCancellation
                    |> Option.map (fun (deff, cts) -> (statistics, overall, deff, cts))
                else
                    None
            | _ -> None

        let (|``Finish of AllocateBreakTime``|_|) (model: DailyStatisticModel) (msg: Msg) =
            match msg with
            | Msg.AllocateBreakTime (AsyncOperation.Finish (res, cts)) ->
                model.AllocateBreakTimeState
                |> AsyncDeferred.chooseRetrievedResultWithin res cts
                |> Option.map (Result.map snd)
            | _ -> None


        let (|``Start of RedoAllocateBreakTime``|_|) (model: DailyStatisticModel) (msg: Msg) =
            match msg, model.WorkStatistics, model.AllocateBreakTimeState with
            | Msg.RedoAllocateBreakTime (AsyncOperation.Start _), AsyncDeferred.Retrieved l, AsyncDeferred.NotRequested when model.AllocatedBreaks |> List.isEmpty |> not ->
                model.AllocateBreakTimeState
                |> AsyncDeferred.tryInProgressWithCancellation
                |> Option.map (fun (deff, cts) -> (model.AllocatedBreaks, deff, cts))
            | _ -> None

        let (|``Finish of RedoAllocateBreakTime``|_|) (model: DailyStatisticModel) (msg: Msg) =
            match msg with
            | Msg.RedoAllocateBreakTime (AsyncOperation.Finish (res, cts)) ->
                model.AllocateBreakTimeState
                |> AsyncDeferred.chooseRetrievedResultWithin res cts
                |> Option.map (Result.map snd)
            | _ -> None


    let init (dailyStatistic: DailyStatistic) =
        let workStatistics =
            dailyStatistic.WorkStatistic
            |> List.map WorkStatisticModel.init
        {
            Day = dailyStatistic.Date
            WorkStatistics = workStatistics |> AsyncDeferred.Retrieved
            ExportToExcelState = AsyncDeferred.NotRequested
            AddWorkTime = None
            AllocateBreakTimeState = AsyncDeferred.NotRequested
            AllocatedBreaks = List.empty
        }

    let period (model: DailyStatisticModel) =
        (model.Day, model.Day) ||> DateOnlyPeriod.create

    let withStatistics deff (model: DailyStatisticModel) =
        { model with WorkStatistics = deff }

    let withExportToExcelState deff (model: DailyStatisticModel) =
        { model with ExportToExcelState = deff }

    let withAllocateBreakTimeState deff (model: DailyStatisticModel) =
        { model with AllocateBreakTimeState = deff }

    let workStatisticModels (model: DailyStatisticModel) : WorkStatisticModel list =
        match model.WorkStatistics with
        | AsyncDeferred.Retrieved models -> models
        | _ -> List.empty

    let overallTotalTime (model: DailyStatisticModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.bind (function
            | [] -> None
            | l -> l |> List.choose _.Statistic |> List.map Statistic.total |> List.reduce (+) |> Some)

    let workTotalTime (model: DailyStatisticModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.bind (function
            | [] -> None
            | l -> l |> List.choose _.Statistic |> List.map (fun s -> s.WorkTime) |> List.reduce (+) |> Some)

    let breakTotalTime (model: DailyStatisticModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.bind (function
            | [] -> None
            | l -> l |> List.choose _.Statistic |> List.map (fun s -> s.BreakTime) |> List.reduce (+) |> Some)

    let dayCount (_: DailyStatisticModel) = 1
        //(model.EndDate.DayNumber - model.StartDate.DayNumber) + 1

    let overallTimeRemains (model: DailyStatisticModel) =
        let dayCount = model |> dayCount
        overallTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromMinutes(float (overallMinutesPerDayMax * dayCount)))

    let workTimeRemains (model: DailyStatisticModel) =
        let dayCount = model |> dayCount
        workTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromMinutes(float (workMinutesPerDayMax * dayCount)))

    let breakTimeRemains (model: DailyStatisticModel) =
        let dayCount = model |> dayCount
        breakTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromMinutes(float (breakMinutesPerDayMax * dayCount)))

    let overallAtParTime (model: DailyStatisticModel) =
        let dayCount = model |> dayCount
        TimeSpan.FromMinutes(float (overallMinutesPerDayMax * dayCount))

    let workAtParTime (model: DailyStatisticModel) =
        let dayCount = model |> dayCount
        TimeSpan.FromMinutes(float (workMinutesPerDayMax * dayCount))

    let breakAtParTime (model: DailyStatisticModel) =
        let dayCount = model |> dayCount
        TimeSpan.FromMinutes(float (breakMinutesPerDayMax * dayCount))

    let withAddWorkTimeModel addWorkTimeModel (model: DailyStatisticModel) =
        { model with AddWorkTime = addWorkTimeModel }
    
    let canAllocateBreakTime (model: DailyStatisticModel) =
        match model.WorkStatistics, model.AllocateBreakTimeState with
        | AsyncDeferred.Retrieved l, AsyncDeferred.NotRequested ->
            let (overall, ``break``) = l |> WorkStatisticModel.List.dailyOverallAndBreakTimeRemains
            model.AllocatedBreaks |> List.isEmpty
            && ``break`` > TimeSpan.FromMinutes(1)
            && overall > TimeSpan.FromMinutes(1)
        | _ -> false

    let canRedoAllocateBreakTime (model: DailyStatisticModel) =
        match model.WorkStatistics, model.AllocateBreakTimeState with
        | AsyncDeferred.Retrieved _, AsyncDeferred.NotRequested ->
            model.AllocatedBreaks |> List.isEmpty |> not
        | _ -> false

    let withAllocatedBreaks allocatedBreaks (model: DailyStatisticModel) =
        let (l, op) =
            if allocatedBreaks |> List.isEmpty then
                (model.AllocatedBreaks, (-))
            else
                (allocatedBreaks, (+))

        let workStatistics =
            match model.WorkStatistics with
            | AsyncDeferred.Retrieved ws ->
                let rec running op offsets (ws: WorkStatisticModel list) (res: WorkStatisticModel list) =
                    match offsets, ws with
                    | [], _ -> (res |> List.rev) @ ws
                    | _, [] -> (res |> List.rev)
                    | _, head :: tail ->
                        match offsets |> List.tryFindIndex (fst >> (=) head.WorkId) with
                        | Some ind ->
                            let (_, offset) = offsets |> List.item ind
                            let w =
                                { head with
                                    Statistic = head.Statistic |> Option.map (fun s -> { s with BreakTime = op s.BreakTime offset } )
                                }
                            running op (offsets |> List.removeAt ind) tail (w :: res)
                        | None ->
                            running op offsets tail (head :: res)

                running op l ws []
                |> AsyncDeferred.Retrieved
            | _ -> model.WorkStatistics

        { model with AllocatedBreaks = allocatedBreaks; WorkStatistics = workStatistics }

    let breakOffsets (overall: TimeSpan) (l: (WorkId * Statistic) list) =
        let diff = Statistic.overallMinutesPerDayMax - overall
        let breakReminded =
            l |> List.map (snd >> _.BreakTime) |> List.reduce (+) |> fun b -> Statistic.breakMinutesPerDayMax - b
        let spreadTime = float (MathF.Min(float32 diff.TotalSeconds, float32 breakReminded.TotalSeconds))
        Debug.Assert(diff > TimeSpan.Zero)
        l
        |> List.map (fun (workId, statistic) ->
            let perc = statistic |> Statistic.total |> fun t -> t.TotalSeconds / overall.TotalSeconds
            (workId, TimeSpan.FromSeconds(perc * spreadTime))
        )


