namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions

type DailyStatisticListModel =
    {
        DailyStatistics: AsyncDeferred<DailyStatisticModel list>
        StartDate: DateOnly
        EndDate: DateOnly
        IsByDay: bool
        ExportToExcelState: AsyncDeferred<unit>
        AddWorkTime: AddWorkTimeModel option
        WorkEvents: WorkEventListModel option
    }

module DailyStatisticListModel =

    open PomodoroWindowsTimer.Types
    open PomodoroWindowsTimer.Abstractions
    open PomodoroWindowsTimer.ElmishApp.Abstractions

    type Msg =
        | LoadDailyStatistics of AsyncOperation<unit, Result<DailyStatistic list, string>>
        | DailyStatisticModelMsg of DateOnly * DailyStatisticModel.Msg
        | ExportToExcel of AsyncOperation<unit, Result<unit, string>>
        | SetStartDate of DateOnly
        | SetEndDate of DateOnly
        | SetIsByDay of bool
        | AddWorkTimeDialogMsg of AddWorkTimeDialogMsg
        | WorkEventListDialogMsg of WorkEventListDialogMsg
        | AllocateBreakTime
        | RedoAllocateBreakTime
        | Close
        | EnqueueExn of exn
    and
        [<RequireQualifiedAccess>]
        AddWorkTimeDialogMsg =
            | LoadAddWorkTimeModel of day: DateOnly * workId: uint64
            | UnloadAddWorkTimeModel
            | AddWorkTimeModelMsg of AddWorkTimeModel.Msg
            | AddWorkTimeOffset
    and
        WorkEventListDialogMsg =
            | LoadWorkEventListModel of day: DateOnly * workId: uint64
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

        // -------------------------- load DailyStatistics
        let (|``Start of LoadDailyStatistics``|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg with
            | Msg.LoadDailyStatistics (AsyncOperation.Start _) ->
                model.DailyStatistics |> AsyncDeferred.forceInProgressWithCancellation |> Some
            | _ -> None

        let (|``Finish of LoadDailyStatistics``|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg with
            | Msg.LoadDailyStatistics (AsyncOperation.Finish (res, cts)) ->
                model.DailyStatistics
                |> AsyncDeferred.chooseRetrievedResultWithin res cts
                |> Option.map (
                    Result.map (fun (_, res) ->
                        res
                        |> List.map DailyStatisticModel.init
                        |> AsyncDeferred.Retrieved
                    )
                )
            | _ -> None

        let (|DailyStatisticModelMsg|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.DailyStatistics with
            | Msg.DailyStatisticModelMsg (day, smsg), AsyncDeferred.Retrieved l ->
                (day, smsg, l) |> Some
            | _ -> None

        // -------------------------- AddWorkTimeDialog
        let (|LoadAddWorkTimeModel|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.DailyStatistics with
            | Msg.AddWorkTimeDialogMsg (AddWorkTimeDialogMsg.LoadAddWorkTimeModel (day, workId)), AsyncDeferred.Retrieved statistic ->
                statistic
                |> List.tryFind (_.Day >> (=) day)
                |> Option.bind (
                    _.WorkStatistics
                    >> AsyncDeferred.chooseRetrieved
                    >> Option.bind (List.tryFind (_.Work >> _.Id >> (=) workId))
                    >> Option.map (fun sm -> day, sm.Work.Work)
                )
            | _ -> None

        let (|UnloadAddWorkTimeModel|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.AddWorkTime with
            | Msg.AddWorkTimeDialogMsg AddWorkTimeDialogMsg.UnloadAddWorkTimeModel, Some am ->
                am |> Some
            | _ -> None

        let (|AddWorkTimeModelMsg|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.AddWorkTime with
            | Msg.AddWorkTimeDialogMsg (AddWorkTimeDialogMsg.AddWorkTimeModelMsg amsg), Some am ->
                (amsg, am) |> Some
            | _ -> None

        let (|AddWorkTimeOffset|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.AddWorkTime with
            | Msg.AddWorkTimeDialogMsg AddWorkTimeDialogMsg.AddWorkTimeOffset, Some am ->
                am |> Some
            | _ -> None

        // -------------------------- WorkEventListDialog
        let (|LoadWorkEventListModel|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.DailyStatistics with
            | Msg.WorkEventListDialogMsg (WorkEventListDialogMsg.LoadWorkEventListModel (day, workId)), AsyncDeferred.Retrieved statistic ->
                statistic
                |> List.tryFind (_.Day >> (=) day)
                |> Option.bind (
                    _.WorkStatistics
                    >> AsyncDeferred.chooseRetrieved
                    >> Option.bind (List.tryFind (_.Work >> _.Id >> (=) workId))
                    >> Option.map (fun sm -> day, sm.Work.Work)
                )
            | _ -> None

        let (|UnloadWorkEventListModel|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.WorkEvents with
            | Msg.WorkEventListDialogMsg WorkEventListDialogMsg.UnloadWorkEventListModel, Some am ->
                am |> Some
            | _ -> None

        let (|WorkEventListModelMsg|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.WorkEvents with
            | Msg.WorkEventListDialogMsg (WorkEventListDialogMsg.WorkEventListModelMsg emsg), Some em ->
                (emsg, em) |> Some
            | _ -> None


        let (|``Start of ExportToExcel``|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg with
            | Msg.ExportToExcel (AsyncOperation.Start _) ->
                model.ExportToExcelState |> AsyncDeferred.forceInProgressWithCancellation |> Some
            | _ -> None

        let (|``Finish of ExportToExcel``|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg with
            | Msg.ExportToExcel (AsyncOperation.Finish (res, cts)) ->
                model.ExportToExcelState
                |> AsyncDeferred.chooseRetrievedResultWithin res cts
                |> Option.map (Result.map fst)
            | _ -> None

        // -------------------------- break times
        let (|AllocateBreakTime|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.DailyStatistics with
            | Msg.AllocateBreakTime, AsyncDeferred.Retrieved l ->
                l
                |> List.filter DailyStatisticModel.canAllocateBreakTime
                |> Some
            | _ -> None

        let (|RedoAllocateBreakTime|_|) (model: DailyStatisticListModel) (msg: Msg) =
            match msg, model.DailyStatistics with
            | Msg.RedoAllocateBreakTime, AsyncDeferred.Retrieved l ->
                l
                |> List.filter DailyStatisticModel.canRedoAllocateBreakTime
                |> Some
            | _ -> None

    open Elmish

    let init (userSettings: IUserSettings) (timeProvider: System.TimeProvider) =
        let period =
            match userSettings.LastStatisticPeriod with
            | Some period ->
                period
            | None ->
                let nowDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().LocalDateTime)
                (nowDate, nowDate) ||> DateOnlyPeriod.create

        {
            DailyStatistics = AsyncDeferred.NotRequested
            ExportToExcelState = AsyncDeferred.NotRequested
            StartDate = period.Start
            EndDate = period.EndInclusive
            IsByDay = period |> DateOnlyPeriod.isOneDay
            AddWorkTime = None
            WorkEvents = None
        }
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadDailyStatistics)


    let period (model: DailyStatisticListModel) =
        (model.StartDate, model.EndDate) ||> DateOnlyPeriod.create


    let withStartDate (userSettings: IUserSettings) startDate (model: DailyStatisticListModel) =
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

    let withEndDate (userSettings: IUserSettings) endDate (model: DailyStatisticListModel) =
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

    let withIsByDay (userSettings: IUserSettings) isByDay (model: DailyStatisticListModel) =
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

    let withDailyStatistics deff (model: DailyStatisticListModel) =
        { model with DailyStatistics = deff }

    let withExportToExcelState deff (model: DailyStatisticListModel) =
        { model with ExportToExcelState = deff }

    let dailyStatistics (model: DailyStatisticListModel) =
        match model.DailyStatistics with
        | AsyncDeferred.Retrieved models -> models
        | _ -> List.empty

    let tryFindWorkStatisticModel (day: DateOnly) (workId: WorkId) (model: DailyStatisticListModel) =
        model.DailyStatistics
        |> AsyncDeferred.chooseRetrieved
        |> Option.bind (fun dailyStatList ->
            dailyStatList
            |> List.tryFind (_.Day >> (=) day)
            |> Option.bind (fun dailyStat ->
                dailyStat.WorkStatistics
                |> AsyncDeferred.chooseRetrieved
                |> Option.bind (List.tryFind (_.Work >> _.Id >> (=) workId))
                |> Option.map (fun workStat -> dailyStatList, dailyStat, workStat)
            )
        )

    let withAddWorkTimeModel addWorkTimeModel (model: DailyStatisticListModel) =
        { model with AddWorkTime = addWorkTimeModel }
    
    let withWorkEventListModel workEventListModel (model: DailyStatisticListModel) =
        { model with WorkEvents = workEventListModel }

    let canAllocateBreakTime (model: DailyStatisticListModel) =
        match model.DailyStatistics with
        | AsyncDeferred.Retrieved sms ->
            sms
            |> List.exists DailyStatisticModel.canAllocateBreakTime
        | _ -> false

    let canRedoAllocateBreakTime (model: DailyStatisticListModel) =
        match model.DailyStatistics with
        | AsyncDeferred.Retrieved sms ->
            sms
            |> List.exists DailyStatisticModel.canRedoAllocateBreakTime
        | _ -> false
