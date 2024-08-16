namespace PomodoroWindowsTimer.ElmishApp

open System
open System.Threading.Tasks
open System.Threading

open PomodoroWindowsTimer.Types

type WorkEventStore =
    {
        StoreStartedWorkEventTask:    WorkId * DateTimeOffset * ActiveTimePoint -> Task<unit>
        StoreStoppedWorkEventTask:    WorkId * DateTimeOffset * ActiveTimePoint -> Task<unit>
        StoreWorkReducedEventTask:    WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        StoreBreakReducedEventTask:   WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        StoreBreakIncreasedEventTask: WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        StoreWorkIncreasedEventTask:  WorkId * DateTimeOffset * TimeSpan -> Task<unit>
        WorkSpentTimeListTask:   TimePointId * Kind * DateTimeOffset * float<sec> * CancellationToken -> Task<Result<WorkSpentTime list, string>>
    }


module WorkEventStore =

    open FsToolkit.ErrorHandling
    open PomodoroWindowsTimer.Abstractions

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


    let private workSpentTimeListTask
        (workEventRepository: IWorkEventRepository)
        (activeTimePointId: TimePointId, activeTimePointKind: Kind, notAfterDate: DateTimeOffset, diff: float<sec>, cancellationToken: CancellationToken)
        : Task<Result<WorkSpentTime list, string>>
        =
        task {
            let! res = workEventRepository.FindByActiveTimePointIdByDateAsync activeTimePointId notAfterDate cancellationToken

            let spentTime = spentTime (notAfterDate.Subtract(TimeSpan.FromSeconds(float diff)))

            match res with
            | Error err -> return Error $"Failed to obtain work events. {err}"
            | Ok workEventLists ->
                return
                    workEventLists
                    |> List.traverseResultM (fun wel ->
                        let (filterA, filterB) =
                            if activeTimePointKind |> Kind.isWork then
                                WorkEvent.filterWorkStartStopped, WorkEvent.filterWorkIncreasedReduced
                            else
                                WorkEvent.filterBreakStartStopped, WorkEvent.filterBreakIncreasedReduced

                        wel.Events
                        |> List.filter filterA
                        |> spentTime
                        |> Result.map (fun timeSpent ->
                            match wel.Events |> List.filter filterB |> changeTime with
                            | Some t ->
                                ({ Work = wel.Work; SpentTime = timeSpent}, t)
                            | None -> 
                                ({ Work = wel.Work; SpentTime = timeSpent}, TimeSpan.Zero)
                        |> Result.mapError (fun err -> $"Failed to calculate spent time for {wel.Work.Id} work. {err}")
                    )
                    |> Result.map (fun l ->
                        let startStopSum = l |> List.sumBy (fst >> _.SpentTime)
                        let increasedReducedExist = l |> List.exists (snd >> (<) TimeSpan.Zero)

                        if startStopSum.TotalSeconds < (float diff) && increasedReducedExist then
                            let rec substract l remaining res =

                        else



                    )
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
