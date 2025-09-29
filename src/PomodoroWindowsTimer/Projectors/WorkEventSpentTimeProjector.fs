module PomodoroWindowsTimer.WorkEventSpentTimeProjector

open System
open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

let rec internal projectSpentTime (lastWorkEvent: Work * WorkEvent) (workEvents: (Work * WorkEvent) list) remainingTime (res: Map<WorkId, WorkSpentTime>) =
    match workEvents with
    | [] ->
        match lastWorkEvent with
        | lastWork, WorkEvent.WorkIncreased (value = v)
        | lastWork, WorkEvent.BreakIncreased (value = v) ->
            let spentTime =
                if v > remainingTime then
                    remainingTime
                else
                    v
            let workSpentTime = 
                match res |> Map.tryFind lastWork.Id with
                | None ->
                    { Work = lastWork; SpentTime = spentTime }
                | Some workSpentTime ->
                    { workSpentTime with SpentTime = workSpentTime.SpentTime + spentTime }

            res |> Map.add lastWork.Id workSpentTime

        | lastWork, WorkEvent.WorkReduced (value = v)
        | lastWork, WorkEvent.BreakReduced (value = v) ->
            let workSpentTime = 
                match res |> Map.tryFind lastWork.Id with
                | None ->
                    { Work = lastWork; SpentTime = -v }
                | Some workSpentTime ->
                    { workSpentTime with SpentTime = workSpentTime.SpentTime - v }

            res |> Map.add lastWork.Id workSpentTime

        | _ ->
            res

    | (work, ev) :: tail ->
        match lastWorkEvent, ev with
        | (lastWork, WorkEvent.Stopped endDt), WorkEvent.WorkStarted (createdAt = startDt)
        | (lastWork, WorkEvent.Stopped endDt), WorkEvent.BreakStarted (createdAt = startDt) when work.Id = lastWork.Id ->
            let spentTime =
                let diff = (endDt - startDt)
                if diff > remainingTime then
                    remainingTime
                else
                    diff

            let workSpentTime = 
                match res |> Map.tryFind work.Id with
                | None ->
                    { Work = work; SpentTime = spentTime }
                | Some workSpentTime ->
                    { workSpentTime with SpentTime = workSpentTime.SpentTime + spentTime }

            if spentTime < remainingTime then
                projectSpentTime
                    lastWorkEvent
                    tail
                    (remainingTime - spentTime)
                    (res |> Map.add work.Id workSpentTime)
            else
                (res |> Map.add work.Id workSpentTime)


        | _, WorkEvent.Stopped _
        | _, WorkEvent.Stopped _ ->
            projectSpentTime
                (work, ev)
                tail
                remainingTime
                res

        | _, WorkEvent.WorkReduced (value = v)
        | _, WorkEvent.BreakReduced (value = v) ->
            let workSpentTime = 
                match res |> Map.tryFind work.Id with
                | None ->
                    { Work = work; SpentTime = -v }
                | Some workSpentTime ->
                    { workSpentTime with SpentTime = workSpentTime.SpentTime - v }

            projectSpentTime
                lastWorkEvent
                tail
                (remainingTime + v)
                (res |> Map.add work.Id workSpentTime)


        | _, WorkEvent.WorkIncreased (value = v)
        | _, WorkEvent.BreakIncreased (value = v) ->
            let workSpentTime = 
                match res |> Map.tryFind work.Id with
                | None ->
                    { Work = work; SpentTime = v }
                | Some workSpentTime ->
                    { workSpentTime with SpentTime = workSpentTime.SpentTime + v }

            let remainingTime = remainingTime - v

            if remainingTime > TimeSpan.Zero then
                projectSpentTime
                    lastWorkEvent
                    tail
                    remainingTime
                    (res |> Map.add work.Id workSpentTime)
            else
                res |> Map.add work.Id workSpentTime

        | (_, lastEvent), _ ->
            raise (ArgumentException($"Unpredictable event order. Current: {ev}, previous: {lastEvent}."))


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
            return
                match workEventLists with
                | [] -> []
                | _ ->
                    workEventLists
                    |> List.groupBy (fun t -> fst t |> _.Id)
                    |> List.map (fun (_, l) ->
                        match l with
                        | [] -> []
                        | head :: tail ->
                            let spentTimes =
                                projectSpentTime
                                    head
                                    tail
                                    (TimeSpan.FromSeconds(float diff))
                                    Map.empty
                                |> Map.values
                                |> Seq.filter (_.SpentTime >> fun t -> t > TimeSpan.Zero)
                                |> List.ofSeq

                            spentTimes
                    )
                    |> List.concat
    }