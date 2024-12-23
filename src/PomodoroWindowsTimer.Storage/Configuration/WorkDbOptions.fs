namespace PomodoroWindowsTimer.Storage.Configuration

open System.ComponentModel.DataAnnotations

/// Init options.
type WorkDbOptions () =
    static member SectionName = "WorkDb"
    [<Required>]
    [<MinLength(16)>]
    member val ConnectionString: string = "" with get, set

