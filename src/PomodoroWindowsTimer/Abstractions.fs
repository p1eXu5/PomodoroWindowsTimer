namespace PomodoroWindowsTimer.Abstractions

open System
open System.Threading
open System.Threading.Tasks
open FSharp.Control
open PomodoroWindowsTimer.Types

type ITimePointQueue =
    inherit IDisposable
    abstract AddMany : TimePoint seq -> unit
    abstract Start : unit -> unit
    abstract TryGetNext : unit -> TimePoint option
    abstract Reload : TimePoint list -> unit
    abstract TryPick : unit -> TimePoint option
    abstract ScrollTo : Guid -> unit

type ILooper =
    inherit IDisposable
    abstract Start : unit -> unit
    abstract Shift : float<sec> -> unit
    abstract Resume : unit -> unit
    abstract AddSubscriber : (LooperEvent -> Async<unit>) -> unit
    abstract PreloadTimePoint : unit -> unit

type IWorkRepository =
    interface
        abstract Create: string option -> string -> CancellationToken -> Task<Result<(int * DateTimeOffset), string>>
        abstract ReadAll: CancellationToken -> Task<Result<Work seq, string>>
        abstract FindById: int -> CancellationToken -> Task<Result<Work option, string>>
        abstract FindByIdOrCreate: Work -> CancellationToken -> Task<Result<Work, string>>
        abstract Update: Work -> CancellationToken -> Task<Result<DateTimeOffset, string>>
    end
