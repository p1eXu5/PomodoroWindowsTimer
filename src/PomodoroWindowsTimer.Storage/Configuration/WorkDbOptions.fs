namespace PomodoroWindowsTimer.Storage.Configuration

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type WorkDbOptions =
    {
        [<Required>]
        [<MinLength(16)>]
        ConnectionString: string
    }
    with
        static member SectionName = "WorkDb"

