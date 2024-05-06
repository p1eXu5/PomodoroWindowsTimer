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
                model.WorkStatistics |> AsyncDeferred.tryInProgressWithCancellation
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

    let withStartDate startDate (model: WorkStatisticListModel) =
        { model with
            StartDate = startDate
            EndDate =
                if startDate > model.EndDate then
                    startDate
                else 
                    model.EndDate
        }

    let withEndDate endDate (model: WorkStatisticListModel) =
        { model with
            EndDate = endDate
            StartDate =
                if endDate < model.StartDate then
                    endDate
                else
                    model.StartDate
        }

    let withIsByDay isByDay (model: WorkStatisticListModel) =
        { model with
            IsByDay = isByDay;
            EndDate =
                if isByDay then
                    model.StartDate
                else
                    model.EndDate
        }

    let withStatistics deff (model: WorkStatisticListModel) =
        { model with WorkStatistics = deff }

    let workStatisticModels (model: WorkStatisticListModel) : WorkStatisticModel list =
        match model.WorkStatistics with
        | AsyncDeferred.Retrieved models -> models
        | _ -> List.empty
