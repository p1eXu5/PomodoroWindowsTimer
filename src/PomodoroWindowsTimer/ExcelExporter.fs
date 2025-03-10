module PomodoroWindowsTimer.ExcelExporter

open System
open FsToolkit.ErrorHandling

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

/// Returns start time with rows
let internal excelRows (gluingThreshold: TimeSpan) (workEventOffsetTimes: WorkEventList list) =
    let events =
        workEventOffsetTimes
        |> List.map (fun it -> it.Events |> List.map (fun t -> (it.Work, t)))
        |> List.concat
        |> List.sortBy (snd >> WorkEvent.createdAt)
        |> List.skipWhile (snd >> WorkEvent.isStopped)

    match events with
    | head :: tail ->

        let workEvent = snd

        match head |> workEvent with
        | WorkEvent.WorkStarted (ct, _, _)
        | WorkEvent.BreakStarted (ct, _, _) ->

            let reduceTime (work: Work) (startTime: TimeOnly) (value: TimeSpan) (rows: ExcelRow list) =
                let rec running (value: TimeSpan) (rows: ExcelRow list) res =
                    match rows with
                    | [] ->
                        if value <> TimeSpan.Zero then
                            // Error $"Have no excel rows to reduce time on {value}"
                            res |> List.rev |> Ok
                        else
                            res |> List.rev |> Ok
                    | (ExcelRow.WorkExcelRow head) :: [] ->
                        if head.Work.Id = work.Id then
                            let diff = head.End - startTime - value
                            //if diff >= TimeSpan.Zero then
                            [
                                yield! res |> List.map (fun r -> r |> ExcelRow.subTime value) |> List.rev
                                yield head |> WorkExcelRow.subTime value |> ExcelRow.WorkExcelRow
                            ]
                            |> Ok
                            //else
                            //    Error $"First excel row has no match time to reduce time on {value}"
                        else
                            ((head |> ExcelRow.WorkExcelRow) :: res)
                            |> running value []

                    | (ExcelRow.WorkExcelRow head) :: previousRow :: tail ->
                        if head.Work.Id = work.Id then
                            let previousEnd = previousRow |> ExcelRow.endTimeOnly
                            let diff = head.End - previousEnd - value
                            if diff >= TimeSpan.Zero then
                                [
                                    yield! res |> List.map (fun r -> r |> ExcelRow.subTime value) |> List.rev
                                    yield head |> WorkExcelRow.subTime value |> ExcelRow.WorkExcelRow
                                    yield previousRow
                                    yield! tail
                                ]
                                |> Ok
                            else
                                let nextValue = -diff
                                let diff = head.End - previousEnd
                                [
                                    yield head |> WorkExcelRow.subTime diff |> ExcelRow.WorkExcelRow
                                    yield! res |> List.map (fun r -> r |> ExcelRow.subTime diff)
                                ]
                                |> running
                                    nextValue // invert negative value
                                    (previousRow :: tail)
                        else
                            ((head |> ExcelRow.WorkExcelRow) :: res)
                            |> running value (previousRow :: tail)

                    | idle :: tail ->
                        (idle :: res)
                        |> running value tail

                if value < TimeSpan.Zero then
                    Error "Negative value"
                else
                    running value rows []

            // ---------------- increaseTime

            let increaseTime (work: Work) (_: TimeOnly) (value: TimeSpan) (rows: ExcelRow list) =
                let rec running (value: TimeSpan) (rows: ExcelRow list) res =
                    match rows with
                    | [] ->
                        if value <> TimeSpan.Zero then
                            Error $"Have no excel rows to increase time on {value}"
                        else
                            res |> List.rev |> Ok

                    | (ExcelRow.WorkExcelRow head) :: tail ->
                        if head.Work.Id = work.Id then
                            [
                                yield! res |> List.map (fun r -> r |> ExcelRow.addTime value) |> List.rev
                                yield head |> WorkExcelRow.addTime value |> ExcelRow.WorkExcelRow
                                yield! tail
                            ]
                            |> Ok
                        else
                            ((head |> ExcelRow.WorkExcelRow) :: res)
                            |> running value tail
                    | idle :: tail ->
                        (idle :: res)
                        |> running value tail

                if value < TimeSpan.Zero then
                    Error "Negative value"
                else
                    running value rows []



            // ----------------


            let folder (startDt: TimeOnly) (rows: ExcelRow list, last: Work * WorkEvent) (curr: Work * WorkEvent) =
                let (currWork, currEv) = curr
                let (lastWork, lastEv) = last

                match currEv, lastEv with
                | WorkEvent.WorkStarted (currCt, _, _), WorkEvent.WorkStarted (lastCt, _, _)
                | WorkEvent.WorkStarted (currCt, _, _), WorkEvent.BreakStarted (lastCt, _, _)
                | WorkEvent.BreakStarted (currCt, _, _), WorkEvent.WorkStarted (lastCt, _, _)
                | WorkEvent.BreakStarted (currCt, _, _), WorkEvent.BreakStarted (lastCt, _, _)
                | WorkEvent.Stopped currCt, WorkEvent.BreakStarted (lastCt, _, _)
                | WorkEvent.Stopped currCt, WorkEvent.WorkStarted (lastCt, _, _) when currWork.Id = lastWork.Id ->
                    let (head, tail) = rows |> List.head, rows |> List.tail
                    (
                        (head |> ExcelRow.addTime (currCt - lastCt)) :: tail
                        , curr
                    )
                    |> Ok

                // hack: need to cover event generation
                | WorkEvent.WorkStarted (currCt, _, _), WorkEvent.WorkStarted (lastCt, _, _)
                | WorkEvent.WorkStarted (currCt, _, _), WorkEvent.BreakStarted (lastCt, _, _)
                | WorkEvent.BreakStarted (currCt, _, _), WorkEvent.WorkStarted (lastCt, _, _)
                | WorkEvent.BreakStarted (currCt, _, _), WorkEvent.BreakStarted (lastCt, _, _) when currWork.Id <> lastWork.Id ->
                    let (head, tail) = rows |> List.head, rows |> List.tail
                    (
                        (head |> ExcelRow.addTime (currCt - lastCt)) :: tail
                        , curr
                    )
                    |> Ok

                | WorkEvent.WorkStarted (currCt, _, _), WorkEvent.Stopped (lastCt)
                | WorkEvent.BreakStarted (currCt, _, _), WorkEvent.Stopped (lastCt) ->
                    let diff = currCt - lastCt
                    if diff <= gluingThreshold then
                        if currWork.Id = lastWork.Id then
                            let (head, tail) = rows |> List.head, rows |> List.tail
                            (
                                (head |> ExcelRow.addTime diff) :: tail
                                , curr
                            )
                            |> Ok
                        else
                            let head = rows |> List.head
                            let lastNum = head |> ExcelRow.num
                            let lastEnd = head |> ExcelRow.endAddTime diff
                            let w =
                                ExcelRow.createWorkExcelRow
                                    (lastNum + 1)
                                    currWork
                                    lastEnd

                            (
                                (w :: rows)
                                , curr
                            )
                            |> Ok
                    else
                        let head = rows |> List.head
                        let lastNum = head |> ExcelRow.num
                        let iddleEnd = head |> ExcelRow.endAddTime diff
                        let idle =
                            ExcelRow.createIdleExcelRow
                                (lastNum + 1)
                                iddleEnd

                        let w =
                            ExcelRow.createWorkExcelRow
                                (lastNum + 2)
                                currWork
                                iddleEnd

                        (
                            (w :: idle :: rows)
                            , curr
                        )
                        |> Ok

                | WorkEvent.WorkIncreased (_, v, _), _
                | WorkEvent.BreakIncreased (_, v, _), _ ->
                    if v < TimeSpan.Zero then
                        rows
                        |> reduceTime currWork startDt -v
                        |> Result.map (fun rs ->
                           (rs, last)
                        )
                    else
                        rows
                        |> increaseTime currWork startDt v
                        |> Result.map (fun rs ->
                            (rs, last)
                        )

                | WorkEvent.WorkReduced (_, v, _), _
                | WorkEvent.BreakReduced (_, v, _), _ ->
                    if v < TimeSpan.Zero then
                        rows
                        |> increaseTime currWork startDt -v
                        |> Result.map (fun rs ->
                            (rs, last)
                        )
                    else
                        rows
                        |> reduceTime currWork startDt v
                        |> Result.map (fun rs ->
                           (rs, last)
                        )

                | _ -> Error $"Not implemented. Current: {curr}, Last: {last}"


            let rec traverseRes startDt f list =
                match list with
                | [] -> [] |> Ok
                | head :: [] -> f head |> Result.map fst
                | head :: tail ->
                    f head
                    |> Result.bind (fun h -> 
                        traverseRes startDt (folder startDt h) tail
                    )

            let work = head |> fst

            let startDt = TimeOnly.FromDateTime(ct.LocalDateTime)

            let initWorkExcelRow =
                {
                    Num = 1
                    Work = work
                    End = startDt
                }
                |> ExcelRow.WorkExcelRow

            let init = ([initWorkExcelRow], head)

            traverseRes startDt (folder startDt init) tail
            |> Result.map (fun rows ->
                let round (row: ExcelRow) =
                    let minute = (row |> ExcelRow.endTimeOnly).Minute % 5
                    if minute = 0 then row
                    elif minute < 3 then row |> ExcelRow.subTime (TimeSpan.FromMinutes(minute))
                    else row |> ExcelRow.addTime (TimeSpan.FromMinutes(float (5 - minute)))

                let fakeStartRow = ExcelRow.createIdleExcelRow 0 startDt |> round
                let rows =
                    match rows with
                    | [] -> []
                    | _ ->
                        List.scanBack (fun (curr: ExcelRow) (prev: ExcelRow) ->
                            let mutable curr' = (curr |> round)
                            while (curr' |> ExcelRow.endTimeOnly) < (prev |> ExcelRow.endTimeOnly) do
                                curr' <- curr' |> ExcelRow.addTime (TimeSpan.FromMinutes(5))
                            curr'
                        ) rows fakeStartRow
                        |> List.rev
                        |> List.tail
                (
                    fakeStartRow |> ExcelRow.endTimeOnly,
                    rows
                )
            )

        | _ -> Error $"First event is {head}"

    | [] -> Error "Have no events"



let export (excelBook: IExcelBook) (gluingThreshold: TimeSpan) (fileName: string) (workEvents: WorkEventList list) =
    result {
        let! dailyRows =
            workEvents
            |> WorkEventList.List.groupByDay
            |> List.map (fun (day, wel) ->
                wel
                |> excelRows gluingThreshold
                |> Result.map (fun (startTime, rows) -> (day, startTime, rows))
            )
            |> List.sequenceResultM

        let! sheet = excelBook.Create fileName
        let! startRow = sheet.AddHeaders ()

        let addRows startRow t =
            let (day, startTime, rows) = t
            sheet.AddRows day startTime rows startRow

        let rec running f list =
            match list with
            | [] -> Ok ()
            | head :: tail ->
                head
                |> f
                |> Result.bind (fun nextRow ->
                    running (addRows nextRow) tail
                )

        do! running (addRows startRow) dailyRows

        do! excelBook.Save sheet
        return ()
    }
