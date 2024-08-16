module PomodoroWindowsTimer.WorkEventSpentTimeProjector

open System
open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

let internal projectSpentTime kind (workEvents: WorkEvent list) =
    match workEvents with
    | [] -> TimeSpan.Zero
    | head::tail ->
        tail
        |> List.fold (fun (acc, lastEvent) ev ->
            match ev, lastEvent with
            | WorkEvent.Stopped endDt, WorkEvent.WorkStarted (createdAt = startDt)
            | WorkEvent.Stopped endDt, WorkEvent.BreakStarted (createdAt = startDt) ->
                (acc + (endDt - startDt), ev)
            
            | WorkEvent.WorkStarted _, WorkEvent.Stopped _
            | WorkEvent.BreakStarted _, WorkEvent.Stopped _ ->
                (acc, ev)

            | WorkEvent.BreakReduced (value = v), _ when kind |> Kind.isBreak ->
                (acc - v, lastEvent)

            | WorkEvent.BreakIncreased _, _ when kind |> Kind.isBreak ->
                (acc, lastEvent)

            | WorkEvent.WorkReduced (value = v), _ when kind |> Kind.isWork ->
                (acc - v, lastEvent)

            | WorkEvent.WorkIncreased _, _ when kind |> Kind.isWork ->
                (acc, lastEvent)

            | _ ->
                raise (ArgumentException($"Unpredictable event order. Current: {ev}, previous: {lastEvent}, kind: {kind}"))

        ) (TimeSpan.Zero, head)
        |> fst

let workSpentTimeListTask
    (workEventRepository: IWorkEventRepository)
    (activeTimePointId: TimePointId)
    (activeTimePointKind: Kind)
    (notAfterDate: DateTimeOffset)
    (diff: float<sec>)
    (cancellationToken: CancellationToken)
    : Task<WorkSpentTime list> =
    task {
        let! res = workEventRepository.FindByActiveTimePointIdByDateAsync activeTimePointId activeTimePointKind notAfterDate cancellationToken

        match res with
        | Error err -> return raise (InvalidOperationException $"Failed to obtain work events. {err}")
        | Ok workEventLists ->
            let spentTimes =
                workEventLists
                |> List.map (fun wel ->
                    {
                        Work = wel.Work
                        SpentTime = wel.Events |> projectSpentTime activeTimePointKind
                    }
                )

            let spentTimes =
                let increaseLast spentTimes v =
                    let remaining =
                        spentTimes
                        |> List.filter (_.SpentTime >> fun t -> t > TimeSpan.Zero )
                        |> List.map _.SpentTime
                        |> List.reduce (+)
                        |> fun s -> (float diff) - s.TotalSeconds

                    let lastSpentTime = spentTimes |> List.last

                    spentTimes
                    |> List.take (spentTimes.Length - 1)
                    |> fun l -> l @ [ 
                        { lastSpentTime with
                            SpentTime = TimeSpan.FromSeconds(Math.Max((lastSpentTime.SpentTime + v).TotalSeconds, remaining))
                        }
                    ]

                let rec findLastIncreased l =
                    match l with
                    | [] -> None
                    | head :: tail ->
                        match head with
                        | WorkEvent.Stopped _
                        | WorkEvent.WorkStarted _
                        | WorkEvent.BreakStarted _ ->
                            findLastIncreased tail
                        | WorkEvent.BreakIncreased _ when activeTimePointKind |> Kind.isBreak -> head |> Some
                        | WorkEvent.WorkIncreased _ when activeTimePointKind |> Kind.isWork -> head |> Some
                        | _ -> None

                match
                    workEventLists
                    |> List.last
                    |> _.Events
                    |> List.rev
                    |> findLastIncreased
                with
                | Some ev ->
                    match ev with
                    | WorkEvent.WorkIncreased (value = v) when activeTimePointKind |> Kind.isWork -> increaseLast spentTimes v
                    | WorkEvent.BreakIncreased (value = v) when activeTimePointKind |> Kind.isBreak -> increaseLast spentTimes v
                    | _ -> spentTimes
                | _ -> spentTimes
                |> List.filter (_.SpentTime >> fun t -> t > TimeSpan.Zero )

            return spentTimes
    }