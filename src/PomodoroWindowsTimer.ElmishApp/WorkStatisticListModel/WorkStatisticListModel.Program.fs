module PomodoroWindowsTimer.ElmishApp.WorkStatisticListModel.Program

open System
open System.Threading
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkStatisticListModel
open PomodoroWindowsTimer.ElmishApp.Abstractions

let storeWorkIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (date: DateOnly) (offset: TimeSpan) =
    task {
        let! lastEvent = workEventRepository.FindLastByWorkIdByDateAsync workId date CancellationToken.None

        match lastEvent with
        | Ok (Some ev) ->
            let time = ev |> WorkEvent.createdAt |> fun dt-> dt.AddMilliseconds(1) 
            let workEvent =
                WorkEvent.WorkIncreased (time, offset)

            let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        | Ok None ->
            raise (InvalidOperationException($"Work {workId} has no event on {date}"))
        | Error err ->
            raise (InvalidOperationException(err))
    }

let storeWorkReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (date: DateOnly) (offset: TimeSpan) =
    task {
        let! lastEvent = workEventRepository.FindLastByWorkIdByDateAsync workId date CancellationToken.None

        match lastEvent with
        | Ok (Some ev) ->
            let time = ev |> WorkEvent.createdAt |> fun dt-> dt.AddMilliseconds(1) 
            let workEvent =
                WorkEvent.WorkReduced (time, offset)

            let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        | Ok None ->
            raise (InvalidOperationException($"Work {workId} has no event on {date}"))
        | Error err ->
            raise (InvalidOperationException(err))
    }


let (|WorkEventListDialogMsg|_|) updateWorkEventListModel (model: WorkStatisticListModel) msg =
    match msg with
    | MsgWith.LoadWorkEventListModel model work ->
        let (workEventListModel, cmd) = WorkEventListModel.init work.Work.Id (model |> WorkStatisticListModel.dateOnlyPeriod)
        (
            model |> withWorkEventListModel (workEventListModel |> Some)
            , Cmd.map (WorkEventListDialogMsg.WorkEventListModelMsg >> Msg.WorkEventListDialogMsg) cmd
            , Intent.None
        )
        |> Some

    | MsgWith.UnloadWorkEventListModel model _ ->
        model |> withWorkEventListModel None |> withCmdNone |> withNoIntent |> Some

    | MsgWith.WorkEventListModelMsg model (amsg, am) ->
        let (workEventListModel, cmd) = updateWorkEventListModel amsg am
        (
            model |> withWorkEventListModel (workEventListModel |> Some)
            , Cmd.map (WorkEventListDialogMsg.WorkEventListModelMsg >> Msg.WorkEventListDialogMsg) cmd
            , Intent.None
        )
        |> Some
    | _ -> None


