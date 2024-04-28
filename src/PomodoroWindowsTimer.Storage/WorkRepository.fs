module PomodoroWindowsTimer.Storage.WorkRepository

open System
open System.Data
open System.Threading.Tasks
open Dapper
open Dapper.FSharp.SQLite
open PomodoroWindowsTimer.Types
open Dapper
open System.Collections.Generic

[<CLIMutable>]
type ReadRow =
    {
        id: int
        number: string option
        title: string
        created_at: int64
        updated_at: int64 option
    }

type CreateRow =
    {
        number: string option
        title: string
        created_at: int64
    }

type internal Cfg =
    {
        DbConnection: IDbConnection
        TimeProvider: System.TimeProvider
    }

let private readTable = table'<ReadRow> "work"
let private writeTable = table'<CreateRow> "work"

let create (timeProvider: System.TimeProvider) (execute: string seq -> Map<string, obj> seq -> Task<Result<int, string>>) (number: string option) (title: string) =
    task {
        let nowDate = timeProvider.GetUtcNow().ToUnixTimeMilliseconds()

        let newWork =
            {
                number = number
                title = title
                created_at = nowDate
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

        let cmd = CommandDefinition()

        return!
            execute (seq { insertSql; selectLastSql }) (seq { insertSqlParams; selectLastSqlParams })
    }

let readAll (selectf: SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) () =
    task {
        let! res =
            select {
                for _ in readTable do
                selectAll
            }
            |> selectf

        return
            res
            |> Result.map (Seq.map (fun r ->
                {
                    Id = r.id
                    Number = r.number
                    Title = r.title
                    CreatedAt = r.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                    UpdatedAt = r.updated_at |> Option.defaultValue r.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                } : Work))
    }

let findById (selectf: SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) (id: int) =
    task {
        let! res =
            select {
                for row in readTable do
                where (row.id = id)
            }
            |> selectf

        return
            res
            |> Result.map (Seq.map (fun r ->
                {
                    Id = r.id
                    Number = r.number
                    Title = r.title
                    CreatedAt = r.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                    UpdatedAt = r.updated_at |> Option.defaultValue r.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                } : Work) >> Seq.tryHead)
    }

let find (selectf: SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) (text: string) =
    task {
        let likeExpr = sprintf "%%%s%%" text

        let! res =
            select {
                for row in readTable do
                where (like row.number likeExpr)
                orWhere (like row.title likeExpr)
            }
            |> selectf

        return
            res
            |> Result.map (Seq.map (fun r ->
                {
                    Id = r.id
                    Number = r.number
                    Title = r.title
                    CreatedAt = r.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                    UpdatedAt = r.updated_at |> Option.defaultValue r.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                } : Work))
    }

let update (timeProvider: System.TimeProvider) (updatef: UpdateQuery<_> -> Task<Result<unit, string>>) (work: Work) =
    task {
        let nowDate = timeProvider.GetUtcNow().ToUnixTimeMilliseconds()

        return!
            update {
                for r in readTable do
                setColumn r.number work.Number
                setColumn r.title work.Title
                setColumn r.updated_at (nowDate |> Some)
                where (r.id = work.Id)
            }
            |> updatef
    }

let delete (deletef: DeleteQuery -> Task<Result<unit, string>>) (id: int) =
    task {
        return!
            delete {
                for r in readTable do
                where (r.id = id)
            }
            |> deletef
    }

