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
        abstract Create: string option -> string -> CancellationToken -> Task<Result<(uint64 * DateTimeOffset), string>>
        abstract ReadAll: CancellationToken -> Task<Result<Work seq, string>>
        abstract FindById: uint64 -> CancellationToken -> Task<Result<Work option, string>>
        abstract FindByIdOrCreate: Work -> CancellationToken -> Task<Result<Work, string>>
        abstract Update: Work -> CancellationToken -> Task<Result<DateTimeOffset, string>>
    end


type IWorkEventRepository =
    interface
        abstract Create: uint64 -> WorkEvent -> CancellationToken -> Task<Result<(uint64 * DateTimeOffset), string>>
        abstract ReadAll: CancellationToken -> Task<Result<WorkEvent seq, string>>
    end

