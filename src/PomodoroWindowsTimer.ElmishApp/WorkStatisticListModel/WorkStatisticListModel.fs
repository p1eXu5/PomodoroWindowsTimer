namespace PomodoroWindowsTimer.ElmishApp.Models

open Elmish.Extensions
open PomodoroWindowsTimer.Types
open System
open PomodoroWindowsTimer.ElmishApp.Abstractions

type WorkStatisticListModel =
    {
        WorkStatistics: AsyncDeferred<WorkStatisticModel list>
        ExportToExcelState: AsyncDeferred<unit>
        StartDate: DateOnly
        EndDate: DateOnly
        IsByDay: bool
        AddWorkTime: AddWorkTimeModel option
        WorkEvents: WorkEventListModel option
    }

module WorkStatisticListModel =

    [<RequireQualifiedAccess>]
    type Msg =
        | SetStartDate of DateOnly
        | SetEndDate of DateOnly
        | SetIsByDay of bool
        | LoadStatistics of AsyncOperation<unit, Result<WorkStatistic list, string>>
        | ExportToExcel of AsyncOperation<unit, Result<unit, string>>
        | WorkStatisticMsg of workId: uint64 * WorkStatisticModel.Msg
        | Close
        | AddWorkTimeDialogMsg of AddWorkTimeDialogMsg
        | WorkEventListDialogMsg of WorkEventListDialogMsg
        | EnqueueExn of exn
    and
        [<RequireQualifiedAccess>]
        AddWorkTimeDialogMsg =
            | LoadAddWorkTimeModel of workId: uint64
            | UnloadAddWorkTimeModel
            | AddWorkTimeModelMsg of AddWorkTimeModel.Msg
            | AddWorkTimeOffset
    and
        WorkEventListDialogMsg =
            | LoadWorkEventListModel of workId: uint64
            | UnloadWorkEventListModel
            | WorkEventListModelMsg of WorkEventListModel.Msg

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | CloseDialogRequested

    [<AutoOpen>]
    module Intent =

        let withNoIntent (model, cmd) =
            (model, cmd, Intent.None)

        let withCloseIntent (model, cmd) =
            (model, cmd, Intent.CloseDialogRequested)

    module MsgWith =

        let (|``Start of LoadStatistics``|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg with
            | Msg.LoadStatistics (AsyncOperation.Start _) ->
                model.WorkStatistics |> AsyncDeferred.forceInProgressWithCancellation |> Some
            | _ -> None

        let (|``Finish of LoadStatistics``|_|) (model: WorkStatisticListModel) (msg: Msg) =
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

        let (|``Start of ExportToExcel``|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg with
            | Msg.ExportToExcel (AsyncOperation.Start _) ->
                model.ExportToExcelState |> AsyncDeferred.forceInProgressWithCancellation |> Some
            | _ -> None

        let (|``Finish of ExportToExcel``|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg with
            | Msg.ExportToExcel (AsyncOperation.Finish (res, cts)) ->
                model.ExportToExcelState
                |> AsyncDeferred.chooseRetrievedResultWithin res cts
                |> Option.map (Result.map fst)
            | _ -> None

        let (|LoadAddWorkTimeModel|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg, model.WorkStatistics with
            | Msg.AddWorkTimeDialogMsg (AddWorkTimeDialogMsg.LoadAddWorkTimeModel workId), AsyncDeferred.Retrieved statistic ->
                statistic
                |> List.tryFind (fun sm -> sm.WorkId = workId)
                |> Option.map (fun sm -> sm.Work)
            | _ -> None

        let (|AddWorkTimeModelMsg|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg, model.AddWorkTime with
            | Msg.AddWorkTimeDialogMsg (AddWorkTimeDialogMsg.AddWorkTimeModelMsg amsg), Some am ->
                (amsg, am) |> Some
            | _ -> None

        let (|UnloadAddWorkTimeModel|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg, model.AddWorkTime with
            | Msg.AddWorkTimeDialogMsg AddWorkTimeDialogMsg.UnloadAddWorkTimeModel, Some am ->
                am |> Some
            | _ -> None

        let (|AddWorkTimeOffset|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg, model.AddWorkTime with
            | Msg.AddWorkTimeDialogMsg AddWorkTimeDialogMsg.AddWorkTimeOffset, Some am ->
                am |> Some
            | _ -> None

        let (|LoadWorkEventListModel|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg, model.WorkStatistics with
            | Msg.WorkEventListDialogMsg (WorkEventListDialogMsg.LoadWorkEventListModel workId), AsyncDeferred.Retrieved statistic ->
                statistic
                |> List.tryFind (fun sm -> sm.WorkId = workId)
                |> Option.map (fun sm -> sm.Work)
            | _ -> None

        let (|UnloadWorkEventListModel|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg, model.WorkEvents with
            | Msg.WorkEventListDialogMsg WorkEventListDialogMsg.UnloadWorkEventListModel, Some am ->
                am |> Some
            | _ -> None

        let (|WorkEventListModelMsg|_|) (model: WorkStatisticListModel) (msg: Msg) =
            match msg, model.WorkEvents with
            | Msg.WorkEventListDialogMsg (WorkEventListDialogMsg.WorkEventListModelMsg emsg), Some em ->
                (emsg, em) |> Some
            | _ -> None


    open Elmish

    let init (userSettings: IUserSettings) (timeProvider: System.TimeProvider) =
        let (startDate, endDate) =
            match userSettings.LastStatisticPeriod with
            | Some period ->
                (period.Start, period.EndInclusive)
            | None ->
                let nowDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().LocalDateTime)
                (nowDate, nowDate)

        {
            WorkStatistics = AsyncDeferred.NotRequested
            ExportToExcelState = AsyncDeferred.NotRequested
            StartDate = startDate
            EndDate = endDate
            IsByDay = (startDate = endDate)
            AddWorkTime = None
            WorkEvents = None
        }
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadStatistics)

    let withStartDate (userSettings: IUserSettings) startDate (model: WorkStatisticListModel) =
        let endDate =
            if model.IsByDay || startDate > model.EndDate then
                startDate
            else 
                model.EndDate

        userSettings.LastStatisticPeriod <- ({ Start = startDate; EndInclusive = endDate } : DateOnlyPeriod) |> Some

        { model with
            StartDate = startDate
            EndDate = endDate
        }

    let withEndDate (userSettings: IUserSettings) endDate (model: WorkStatisticListModel) =
        let startDate =
            if model.IsByDay || endDate < model.StartDate then
                endDate
            else
                model.StartDate

        userSettings.LastStatisticPeriod <- ({ Start = startDate; EndInclusive = endDate } : DateOnlyPeriod) |> Some

        { model with
            EndDate = endDate
            StartDate = startDate
        }

    let withIsByDay (userSettings: IUserSettings) isByDay (model: WorkStatisticListModel) =
        let endDate =
            if isByDay then
                model.StartDate
            else
                model.EndDate

        userSettings.LastStatisticPeriod <- ({ Start = model.StartDate; EndInclusive = endDate } : DateOnlyPeriod) |> Some

        { model with
            IsByDay = isByDay;
            EndDate = endDate
        }

    let withStatistics deff (model: WorkStatisticListModel) =
        { model with WorkStatistics = deff }

    let withExportToExcelState deff (model: WorkStatisticListModel) =
        { model with ExportToExcelState = deff }

    let workStatisticModels (model: WorkStatisticListModel) : WorkStatisticModel list =
        match model.WorkStatistics with
        | AsyncDeferred.Retrieved models -> models
        | _ -> List.empty

    let withAddWorkTimeModel addWorkTimeModel (model: WorkStatisticListModel) =
        { model with AddWorkTime = addWorkTimeModel }
    
    let withWorkEventListModel workEventListModel (model: WorkStatisticListModel) =
        { model with WorkEvents = workEventListModel }

    let overallTotalTime (model: WorkStatisticListModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.bind (function
            | [] -> None
            | l -> l |> List.choose _.Statistic |> List.map (fun s -> s.WorkTime + s.BreakTime) |> List.reduce (+) |> Some)

    let workTotalTime (model: WorkStatisticListModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.bind (function
            | [] -> None
            | l -> l |> List.choose _.Statistic |> List.map (fun s -> s.WorkTime) |> List.reduce (+) |> Some)

    let breakTotalTime (model: WorkStatisticListModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.bind (function
            | [] -> None
            | l -> l |> List.choose _.Statistic |> List.map (fun s -> s.BreakTime) |> List.reduce (+) |> Some)


    [<Literal>]
    let private overallMinutesPerDayMax = 8 * 60

    [<Literal>]
    let private workMinutesPerDayMax = 25 * 4 * 3 + 25 + 25 + 15

    [<Literal>]
    let private breakMinutesPerDayMax = overallMinutesPerDayMax - workMinutesPerDayMax

    let dayCount (model: WorkStatisticListModel) =
        (model.EndDate.DayNumber - model.StartDate.DayNumber) + 1

    let overallTimeRemains (model: WorkStatisticListModel) =
        let dayCount = model |> dayCount
        overallTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromMinutes(float (overallMinutesPerDayMax * dayCount)))

    let workTimeRemains (model: WorkStatisticListModel) =
        let dayCount = model |> dayCount
        workTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromMinutes(float (workMinutesPerDayMax * dayCount)))

    let breakTimeRemains (model: WorkStatisticListModel) =
        let dayCount = model |> dayCount
        breakTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromMinutes(float (breakMinutesPerDayMax * dayCount)))

    let overallAtParTime (model: WorkStatisticListModel) =
        let dayCount = model |> dayCount
        TimeSpan.FromMinutes(float (overallMinutesPerDayMax * dayCount))

    let workAtParTime (model: WorkStatisticListModel) =
        let dayCount = model |> dayCount
        TimeSpan.FromMinutes(float (workMinutesPerDayMax * dayCount))

    let breakAtParTime (model: WorkStatisticListModel) =
        let dayCount = model |> dayCount
        TimeSpan.FromMinutes(float (breakMinutesPerDayMax * dayCount))

    let dateOnlyPeriod (model: WorkStatisticListModel) : DateOnlyPeriod =
        {
            Start = model.StartDate
            EndInclusive = model.EndDate
        }

