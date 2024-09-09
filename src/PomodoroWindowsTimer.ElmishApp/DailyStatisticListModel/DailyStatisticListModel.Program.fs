module PomodoroWindowsTimer.ElmishApp.DailyStatisticListModel.Program

open System
open System.Threading
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.DailyStatisticListModel
open PomodoroWindowsTimer.ElmishApp.Infrastructure

let private storeIncreasedEventTask (workEventRepository: IWorkEventRepository) increasedEventCtor (workId: uint64) (date: DateOnly) (offset: TimeSpan) =
    task {
        let! lastEvent = workEventRepository.FindLastByWorkIdByDateAsync workId date CancellationToken.None

        match lastEvent with
        | Ok (Some ev) ->
            let time = ev |> WorkEvent.createdAt |> fun dt-> dt.AddMilliseconds(1) 
            let workEvent =
                increasedEventCtor (time, offset, None)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        | Ok None ->
            raise (InvalidOperationException($"Work {workId} has no event on {date}"))
        | Error err ->
            raise (InvalidOperationException(err))
    }

let private storeReducedEventTask (workEventRepository: IWorkEventRepository) reducedEventCtor (workId: uint64) (date: DateOnly) (offset: TimeSpan) =
    task {
        let! lastEvent = workEventRepository.FindLastByWorkIdByDateAsync workId date CancellationToken.None

        match lastEvent with
        | Ok (Some ev) ->
            let time = ev |> WorkEvent.createdAt |> fun dt-> dt.AddMilliseconds(1) 
            let workEvent =
                reducedEventCtor (time, offset, None)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        | Ok None ->
            raise (InvalidOperationException($"Work {workId} has no event on {date}"))
        | Error err ->
            raise (InvalidOperationException(err))
    }

let internal addWorkTimeOffset (workEventRepo: IWorkEventRepository) model am =
    let storeWorkIncreasedEventTask = storeIncreasedEventTask workEventRepo WorkEvent.WorkIncreased
    let storeWorkReducedEventTask = storeReducedEventTask workEventRepo WorkEvent.WorkReduced

    let storeBreakIncreasedEventTask = storeIncreasedEventTask workEventRepo WorkEvent.BreakIncreased
    let storeBreakReducedEventTask = storeReducedEventTask workEventRepo WorkEvent.BreakReduced

    let withOffset updateStatisticf model =
        match model.DailyStatistics with
        | AsyncDeferred.Retrieved l ->
            l
            |> List.mapFirst
                (_.Day >> (=) am.Date)
                (fun dailyStat ->
                    match dailyStat.WorkStatistics with
                    | AsyncDeferred.Retrieved l ->
                        l
                        |> List.mapFirst
                            (_.Work >> _.Id >> (=) am.Work.Id)
                            (fun workStat ->
                                { workStat with
                                    Statistic = workStat.Statistic |> Option.map updateStatisticf
                                }
                            )
                        |> AsyncDeferred.Retrieved
                        |> flip DailyStatisticModel.withStatistics dailyStat
                    | _ ->
                        dailyStat
                )
            |> AsyncDeferred.Retrieved
            |> flip withDailyStatistics model
        | _ ->
            model

    if am.TimeOffset <> TimeSpan.Zero then
        if am.IsWork then
            let withOffset op model = model |> withOffset (fun s -> { s with WorkTime = op s.WorkTime am.TimeOffset })

            if am.IsReduce then
                // model with reduced work time
                (
                    model |> withOffset (-) |> withAddWorkTimeModel None
                    , Cmd.OfTask.attempt (storeWorkReducedEventTask am.Work.Id am.Date) am.TimeOffset Msg.EnqueueExn
                    , Intent.None
                )
            
            else
                // model with increased work time
                (
                    model |> withOffset (+) |> withAddWorkTimeModel None
                    , Cmd.OfTask.attempt (storeWorkIncreasedEventTask am.Work.Id am.Date) am.TimeOffset Msg.EnqueueExn
                    , Intent.None
                )
        else
            let withOffset op model = model |> withOffset (fun s -> { s with BreakTime = op s.BreakTime am.TimeOffset })

            if am.IsReduce then
                // model with reduced work time
                (
                    model |> withOffset (-) |> withAddWorkTimeModel None
                    , Cmd.OfTask.attempt (storeBreakReducedEventTask am.Work.Id am.Date) am.TimeOffset Msg.EnqueueExn
                    , Intent.None
                )
            
            else
                // model with increased work time
                (
                    model |> withOffset (+) |> withAddWorkTimeModel None
                    , Cmd.OfTask.attempt (storeBreakIncreasedEventTask am.Work.Id am.Date) am.TimeOffset Msg.EnqueueExn
                    , Intent.None
                )
    else
        model |> withAddWorkTimeModel None |> withCmdNone |> withNoIntent


