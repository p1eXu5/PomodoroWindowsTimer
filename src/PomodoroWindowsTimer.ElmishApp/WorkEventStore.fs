namespace PomodoroWindowsTimer.ElmishApp

open System
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open System.Threading

type WorkEventStore =
    {
        StoreStartedWorkEventTask: WorkId -> DateTimeOffset -> ActiveTimePoint -> Task<unit>
        StoreStoppedWorkEventTask: WorkId -> DateTimeOffset -> ActiveTimePoint -> Task<unit>
        StoreWorkReducedEventTask: WorkId -> DateTimeOffset -> TimeSpan -> Task<unit>
        StoreBreakIncreasedEventTask: WorkId -> DateTimeOffset -> TimeSpan -> Task<unit>
        WorkSpentTimeListTask: TimePointId * DateTimeOffset * float<sec> * CancellationToken -> Task<Result<WorkSpentTime list, string>>
    }


module WorkEventStore =

    open System.Threading
    open PomodoroWindowsTimer.Abstractions

    let private storeStartedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (activeTimePoint: ActiveTimePoint) =
        task {
            let workEvent =
                match activeTimePoint.Kind with
                | Kind.Break
                | Kind.LongBreak ->
                    (time, activeTimePoint.Name, activeTimePoint.Id) |> WorkEvent.BreakStarted
                | Kind.Work ->
                    (time, activeTimePoint.Name, activeTimePoint.Id) |> WorkEvent.WorkStarted

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeStoppedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (_: ActiveTimePoint) =
        task {
            let workEvent =
                time |> WorkEvent.Stopped

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeWorkReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (offset: TimeSpan) =
        task {
            let workEvent =
                WorkEvent.WorkReduced (time, offset)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeBreakIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (offset: TimeSpan) =
        task {
            let workEvent =
                WorkEvent.BreakIncreased (time, offset)

            let! res = workEventRepository.InsertAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private workSpentTimeListTask (workEventRepository: IWorkEventRepository) (timePointId: TimePointId, beforeDate: DateTimeOffset, diff: float<sec>, cancellationToken: CancellationToken) : Task<Result<WorkSpentTime list, string>> =
        task {
            return raise (NotImplementedException())
        }

    let init (workEventRepository: IWorkEventRepository) : WorkEventStore =
        {
            StoreStartedWorkEventTask = storeStartedWorkEventTask workEventRepository
            StoreStoppedWorkEventTask = storeStoppedWorkEventTask workEventRepository
            StoreWorkReducedEventTask = storeWorkReducedEventTask workEventRepository
            StoreBreakIncreasedEventTask = storeBreakIncreasedEventTask workEventRepository
            WorkSpentTimeListTask = workSpentTimeListTask workEventRepository
        }
