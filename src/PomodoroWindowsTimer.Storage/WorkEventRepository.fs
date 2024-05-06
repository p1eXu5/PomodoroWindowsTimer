module PomodoroWindowsTimer.Storage.WorkEventRepository

open System
open System.Collections.Generic
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.SQLite

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer

type CreateRow =
    {
        work_id: uint64
        event_json: string
        created_at: int64
    }

[<CLIMutable>]
type ReadRow =
    {
        id: uint64
        work_id: uint64
        event_json: string
        created_at: int64
    }

let private readTable = table'<ReadRow> "work_event"
let private writeTable = table'<CreateRow> "work_event"

let createTask
    (timeProvider: System.TimeProvider)
    (execute: string seq -> Map<string, obj> seq -> CancellationToken -> Task<Result<uint64, string>>)
    (workId: uint64)
    (workEvent: WorkEvent)
    (ct: CancellationToken)
    =
    task {
        let nowDate = timeProvider.GetUtcNow()

        let newWork =
            {
                work_id = workId
                event_json = workEvent |> JsonHelpers.Serialize
                created_at = (workEvent |> WorkEvent.createdAt).ToUnixTimeMilliseconds()
            } : CreateRow

        let (insertSql, insertSqlParams) =
            insert {
                into writeTable
                value newWork
            }
            |> Deconstructor.insert

        let (selectLastSql, selectLastSqlParams) =
            select {
                for r in readTable do
                orderByDescending r.id
                skipTake 0 1
            }
            |> Deconstructor.select<ReadRow>

        let! res =
            execute (seq { insertSql; selectLastSql }) (seq { insertSqlParams; selectLastSqlParams }) ct

        return res |> Result.map (fun id -> id, nowDate)
    }


let findByWorkIdTask (selectf: CancellationToken -> SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) (workId: uint64) ct =
    task {
        let! res =
            select {
                for r in readTable do
                where (r.work_id = workId)
            }
            |> selectf ct

        return
            res
            |> Result.map (Seq.map (fun r ->
                JsonHelpers.Deserialize<WorkEvent>(r.event_json)
            ))
    }

let findByWorkId (selectf: SelectQuery -> Result<IEnumerable<ReadRow>, string>) (workId: uint64) =
    let res =
        select {
            for r in readTable do
            where (r.work_id = workId)
        }
        |> selectf

    res
    |> Result.map (Seq.map (fun r ->
        JsonHelpers.Deserialize<WorkEvent>(r.event_json)
    ))


let findByWorkIdByPeriodQuery workId dateMin dateMax =
    select {
        for r in readTable do
        where (r.work_id = workId)
        andWhere (r.created_at >= dateMin)
        andWhere (r.created_at < dateMax)
    }

let findByWorkIdByDateTask (timeProvider: System.TimeProvider) (selectf: CancellationToken -> SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) (workId: uint64) (date: DateOnly) ct =
    task {

        let dateMin = DateTimeOffset(date, TimeOnly(0, 0, 0), timeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()
        let dateMax = DateTimeOffset(date.AddDays(1), TimeOnly(0, 0, 0), timeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()

        let! res =
            findByWorkIdByPeriodQuery workId dateMin dateMax
            |> selectf ct

        return
            res
            |> Result.map (Seq.map (fun r ->
                JsonHelpers.Deserialize<WorkEvent>(r.event_json)
            ))
    }

let findByWorkIdByPeriodTask (timeProvider: System.TimeProvider) (selectf: CancellationToken -> SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) (workId: uint64) (period: Period) ct =
    task {

        let dateMin = DateTimeOffset(period.Start, TimeOnly(0, 0, 0), timeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()
        let dateMax = DateTimeOffset(period.EndInclusive.AddDays(1), TimeOnly(0, 0, 0), timeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()

        let! res =
            findByWorkIdByPeriodQuery workId dateMin dateMax
            |> selectf ct

        return
            res
            |> Result.map (Seq.map (fun r ->
                JsonHelpers.Deserialize<WorkEvent>(r.event_json)
            ))
    }


let findAllByPeriodTask
    (timeProvider: System.TimeProvider)
    (selectf: CancellationToken -> SelectQuery -> Task<Result<IEnumerable<ReadRow * WorkRepository.ReadRow>, string>>)
    (period: Period)
    ct
    =
    task {

        let dateMin = DateTimeOffset(period.Start, TimeOnly(0, 0, 0), timeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()
        let dateMax = DateTimeOffset(period.EndInclusive.AddDays(1), TimeOnly(0, 0, 0), timeProvider.LocalTimeZone.BaseUtcOffset).ToUnixTimeMilliseconds()

        let! res =
            select {
                for r in readTable do
                innerJoin w in WorkRepository.readTable on (r.work_id = w.id)
                andWhere (r.created_at >= dateMin)
                andWhere (r.created_at < dateMax)
                orderBy r.work_id
                thenBy r.created_at
            }
            |> selectf ct

        return
            res
            |> Result.map (fun l ->
                l
                |> Seq.map (fun (r, w) ->
                    let work = w |> WorkRepository.ReadRow.toWork
                    let ev = JsonHelpers.Deserialize<WorkEvent>(r.event_json)
                    (work, ev)
                )
                |> Seq.groupBy fst
                |> Seq.map (fun (w, evs) ->
                    {
                        Work = w
                        Events = evs |> Seq.map snd |> Seq.toList
                    }
                )
            )
    }