let update
    (userSettings: IUserSettings)
    (workEventRepo: IWorkEventRepository)
    (excelBook: IExcelBook)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<DailyStatisticListModel>)
    updateDailyStatisticModel
    updateWorkEventListModel
    msg model
    =
    match msg with
    // -------------------------- period
    | Msg.SetStartDate startDate when model.StartDate <> startDate ->
        model |> withStartDate userSettings startDate
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadDailyStatistics)
        , Intent.None

    | Msg.SetEndDate endDate when model.EndDate <> endDate ->
        model |> withEndDate userSettings endDate
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadDailyStatistics)
        , Intent.None

    | Msg.SetIsByDay v ->
        let model' = model |> withIsByDay userSettings v
        if model'.StartDate <> model.StartDate || model'.EndDate <> model.EndDate then
            model'
            , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadDailyStatistics)
            , Intent.None
        else
            model' |> withCmdNone |> withNoIntent

    // -------------------------- load DailyStatistics
    | MsgWith.``Start of LoadDailyStatistics`` model (deff, cts) ->
        let period = model |> period

        model |> withDailyStatistics deff
        , Cmd.OfTask.perform (WorkEventProjector.projectDailyByPeriod workEventRepo period) cts.Token (AsyncOperation.finishWithin Msg.LoadDailyStatistics cts)
        , Intent.None

    | MsgWith.``Finish of LoadDailyStatistics`` model res ->
        match res with
        | Ok deff ->
            model |> withDailyStatistics deff |> withCmdNone |> withNoIntent
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withDailyStatistics AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent

    | MsgWith.DailyStatisticModelMsg model (day, smsg, ms) ->
        let (ms, cmd, intent) = ms |> List.mapFirstCmdIntent (_.Day >> (=) day) (updateDailyStatisticModel smsg) DailyStatisticModel.Intent.None

        let model = (model |> withDailyStatistics (ms |> AsyncDeferred.Retrieved))
        let cmd = Cmd.map (fun sm -> Msg.DailyStatisticModelMsg (day, sm)) cmd
        
        match intent with
        | DailyStatisticModel.Intent.None -> model, cmd, Intent.None
        | DailyStatisticModel.Intent.OpenAddWorkTimeDialog (day, workId) ->
            model,
            Cmd.batch [
                cmd;
                Cmd.ofMsg (
                    Msg.AddWorkTimeDialogMsg (AddWorkTimeDialogMsg.LoadAddWorkTimeModel (day, workId))
                )
            ],
            Intent.None
        | DailyStatisticModel.Intent.OpenEventListDialog (day, workId) ->
            model,
            Cmd.batch [
                cmd;
                Cmd.ofMsg (
                    Msg.WorkEventListDialogMsg (WorkEventListDialogMsg.LoadWorkEventListModel (day, workId))
                )
            ],
            Intent.None

    // -------------------------- AddWorkTimeDialog
    | MsgWith.LoadAddWorkTimeModel model (day, work) ->
        let addWorkTimeModel = AddWorkTimeModel.init work day
        model |> withAddWorkTimeModel (addWorkTimeModel |> Some) |> withCmdNone |> withNoIntent

    | MsgWith.UnloadAddWorkTimeModel model _ ->
        model |> withAddWorkTimeModel None |> withCmdNone |> withNoIntent

    | MsgWith.AddWorkTimeModelMsg model (amsg, am) ->
        let addWorkTimeModel = AddWorkTimeModel.Program.update amsg am
        model |> withAddWorkTimeModel (addWorkTimeModel |> Some) |> withCmdNone |> withNoIntent

    | MsgWith.AddWorkTimeOffset model am -> addWorkTimeOffset workEventRepo model am

    // -------------------------- WorkEventListDialog
    | MsgWith.LoadWorkEventListModel model (day, work) ->
        let (workEventListModel, cmd) = WorkEventListModel.init work.Id ((day, day) ||> DateOnlyPeriod.create)
        (
            model |> withWorkEventListModel (workEventListModel |> Some)
            , Cmd.map (WorkEventListDialogMsg.WorkEventListModelMsg >> Msg.WorkEventListDialogMsg) cmd
            , Intent.None
        )

    | MsgWith.UnloadWorkEventListModel model _ ->
        model |> withWorkEventListModel None |> withCmdNone |> withNoIntent

    | MsgWith.WorkEventListModelMsg model (amsg, am) ->
        let (workEventListModel, cmd) = updateWorkEventListModel amsg am
        (
            model |> withWorkEventListModel (workEventListModel |> Some)
            , Cmd.map (WorkEventListDialogMsg.WorkEventListModelMsg >> Msg.WorkEventListDialogMsg) cmd
            , Intent.None
        )

    // -------------------------- export excel report
    | MsgWith.``Start of ExportToExcel`` model (deff, cts) ->
        let period = model |> period
        let exportTask = ExcelReport.exportToExcelTask workEventRepo excelBook
        model |> withExportToExcelState deff
        , Cmd.OfTask.perform (exportTask period) cts.Token (AsyncOperation.finishWithin Msg.ExportToExcel cts)
        , Intent.None

    | MsgWith.``Finish of ExportToExcel`` model res ->
        match res with
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withExportToExcelState AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent
        | Ok deff ->
            model |> withExportToExcelState deff |> withCmdNone |> withNoIntent

    // -------------------------- export excel report
    | MsgWith.AllocateBreakTime model sms when sms |> List.isEmpty |> not ->
        let cmds =
            sms
            |> List.map (fun sm -> 
                Msg.DailyStatisticModelMsg (sm.Day, (AsyncOperation.startUnit DailyStatisticModel.Msg.AllocateBreakTime))
                |> Cmd.ofMsg
            )
            |> Cmd.batch

        model
        , cmds
        , Intent.None

    | MsgWith.RedoAllocateBreakTime model sms when sms |> List.isEmpty |> not ->
        let cmds =
            sms
            |> List.map (fun sm -> 
                Msg.DailyStatisticModelMsg (sm.Day, (AsyncOperation.startUnit DailyStatisticModel.Msg.RedoAllocateBreakTime))
                |> Cmd.ofMsg
            )
            |> Cmd.batch

        model
        , cmds
        , Intent.None

    // --------------------------
    | Msg.Close ->
        match model.DailyStatistics with
        | AsyncDeferred.InProgress cts ->
            cts.Cancel()
        | _ -> ()
        model |> withCmdNone |> withCloseIntent

    | Msg.EnqueueExn ex ->
        errorMessageQueue.EnqueueError(ex.Message)
        model |> withCmdNone |> withNoIntent

    | _ ->
        logger.LogUnprocessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent



