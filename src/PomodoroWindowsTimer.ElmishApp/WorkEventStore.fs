namespace PomodoroWindowsTimer.ElmishApp

open System
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open System.Threading
open PomodoroWindowsTimer.Types.WorkEvent
open FsToolkit.ErrorHandling
open System

type WorkEventStore =
    {
        StoreStartedWorkEventTask:    WorkId * DateTimeOffset * ActiveTimePoint -> Task<unit>
        StoreStoppedWorkEventTask:    WorkId * DateTimeOffset * ActiveTimePoint -> Task<unit>
        StoreWorkReducedEventTask:    WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        StoreBreakIncreasedEventTask: WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        WorkSpentTimeListTask:   TimePointId * DateTimeOffset * float<sec> * CancellationToken -> Task<Result<WorkSpentTime list, string>>
    }


module WorkEventStore =

    open System.Threading
    open PomodoroWindowsTimer.Abstractions

    let private storeStartedWorkEventTask (workEventRepository: IWorkEventRepository) (workId: uint64, time: DateTimeOffset, activeTimePoint: ActiveTimePoint) =
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
            let workEvent =
                WorkEvent.WorkReduced (time, offset)

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

    let private spentTime (startDateOffset: DateTimeOffset) workEvents =
        let rec running workEvents state (spentTime: TimeSpan) =
            match workEvents, state with
            | [], ValueNone -> spentTime |> Ok
            | [], ValueSome _ -> 
                Error "Expecting WorkStarted or BreakStarted first event but was Stopped"
            | WorkEvent.WorkStarted (startedAt, _, _) :: tail, ValueSome stopedAt
            | WorkEvent.BreakStarted (startedAt, _, _) :: tail, ValueSome stopedAt ->
                if startedAt < startDateOffset then
                    spentTime.Add(stopedAt - startDateOffset)
                    |> running tail ValueNone
                else
                    spentTime.Add(stopedAt - startedAt)
                    |> running tail ValueNone
            | WorkEvent.Stopped stoppedAt :: tail, ValueNone ->
                spentTime |> running tail (ValueSome stoppedAt)
            | head :: _, ValueNone ->
                Error $"Unexpected {head |> WorkEvent.name} work event when next event is WorkStarted or BreakStarted event."
            | head :: _, ValueSome _ ->
                Error $"Unexpected {head |> WorkEvent.name} work event when next event is Stopped event."

        match workEvents with
        | [] -> Error "Have no work events."
        | WorkEvent.Stopped stoppedAt :: tail ->
            running tail (stoppedAt |> ValueSome) TimeSpan.Zero
        | head :: _ -> Error $"Unexpected {head |> WorkEvent.name} work event."


    let private workSpentTimeListTask (workEventRepository: IWorkEventRepository) (timePointId: TimePointId, notAfterDate: DateTimeOffset, diff: float<sec>, cancellationToken: CancellationToken) : Task<Result<WorkSpentTime list, string>> =
        task {
            let! res = workEventRepository.FindByActiveTimePointIdByDateAsync timePointId notAfterDate cancellationToken

            let spentTime = spentTime (notAfterDate.Subtract(TimeSpan.FromSeconds(float diff)))

            match res with
            | Error err -> return Error $"Failed to obtain work events. {err}"
            | Ok workEventLists ->
                return
                    workEventLists
                    |> List.traverseResultM (fun wel ->
                        wel.Events
                        |> spentTime
                        |> Result.map (fun timeSpent -> { Work = wel.Work; SpentTime = timeSpent })
                        |> Result.mapError (fun err -> $"Failed to calculate spent time for {wel.Work.Id} work. {err}")
                    )
        }

    let init (workEventRepository: IWorkEventRepository) : WorkEventStore =
        {
            StoreStartedWorkEventTask = storeStartedWorkEventTask workEventRepository
            StoreStoppedWorkEventTask = storeStoppedWorkEventTask workEventRepository
            StoreWorkReducedEventTask = storeWorkReducedEventTask workEventRepository
            StoreBreakIncreasedEventTask = storeBreakIncreasedEventTask workEventRepository
            WorkSpentTimeListTask = workSpentTimeListTask workEventRepository
        }
