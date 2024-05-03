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

let create
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
                created_at = nowDate.ToUnixTimeMilliseconds()
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


let readAll (selectf: CancellationToken -> SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) ct =
    task {
        let! res =
            select {
                for _ in readTable do
                selectAll
            }
            |> selectf ct

        return
            res
            |> Result.map (Seq.map (fun r ->
                JsonHelpers.Deserialize<WorkEvent>(r.event_json)
            ))
    }


