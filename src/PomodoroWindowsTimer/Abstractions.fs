namespace PomodoroWindowsTimer.Abstractions

open System
open System.Threading
open System.Threading.Tasks
open FSharp.Control
open PomodoroWindowsTimer.Types
open System.Threading

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
    abstract Stop : unit -> unit
    abstract Next : unit -> unit
    abstract Shift : float<sec> -> unit
    abstract ShiftAck : float<sec> -> unit
    abstract Resume : unit -> unit
    abstract AddSubscriber : (LooperEvent -> Async<unit>) -> unit
    abstract PreloadTimePoint : unit -> unit



type IWorkRepository =
    interface
        abstract CreateAsync: number: string -> title: string -> CancellationToken -> Task<Result<(uint64 * DateTimeOffset), string>>
        abstract ReadAllAsync: CancellationToken -> Task<Result<Work list, string>>
        abstract FindByIdAsync: workId: uint64 -> CancellationToken -> Task<Result<Work option, string>>
        abstract FindByIdOrCreateAsync: Work -> CancellationToken -> Task<Result<Work, string>>
        abstract UpdateAsync: Work -> CancellationToken -> Task<Result<DateTimeOffset, string>>
    end

type IWorkEventRepository =
    interface
        abstract CreateAsync: workId: uint64 -> WorkEvent -> CancellationToken -> Task<Result<(uint64 * DateTimeOffset), string>>
        abstract FindByWorkIdAsync: workId: uint64 -> CancellationToken -> Task<Result<WorkEvent list, string>>
        abstract FindByWorkId: workId: uint64 -> Result<WorkEvent list, string>
        abstract FindByWorkIdByDateAsync: workId: uint64 -> DateOnly -> CancellationToken -> Task<Result<WorkEvent list, string>>
        abstract FindByWorkIdByPeriodAsync: workId: uint64 -> DateOnlyPeriod -> CancellationToken -> Task<Result<WorkEvent list, string>>
        
        /// Returns work event list ordered by work id then by created at time.
        abstract FindAllByPeriodAsync: DateOnlyPeriod -> CancellationToken -> Task<Result<WorkEventList list, string>>
        
        abstract FindLastByWorkIdByDateAsync: workId: uint64 -> DateOnly -> CancellationToken -> Task<Result<WorkEvent option, string>>
    end

type IExcelSheet =
    interface
        abstract AddRowsAsync: ExcelRow seq -> CancellationToken -> Task<Result<unit, string>>
    end

type IExcelBook =
    interface
        abstract CreateAsync: string -> CancellationToken -> Task<Result<IExcelSheet, string>>
        abstract SaveAsync: CancellationToken -> Task<Result<unit, string>>
    end

