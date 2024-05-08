namespace PomodoroWindowsTimer.ElmishApp.Models

open Elmish.Extensions
open PomodoroWindowsTimer.Types
open System
open PomodoroWindowsTimer.ElmishApp.Abstractions

type WorkStatisticListModel =
    {
        WorkStatistics: AsyncDeferred<WorkStatisticModel list>
        StartDate: DateOnly
        EndDate: DateOnly
        IsByDay: bool
    }

module WorkStatisticListModel =

    type Msg =
        | SetStartDate of DateOnly
        | SetEndDate of DateOnly
        | SetIsByDay of bool
        | LoadStatistics of AsyncOperation<unit, Result<WorkStatistic list, string>>
        | WorkStatisticMsg of workId: uint64 * WorkStatisticModel.Msg
        | Close

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

    open Elmish

    let init (userSettings: IUserSettings) (timeProvider: System.TimeProvider) =
        let (startDate, endDate) =
            match userSettings.LastStatisticPeriod with
            | Some period ->
                (period.Start, period.EndInclusive)
            | None ->
                let nowDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime)
                (nowDate, nowDate)

        {
            WorkStatistics = AsyncDeferred.NotRequested
            StartDate = startDate
            EndDate = endDate
            IsByDay = (startDate = endDate)
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

    let workStatisticModels (model: WorkStatisticListModel) : WorkStatisticModel list =
        match model.WorkStatistics with
        | AsyncDeferred.Retrieved models -> models
        | _ -> List.empty

    

    let overallTotalTime (model: WorkStatisticListModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.map (List.choose _.Statistic >> List.map (fun s -> s.WorkTime + s.BreakTime) >> List.reduce (+))

    let workTotalTime (model: WorkStatisticListModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.map (List.choose _.Statistic >> List.map (fun s -> s.WorkTime) >> List.reduce (+))

    let breakTotalTime (model: WorkStatisticListModel) =
        model.WorkStatistics
        |> AsyncDeferred.toOption
        |> Option.map (List.choose _.Statistic >> List.map (fun s -> s.BreakTime) >> List.reduce (+))


    [<Literal>]
    let private workHoursMax = 25.0 * 4.0 * 3.0 + 25.0 + 25.0 + 15.0

    let overallTotalTimeRemains (model: WorkStatisticListModel) =
        overallTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromHours(8))

    let workTotalTimeRemains (model: WorkStatisticListModel) =
        workTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromMinutes(workHoursMax))

    let breakTotalTimeRemains (model: WorkStatisticListModel) =
        breakTotalTime model
        |> Option.map (fun ts -> ts - TimeSpan.FromMinutes(8.0 * 60.0 - workHoursMax))

