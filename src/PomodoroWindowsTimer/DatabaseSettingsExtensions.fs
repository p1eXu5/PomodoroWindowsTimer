(*
 * User in:
 *   - PomodoroWindowsTimer.Storage
 *   - PomodoroWindowsTimer.Storage.Migrations
 *)

namespace PomodoroWindowsTimer

open System
open System.Runtime.CompilerServices
open System.Text

open PomodoroWindowsTimer.Abstractions

[<Extension>]
type DatabaseSettingsExtensions =
    [<Extension>]
    static member GetConnectionString(dbSettings: IDatabaseSettings) =
        let mode = dbSettings.Mode 
        let cache = dbSettings.Cache

        let isMemoryMode =
            match mode with
            | NonNull m -> m.Equals("Memory", StringComparison.Ordinal)
            | _ -> false

        let isSharedCache =
            match cache with
            | NonNull c -> c.Equals("Shared", StringComparison.Ordinal)
            | _ -> false

        if
            not <| String.IsNullOrWhiteSpace(mode)
            && not <| String.IsNullOrWhiteSpace(cache)
            && isMemoryMode
            && isSharedCache
        then
            $"Data Source={dbSettings.DatabaseFilePath};Mode=Memory;Cache=Shared;"
        else
            StringBuilder($"Data Source={dbSettings.DatabaseFilePath};")
            |> fun sb ->
                if String.IsNullOrEmpty(mode) then sb
                else sb.AppendFormat("Mode={0};", mode)
            |> fun sb ->
                if String.IsNullOrEmpty(cache) then sb
                else sb.AppendFormat("Cache={0};", cache)
            |> fun sb ->
                if dbSettings.Pooling.HasValue then sb.AppendFormat("Pooling={0};", dbSettings.Pooling.Value)
                else sb
            |> fun sb -> sb.ToString()

    static member Create(dbFilePath: string, pooling: Nullable<bool>) =
        { new IDatabaseSettings with 
            member _.DatabaseFilePath
                with get () = dbFilePath
                and set _ = ()
            member _.Pooling with get () = pooling
            member _.Mode with get () = Unchecked.defaultof<_>
            member _.Cache with get () = Unchecked.defaultof<_>
        }