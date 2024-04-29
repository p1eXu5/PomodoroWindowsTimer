module PomodoroWindowsTimer.Storage.WorkRepository

open System
open System.Data
open System.Threading.Tasks
open Dapper
open Dapper.FSharp.SQLite
open PomodoroWindowsTimer.Types
open Dapper
open System.Collections.Generic
open PomodoroWindowsTimer.Abstractions
open Microsoft.Data.Sqlite
open System.Threading

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

let create
    (timeProvider: System.TimeProvider)
    (execute: string seq -> Map<string, obj> seq -> CancellationToken -> Task<Result<int, string>>)
    (number: string option)
    (title: string)
    (ct: CancellationToken)
    =
    task {
        let nowDate = timeProvider.GetUtcNow()

        let newWork =
            {
                number = number
                title = title
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
                {
                    Id = r.id
                    Number = r.number
                    Title = r.title
                    CreatedAt = r.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                    UpdatedAt = r.updated_at |> Option.defaultValue r.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                } : Work))
    }

let findById (selectf: CancellationToken -> SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) (id: int) ct =
    task {
        let! res =
            select {
                for row in readTable do
                where (row.id = id)
            }
            |> selectf ct

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

let find (selectf: CancellationToken -> SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>) (text: string) ct =
    task {
        let likeExpr = sprintf "%%%s%%" text

        let! res =
            select {
                for row in readTable do
                where (like row.number likeExpr)
                orWhere (like row.title likeExpr)
            }
            |> selectf ct

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

let findOrCreate
    (timeProvider: System.TimeProvider)
    (selectf: CancellationToken -> SelectQuery -> Task<Result<IEnumerable<ReadRow>, string>>)
    (execute: string seq -> Map<string, obj> seq -> CancellationToken -> Task<Result<int, string>>)
    (work: Work)
    ct
    =
    task {
        match! findById selectf work.Id ct with
        | Ok (Some work) -> return work |> Ok
        | Ok None ->
            match! create timeProvider execute work.Number work.Title ct with
            | Ok (id, createdAt) ->
                return
                    {
                        work with
                            Id = id
                            CreatedAt = createdAt
                            UpdatedAt = createdAt
                    }
                    |> Ok
            | Error err -> return Error err
        | Error err -> return Error err
    }

let update (timeProvider: System.TimeProvider) (updatef: CancellationToken -> UpdateQuery<_> -> Task<Result<unit, string>>) (work: Work) ct =
    task {
        let nowDate = timeProvider.GetUtcNow()
        let updatedAt = nowDate.ToUnixTimeMilliseconds() |> Some

        let! res =
            update {
                for r in readTable do
                setColumn r.number work.Number
                setColumn r.title work.Title
                setColumn r.updated_at updatedAt
                where (r.id = work.Id)
            }
            |> updatef ct

        return res |> Result.map (fun () -> nowDate)
    }

let delete (deletef: CancellationToken -> DeleteQuery -> Task<Result<unit, string>>) (id: int) ct =
    task {
        return!
            delete {
                for r in readTable do
                where (r.id = id)
            }
            |> deletef ct
    }