let (|AddWorkTimeDialogMsg|_|) (workEventRepo: IWorkEventRepository) (model: WorkStatisticListModel) msg =
    let storeWorkIncreasedEventTask =
        storeWorkIncreasedEventTask workEventRepo

    let storeWorkReducedEventTask =
        storeWorkReducedEventTask workEventRepo

    match msg with
    | MsgWith.LoadAddWorkTimeModel model work ->
        let addWorkTimeModel = AddWorkTimeModel.init work.Work model.EndDate
        model |> withAddWorkTimeModel (addWorkTimeModel |> Some) |> withCmdNone |> withNoIntent |> Some

    | MsgWith.UnloadAddWorkTimeModel model _ ->
        model |> withAddWorkTimeModel None |> withCmdNone |> withNoIntent |> Some

    | MsgWith.AddWorkTimeModelMsg model (amsg, am) ->
        let addWorkTimeModel = AddWorkTimeModel.Program.update amsg am
        model |> withAddWorkTimeModel (addWorkTimeModel |> Some) |> withCmdNone |> withNoIntent |> Some

    | MsgWith.AddWorkTimeOffset model am ->
        if am.TimeOffset <> TimeSpan.Zero then
            if am.IsReduce then
                let model =
                    match model.WorkStatistics with
                    | AsyncDeferred.Retrieved statistic when model.StartDate <= am.Date && am.Date <= model.EndDate ->
                        statistic
                        |> List.mapFirst (fun sm -> sm.WorkId = am.Work.Id) (fun sm -> { sm with Statistic = sm.Statistic |> Option.map (fun s -> { s with WorkTime = s.WorkTime - am.TimeOffset }) })
                        |> AsyncDeferred.Retrieved
                        |> fun s -> { model with WorkStatistics = s }
                    | _ -> model

                (
                    model |> withAddWorkTimeModel None
                    , Cmd.OfTask.attempt (storeWorkReducedEventTask am.Work.Id am.Date) am.TimeOffset Msg.EnqueueExn
                    , Intent.None
                )
                |> Some
            else
                let model =
                    match model.WorkStatistics with
                    | AsyncDeferred.Retrieved statistic when model.StartDate <= am.Date && am.Date <= model.EndDate ->
                        statistic
                        |> List.mapFirst (fun sm -> sm.WorkId = am.Work.Id) (fun sm -> { sm with Statistic = sm.Statistic |> Option.map (fun s -> { s with WorkTime = s.WorkTime + am.TimeOffset }) })
                        |> AsyncDeferred.Retrieved
                        |> fun s -> { model with WorkStatistics = s }
                    | _ -> model

                (
                    model |> withAddWorkTimeModel None
                    , Cmd.OfTask.attempt (storeWorkIncreasedEventTask am.Work.Id am.Date) am.TimeOffset Msg.EnqueueExn
                    , Intent.None
                )
                |> Some
        else
            model |> withAddWorkTimeModel None |> withCmdNone |> withNoIntent |> Some
    | _ -> None



let update
    (userSettings: IUserSettings)
    (workEventRepo: IWorkEventRepository)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<WorkStatisticListModel>)
    updateWorkEventListModel
    msg
    (model: WorkStatisticListModel) =
    match msg with
    | Msg.SetStartDate startDate when model.StartDate <> startDate ->
        model |> withStartDate userSettings startDate
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadStatistics)
        , Intent.None

    | Msg.SetEndDate endDate when model.EndDate <> endDate ->
        model |> withEndDate userSettings endDate
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadStatistics)
        , Intent.None

    | Msg.SetIsByDay v ->
        let model' = model |> withIsByDay userSettings v
        if model'.StartDate <> model.StartDate || model'.EndDate <> model.EndDate then
            model'
            , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadStatistics)
            , Intent.None
        else
            model' |> withCmdNone |> withNoIntent

    | MsgWith.``Start of LoadStatistics`` model (deff, cts) ->
        let period =
            {
                Start = model.StartDate
                EndInclusive = model.EndDate
            }
            : DateOnlyPeriod

        model |> withStatistics deff
        , Cmd.OfTask.perform (WorkEventProjector.projectAllByPeriod workEventRepo period) cts.Token (AsyncOperation.finishWithin Msg.LoadStatistics cts)
        , Intent.None

    | MsgWith.``Finish of LoadStatistics`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withStatistics AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent
        | Ok (deff, _) ->
            model |> withStatistics deff |> withCmdNone |> withNoIntent

    | Msg.Close ->
        match model.WorkStatistics with
        | AsyncDeferred.InProgress cts ->
            cts.Cancel()
        | _ -> ()
        model |> withCmdNone |> withCloseIntent

    | AddWorkTimeDialogMsg workEventRepo model (m, cmd, intent) ->
        (m, cmd, intent)

    | WorkEventListDialogMsg updateWorkEventListModel model (m, cmd, intent) ->
        (m, cmd, intent)

    | Msg.EnqueueExn ex ->
        errorMessageQueue.EnqueueError(ex.Message)
        model |> withCmdNone |> withNoIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent

