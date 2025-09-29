module PomodoroWindowsTimer.ElmishApp.DailyStatisticModel.Program

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
open PomodoroWindowsTimer.ElmishApp.Models.DailyStatisticModel
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure

let private storeWorkEventsTask
    (timeProvider: System.TimeProvider)
    (workEventStore: WorkEventStore)
    (date: DateOnly)
    (eventCtor: DateTimeOffset * TimeSpan -> WorkEvent)
    (breakOffsets: (WorkId * TimeSpan) list)
    ct
    =
    let rec running (l: (WorkId * TimeSpan) list) =
        task {
            match l with
            | [] -> return Ok ()
            | (workId, offset) :: tail ->
                let workEventRepository = workEventStore.GetWorkEventRepository ()
                match! workEventRepository.FindLastByWorkIdByDateAsync workId date ct with
                | Ok lastEventOpt ->
                    let time =
                        lastEventOpt
                        |> Option.map (WorkEvent.createdAt >> fun dt-> dt.AddMilliseconds(1))
                        |> Option.defaultWith (fun () -> DateTimeOffset(date, TimeOnly(0,0,0), timeProvider.LocalTimeZone.BaseUtcOffset))

                    let workEvent = eventCtor (time, offset)
                    
                    match! workEventRepository.InsertAsync workId workEvent ct with
                    | Ok _ -> return! running tail
                    | Error err -> return err |> Error
                | Error err -> return err |> Error
        }

    task {
        match! running breakOffsets with
        | Ok _ -> return breakOffsets |> Ok
        | Error err -> return err |> Error
    }


let update
    (timeProvider: System.TimeProvider)
    (workEventStore: WorkEventStore)
    (excelBook: IExcelBook)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<DailyStatisticModel>)
    msg
    (model: DailyStatisticModel)
    =
    let storeWorkEventsTask =
        storeWorkEventsTask timeProvider workEventStore

    match msg with
    // -------------------------- load DailyStatistics
    | MsgWith.``Start of LoadStatistics`` model (deff, cts) ->
        let period = model |> period

        model |> withStatistics deff
        , Cmd.OfTask.perform workEventStore.ProjectAllWorkStatisticList (period, cts.Token) (AsyncOperation.finishWithin Msg.LoadStatistics cts)
        , Intent.None

    | MsgWith.``Finish of LoadStatistics`` model res ->
        match res with
        | Ok (deff, _) ->
            model |> withStatistics deff |> withCmdNone |> withNoIntent
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withStatistics AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent

    
    // -------------------------- dialogs

    | Msg.RequestAddWorkTimeDialog workId ->
        model |> withCmdNone |> withOpenAddWorkTimeDialogIntent workId

    | Msg.RequestWorkEventListDialog workId ->
        model |> withCmdNone |> withOpenEventListDialogIntent workId

    // -------------------------- excel report
    | MsgWith.``Start of ExportToExcel`` model (deff, cts) ->
        let period =
            {
                Start = model.Day
                EndInclusive = model.Day
            }
            : DateOnlyPeriod

        let exportTask = WorkEvents.exportToExcelTask workEventStore excelBook
        model |> withExportToExcelState deff
        , Cmd.OfTask.perform (exportTask period) cts.Token (AsyncOperation.finishWithin Msg.ExportToExcel cts)
        , Intent.None

    | MsgWith.``Finish of ExportToExcel`` model res ->
        match res with
        | Ok deff ->
            model |> withExportToExcelState deff |> withCmdNone |> withNoIntent
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withExportToExcelState AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent

    // -------------------------- export events
    | MsgWith.``Start of ExportEvents`` model (deff, cts) ->
        let period =
            {
                Start = model.Day
                EndInclusive = model.Day
            }
            : DateOnlyPeriod

        let exportTask = WorkEvents.exportEventsToFileTask workEventStore
        model |> withExportEventsState deff
        , Cmd.OfTask.perform (exportTask period) cts.Token (AsyncOperation.finishWithin Msg.ExportEvents cts)
        , Intent.None

    | MsgWith.``Finish of ExportEvents`` model res ->
        match res with
        | Ok deff ->
            model |> withExportEventsState deff |> withCmdNone |> withNoIntent
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withExportEventsState AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent


    // -------------------------- allocate break
    | MsgWith.``Start of AllocateBreakTime`` model (l, overall, deff, cts) ->
        let breakOffsets = l |> breakOffsets overall
        model |> withAllocateBreakTimeState deff
        , Cmd.OfTask.perform (storeWorkEventsTask model.Day (fun (ct, v) -> WorkEvent.BreakIncreased (ct, v, None)) breakOffsets) cts.Token (AsyncOperation.finishWithin Msg.AllocateBreakTime cts)
        , Intent.None

    | MsgWith.``Finish of AllocateBreakTime`` model res ->
        match res with
        | Ok l ->
            model |> withAllocateBreakTimeState AsyncDeferred.NotRequested |> withAllocatedBreaks l |> withCmdNone |> withNoIntent
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withAllocateBreakTimeState AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent

    | MsgWith.``Start of RedoAllocateBreakTime`` model (map, deff, cts) ->
        model |> withAllocateBreakTimeState deff
        , Cmd.OfTask.perform (storeWorkEventsTask model.Day (fun (ct, v) -> WorkEvent.BreakReduced (ct, v, None)) map) cts.Token (AsyncOperation.finishWithin Msg.RedoAllocateBreakTime cts)
        , Intent.None

    | MsgWith.``Finish of RedoAllocateBreakTime`` model res ->
        match res with
        | Ok _ ->
            model |> withAllocateBreakTimeState AsyncDeferred.NotRequested |> withAllocatedBreaks [] |> withCmdNone |> withNoIntent
        | Error err ->
            do errorMessageQueue.EnqueueError err
            logger.LogError(err)
            model |> withAllocateBreakTimeState AsyncDeferred.NotRequested |> withCmdNone |> withNoIntent

    // ----------------------------

    | Msg.EnqueueExn ex ->
        errorMessageQueue.EnqueueError(ex.Message)
        model |> withCmdNone |> withNoIntent

    | _ ->
        logger.LogNonProcessedMessage(msg, model)
        model |> withCmdNone |> withNoIntent

