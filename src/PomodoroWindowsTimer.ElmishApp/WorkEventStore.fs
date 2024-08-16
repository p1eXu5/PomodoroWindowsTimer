namespace PomodoroWindowsTimer.ElmishApp

open System
open System.Threading.Tasks
open System.Threading

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

type WorkEventStore =
    {
        StoreStartedWorkEventTask:    WorkId * DateTimeOffset * ActiveTimePoint -> Task<unit>
        StoreStoppedWorkEventTask:    WorkId * DateTimeOffset * ActiveTimePoint -> Task<unit>
        StoreWorkReducedEventTask:    WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        StoreBreakReducedEventTask:   WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        StoreBreakIncreasedEventTask: WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        StoreWorkIncreasedEventTask:  WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        WorkSpentTimeListTask: TimePointId * Kind * DateTimeOffset * float<sec> * CancellationToken -> Task<Result<WorkSpentTime list, string>>
    }


module WorkEventStore =


    let private storeStartedWorkEventTask (workEventRepository: IWorkEventRepository) (activeTimePointRepository: IActiveTimePointRepository) (workId: uint64, time: DateTimeOffset, activeTimePoint: ActiveTimePoint) =
        task {
            let workEvent =
                match activeTimePoint.Kind with
                | Kind.Break
                | Kind.LongBreak ->
                    (time, activeTimePoint.Name, activeTimePoint.Id) |> WorkEvent.BreakStarted
                | Kind.Work ->
                    (time, activeTimePoint.Name, activeTimePoint.Id) |> WorkEvent.WorkStarted

            match! activeTimePointRepository.InsertIfNotExistsAsync activeTimePoint CancellationToken.None with
            | Ok _ ->
                match! workEventRepository.InsertAsync workId workEvent CancellationToken.None with
                | Ok _ -> ()
                | Error err -> raise (InvalidOperationException(err))
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeStoppedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, time: DateTimeOffset, _: ActiveTimePoint) =
        task {
            let workEvent =
                time |> WorkEvent.Stopped

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeWorkReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, time: DateTimeOffset, offset: TimeSpan) =
        task {
            let workEvent = WorkEvent.WorkReduced (time, offset)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeBreakReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, time: DateTimeOffset, offset: TimeSpan) =
        task {
            let workEvent = WorkEvent.BreakReduced (time, offset)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeBreakIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, time: DateTimeOffset, offset: TimeSpan) =
        task {
            let workEvent =
                WorkEvent.BreakIncreased (time, offset)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeWorkIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, time: DateTimeOffset, offset: TimeSpan) =
        task {
            let workEvent =
                WorkEvent.WorkIncreased (time, offset)

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


    let init (workEventRepository: IWorkEventRepository) (activeTimePointRepository: IActiveTimePointRepository) : WorkEventStore =
        {
            StoreStartedWorkEventTask = storeStartedWorkEventTask workEventRepository activeTimePointRepository
            StoreStoppedWorkEventTask = storeStoppedWorkEventTask workEventRepository
            StoreWorkReducedEventTask = storeWorkReducedEventTask workEventRepository
            StoreBreakReducedEventTask = storeBreakReducedEventTask workEventRepository
            StoreBreakIncreasedEventTask = storeBreakIncreasedEventTask workEventRepository
            StoreWorkIncreasedEventTask = storeWorkIncreasedEventTask workEventRepository
            WorkSpentTimeListTask = workSpentTimeListTask workEventRepository
        }
