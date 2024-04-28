module PomodoroWindowsTimer.Storage.Helpers

open System
open System.Data

open Dapper
open Dapper.FSharp.SQLite

let execute (dbConnection: IDbConnection) (sqlScripts: string seq) (sqlParameters: Map<string, obj> seq) =
    task {
        let sql =
            String.Join(";" + Environment.NewLine, sqlScripts)

        let sqlParameters =
            sqlParameters
            |> Seq.map (fun m -> m |> Map.toSeq)
            |> Seq.concat
            |> Map.ofSeq

        let command = CommandDefinition(sql, sqlParameters)
        try
            let! v = dbConnection.ExecuteScalarAsync<int>(command)
            return v |> Ok
        with ex ->
            return ex.Message |> Error
    }

let select<'T> (dbConnection: IDbConnection) (selectQuery: SelectQuery) =
    task {
        try
            let! res = dbConnection.SelectAsync<'T>(selectQuery)
            return res |> Ok
        with ex ->
            return ex.Message |> Error
    }


let update<'T> (dbConnection: IDbConnection) (updateQuery: UpdateQuery<'T>) =
    task {
        try
            let! _ = dbConnection.UpdateAsync<'T>(updateQuery)
            return () |> Ok
        with ex ->
            return ex.Message |> Error
    }


let delete<'T> (dbConnection: IDbConnection) (deleteQuery: DeleteQuery) =
    task {
        try
            let! _ = dbConnection.DeleteAsync(deleteQuery)
            return () |> Ok
        with ex ->
            return ex.Message |> Error
    }
