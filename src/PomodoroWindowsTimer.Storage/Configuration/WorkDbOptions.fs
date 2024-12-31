namespace PomodoroWindowsTimer.Storage.Configuration

open System.ComponentModel.DataAnnotations
open PomodoroWindowsTimer.Abstractions
open System

/// Init options.
type WorkDbOptions () =
    static member SectionName = "WorkDb"
    [<Required>]
    [<MinLength(16)>]
    member val DatabaseFilePath: string = "" with get, set
    member val Pooling: Nullable<bool> = Unchecked.defaultof<_> with get, set
    member val Mode: string = Unchecked.defaultof<_> with get, set
    member val Cache: string = Unchecked.defaultof<_> with get, set

    interface IDatabaseSettings with
        member this.DatabaseFilePath
            with get () = this.DatabaseFilePath
            and set v = this.DatabaseFilePath <- v
        member this.Pooling with get () = this.Pooling
        member this.Mode with get () = this.Mode
        member this.Cache with get () = this.Cache

