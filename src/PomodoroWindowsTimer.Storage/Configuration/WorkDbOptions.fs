namespace PomodoroWindowsTimer.Storage.Configuration

open System.ComponentModel.DataAnnotations
open PomodoroWindowsTimer.Abstractions

/// Init options.
type WorkDbOptions () =
    static member SectionName = "WorkDb"
    [<Required>]
    [<MinLength(16)>]
    member val DatabaseFilePath: string = "" with get, set
    member val Pooling: bool = true with get, set

    interface IDatabaseSettings with
        member this.DatabaseFilePath
            with get () = this.DatabaseFilePath
            and set v = this.DatabaseFilePath <- v
        member this.Pooling 
            with get () = this.Pooling

