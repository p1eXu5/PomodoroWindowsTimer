module PomodoroWindowsTimer.Storage.Helpers

open System
open System.Data

open Dapper
open Dapper.FSharp.SQLite
open System.Threading

let execute (dbConnection: IDbConnection) (sqlScripts: string seq) (sqlParameters: Map<string, obj> seq) (ct: CancellationToken) =
    task {
        let sql =
            String.Join(";" + Environment.NewLine, sqlScripts)

        let sqlParameters =
            sqlParameters
            |> Seq.map (fun m -> m |> Map.toSeq)
            |> Seq.concat
            |> Map.ofSeq

        let command = CommandDefinition(sql, sqlParameters, cancellationToken=ct)
        try
            let! v = dbConnection.ExecuteScalarAsync<uint64>(command)
            return v |> Ok
        with ex ->
            return ex.Message |> Error
    }

let select<'T> (dbConnection: IDbConnection) (ct: CancellationToken) (selectQuery: SelectQuery) =
    task {
        try
            let! res = dbConnection.SelectAsync<'T>(selectQuery, cancellationToken=ct)
            return res |> Ok
        with ex ->
            return ex.Message |> Error
    }


let update<'T> (dbConnection: IDbConnection) (ct: CancellationToken) (updateQuery: UpdateQuery<'T>) =
    task {
        try
            let! _ = dbConnection.UpdateAsync<'T>(updateQuery, cancellationToken=ct)
            return () |> Ok
        with ex ->
            return ex.Message |> Error
    }


let delete<'T> (dbConnection: IDbConnection) (ct: CancellationToken) (deleteQuery: DeleteQuery) =
    task {
        try
            let! _ = dbConnection.DeleteAsync(deleteQuery, cancellationToken=ct)
            return () |> Ok
        with ex ->
            return ex.Message |> Error
    }
