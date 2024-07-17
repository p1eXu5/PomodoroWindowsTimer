namespace PomodoroWindowsTimer.ElmishApp

open System
open System.Threading.Tasks
open PomodoroWindowsTimer.Types

type WorkEventStore =
    {
        StoreStartedWorkEventTask: WorkId -> DateTimeOffset -> TimePoint -> Task<unit>
        StoreStoppedWorkEventTask: WorkId -> DateTimeOffset -> TimePoint -> Task<unit>
        StoreWorkReducedEventTask: WorkId -> DateTimeOffset -> TimeSpan -> Task<unit>
        StoreBreakIncreasedEventTask: WorkId -> DateTimeOffset -> TimeSpan -> Task<unit>
    }


module WorkEventStore =

    open System.Threading
    open PomodoroWindowsTimer.Abstractions

    let private storeStartedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (timePoint: TimePoint) =
        task {
            let workEvent =
                match timePoint.Kind with
                | Kind.Break
                | Kind.LongBreak ->
                    (time, timePoint.Name) |> WorkEvent.BreakStarted
                | Kind.Work ->
                    (time, timePoint.Name) |> WorkEvent.WorkStarted

            let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeStoppedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (_: TimePoint) =
        task {
            let workEvent =
                time |> WorkEvent.Stopped

            let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeWorkReducedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (offset: TimeSpan) =
        task {
            let workEvent =
                WorkEvent.WorkReduced (time, offset)

            let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }

    let private storeBreakIncreasedEventTask (workEventRepository: IWorkEventRepository) (workId: uint64) (time: DateTimeOffset) (offset: TimeSpan) =
        task {
            let workEvent =
                WorkEvent.BreakIncreased (time, offset)

            let! res = workEventRepository.CreateAsync workId workEvent CancellationToken.None

            match res with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))
        }


    let init (workEventRepository: IWorkEventRepository) =
        {
            StoreStartedWorkEventTask = storeStartedWorkEventTask workEventRepository
            StoreStoppedWorkEventTask = storeStoppedWorkEventTask workEventRepository
            StoreWorkReducedEventTask = storeWorkReducedEventTask workEventRepository
            StoreBreakIncreasedEventTask = storeBreakIncreasedEventTask workEventRepository
        }
