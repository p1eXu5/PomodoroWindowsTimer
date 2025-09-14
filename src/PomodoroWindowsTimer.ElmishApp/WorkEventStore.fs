namespace PomodoroWindowsTimer.ElmishApp

open System
open System.Threading.Tasks
open System.Threading

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

type WorkEventStore =
    {
        GetWorkRepository: unit -> IWorkRepository
        GetWorkEventRepository: unit -> IWorkEventRepository

        StoreActiveTimePointTask: ActiveTimePoint -> Task<unit>
        StoreStartedWorkEventTask:    WorkId * DateTimeOffset * ActiveTimePoint -> Task<unit>
        StoreStoppedWorkEventTask:    WorkId * DateTimeOffset * ActiveTimePoint -> Task<unit>
        StoreWorkReducedEventTask:    WorkId * DateTimeOffset * TimeSpan * TimePointId option -> Task<unit>
        StoreBreakReducedEventTask:   WorkId * DateTimeOffset * TimeSpan * TimePointId option -> Task<unit>
        StoreBreakIncreasedEventTask: WorkId * DateTimeOffset * TimeSpan * TimePointId option -> Task<unit>
        StoreWorkIncreasedEventTask:  WorkId * DateTimeOffset * TimeSpan * TimePointId option -> Task<unit>
        WorkSpentTimeListTask: TimePointId * Kind * DateTimeOffset * float<sec> * CancellationToken -> Task<Result<WorkSpentTime list, string>>

        /// TODO: rename to ProjectWorkEventOffsetTimeList
        ProjectByWorkIdByPeriod: WorkId * DateOnlyPeriod * CancellationToken -> Task<Result<WorkEventOffsetTime list, string>>
        ProjectAllWorkStatisticList: DateOnlyPeriod * CancellationToken -> Task<Result<WorkStatistic list, string>>
        ProjectDailyWorkStatisticList: DateOnlyPeriod * CancellationToken -> Task<Result<DailyStatistic list, string>>
    }


