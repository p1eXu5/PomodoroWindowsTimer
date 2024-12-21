namespace PomodoroWindowsTimer.Storage

open System

type LastEventCreatedAt(unixMilliseconds: int64) =
    member _.Value =
        if unixMilliseconds >= 0 then
            DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds) |> Some
        else
            None


module SqlMapper =

    open System.Data
    open Microsoft.FSharp.Core
    open Dapper

    type LastEventCreatedAtHandler() =
        inherit SqlMapper.TypeHandler<LastEventCreatedAt>()

        override _.Parse(value: obj) =
            match value with
            | :? int64 as v -> LastEventCreatedAt(v)
            | _ -> raise (ArgumentException("LastEventCreatedAt expects int32 value"))

        override _.SetValue(parameter: IDbDataParameter, value: LastEventCreatedAt) =
            raise (NotImplementedException("LastEventCreatedAt is only for reading."))


        static member Register() =
            SqlMapper.AddTypeHandler<LastEventCreatedAt>(new LastEventCreatedAtHandler())

