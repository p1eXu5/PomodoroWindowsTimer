namespace PomodoroWindowsTimer.Storage

open System.Threading
open System.Threading.Tasks

type DbFileRevisor =
    {
        TryUpdateDatabaseFile: unit -> CancellationToken -> Task<Result<unit, string>>
    }

module DbFileRevisor =
    ()