module WorkEventStore =
    let private storeActiveTimePointTask (activeTimePointRepository: IActiveTimePointRepository) (activeTimePoint: ActiveTimePoint) =
        task {
            match! activeTimePointRepository.InsertAsync activeTimePoint CancellationToken.None with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeStartedWorkEventTask
        (workEventRepository: IWorkEventRepository)
        (activeTimePointRepository: IActiveTimePointRepository)
        (workId: uint64, createdAt: DateTimeOffset, activeTimePoint: ActiveTimePoint)
        =
        task {
            let workEvent =
                match activeTimePoint.Kind with
                | Kind.Break
                | Kind.LongBreak ->
                    (createdAt, activeTimePoint.Name, activeTimePoint.Id) |> WorkEvent.BreakStarted
                | Kind.Work ->
                    (createdAt, activeTimePoint.Name, activeTimePoint.Id) |> WorkEvent.WorkStarted

            match! activeTimePointRepository.InsertIfNotExistsAsync activeTimePoint CancellationToken.None with
            | Ok _ ->
                match! workEventRepository.InsertAsync workId workEvent CancellationToken.None with
                | Ok _ -> ()
                | Error err -> raise (InvalidOperationException(err))
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeStoppedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, createdAt: DateTimeOffset, _: ActiveTimePoint) =
        task {
            let workEvent =
                createdAt |> WorkEvent.Stopped

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeWorkReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, createdAt: DateTimeOffset, spentTime: TimeSpan, atpId: TimePointId option) =
        task {
            let workEvent = WorkEvent.WorkReduced (createdAt, spentTime, atpId)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeBreakReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, createdAt: DateTimeOffset, spentTime: TimeSpan, atpId: TimePointId option) =
        task {
            let workEvent = WorkEvent.BreakReduced (createdAt, spentTime, atpId)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeBreakIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, createdAt: DateTimeOffset, spentTime: TimeSpan, atpId: TimePointId option) =
        task {
            let workEvent =
                WorkEvent.BreakIncreased (createdAt, spentTime, atpId)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeWorkIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, createdAt: DateTimeOffset, spentTime: TimeSpan, atpId: TimePointId option) =
        task {
            let workEvent =
                WorkEvent.WorkIncreased (createdAt, spentTime, atpId)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private workSpentTimeListTask
        (workEventRepository: IWorkEventRepository)
        (
            activeTimePointId: TimePointId,
            activeTimePointKind: Kind,
            notAfterDate: DateTimeOffset,
            diff: float<sec>,
            cancellationToken: CancellationToken
        ) =
            task {
                // let! events = workEventRepository.GetAsync 0 10 cancellationToken
                try
                    let! res =
                        WorkEventSpentTimeProjector.workSpentTimeListTask
                            workEventRepository
                            activeTimePointId
                            activeTimePointKind
                            notAfterDate
                            diff
                            cancellationToken
                    return Ok res
                with
                | ex -> return Error ex.Message
            }

    /// <summary>
    /// Initializes <see cref="WorkEventStore" />.
    /// </summary>
    let init (repositoryFactory: IRepositoryFactory) : WorkEventStore =
        {
            GetWorkRepository =
                repositoryFactory.GetWorkRepository

            GetWorkEventRepository =
                repositoryFactory.GetWorkEventRepository

            StoreActiveTimePointTask =
                fun activeTimePoint ->
                    let activeTimePointRepository = repositoryFactory.GetActiveTimePointRepository()
                    storeActiveTimePointTask activeTimePointRepository activeTimePoint

            StoreStartedWorkEventTask =
                fun (workId, createdAt, activeTimePoint) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    let activeTimePointRepository = repositoryFactory.GetActiveTimePointRepository()
                    storeStartedWorkEventTask workEventRepository activeTimePointRepository (workId, createdAt, activeTimePoint)

            StoreStoppedWorkEventTask =
                fun (workId, createdAt, activeTimePoint) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    storeStoppedWorkEventTask workEventRepository (workId, createdAt, activeTimePoint)

            StoreWorkReducedEventTask =
                fun (workId, createdAt, spentTime, atpId) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    storeWorkReducedEventTask workEventRepository (workId, createdAt, spentTime, atpId)

            StoreBreakReducedEventTask =
                fun (workId, createdAt, spentTime, atpId) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    storeBreakReducedEventTask workEventRepository (workId, createdAt, spentTime, atpId)

            StoreWorkIncreasedEventTask =
                fun (workId, createdAt, spentTime, atpId) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    storeWorkIncreasedEventTask workEventRepository (workId, createdAt, spentTime, atpId)

            StoreBreakIncreasedEventTask =
                fun (workId, createdAt, spentTime, atpId) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    storeBreakIncreasedEventTask workEventRepository (workId, createdAt, spentTime, atpId)

            WorkSpentTimeListTask =
                fun (
                    activeTimePointId: TimePointId,
                    activeTimePointKind: Kind,
                    notAfterDate: DateTimeOffset,
                    diff: float<sec>,
                    cancellationToken: CancellationToken
                    ) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    workSpentTimeListTask workEventRepository (activeTimePointId, activeTimePointKind, notAfterDate, diff, cancellationToken)

            ProjectByWorkIdByPeriod =
                fun (workId, period, cancellationToken) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    WorkEventOffsetTimeProjector.projectByWorkIdByPeriod workEventRepository workId period cancellationToken

            ProjectAllWorkStatisticList =
                fun (period, cancellationToken) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    WorkEventProjector.projectAllByPeriod workEventRepository period cancellationToken

            ProjectDailyWorkStatisticList =
                fun (period, cancellationToken) ->
                    let workEventRepository = repositoryFactory.GetWorkEventRepository()
                    WorkEventProjector.projectDailyByPeriod workEventRepository period cancellationToken
        }
