namespace PomodoroWindowsTimer.Abstractions

open System
open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Types

type ITimePointQueue =
    inherit IDisposable
    abstract AddMany : TimePoint seq -> unit
    abstract Start : unit -> unit
    abstract TryGetNext : unit -> TimePoint option
    abstract Reload : TimePoint list -> unit
    abstract TryPick : unit -> TimePoint option
    abstract ScrollTo : Guid -> unit
    abstract TryFind : TimePointId -> TimePoint option

type ILooper =
    interface
        inherit IDisposable
        abstract Start : unit -> unit
        abstract Stop : unit -> unit
        abstract Next : unit -> unit
        abstract Shift : float<sec> -> unit
        abstract ShiftAck : float<sec> -> unit
        abstract Resume : unit -> unit
        abstract AddSubscriber : (LooperEvent -> Async<unit>) -> unit
        /// Tryes to pick TimePoint from queue, if it present
        /// emits TimePointStarted event and sets ActiveTimePoint.
        abstract PreloadTimePoint : unit -> unit
        abstract GetActiveTimePoint : unit -> ActiveTimePoint option
    end

type IWorkRepository =
    interface
        abstract InsertAsync: number: string -> title: string -> cancellationToken: CancellationToken -> Task<Result<(uint64 * DateTimeOffset), string>>
        abstract ReadAllAsync: cancellationToken: CancellationToken -> Task<Result<Work list, string>>
        abstract SearchByNumberOrTitleAsync: text: string -> cancellationToken: CancellationToken -> Task<Result<Work list, string>>
        abstract FindByIdAsync: workId: WorkId -> cancellationToken: CancellationToken -> Task<Result<Work option, string>>
        abstract FindByIdOrCreateAsync: work: Work -> cancellationToken: CancellationToken -> Task<Result<Work, string>>
        abstract UpdateAsync: work: Work -> cancellationToken: CancellationToken -> Task<Result<DateTimeOffset, string>>
        abstract DeleteAsync: work: Work -> cancellationToken: CancellationToken -> Task<Result<unit, string>>
    end

type IWorkEventRepository =
    interface
        abstract InsertAsync: workId: WorkId -> WorkEvent -> cancellationToken: CancellationToken -> Task<Result<uint64, string>>
        // abstract FindByWorkId: workId: WorkId -> Result<WorkEvent list, string>
        abstract FindByWorkIdAsync: workId: WorkId -> cancellationToken: CancellationToken -> Task<Result<WorkEvent list, string>>
        abstract FindByWorkIdByDateAsync: workId: WorkId -> DateOnly -> cancellationToken: CancellationToken -> Task<Result<WorkEvent list, string>>
        abstract FindByWorkIdByPeriodAsync: workId: WorkId -> DateOnlyPeriod -> cancellationToken: CancellationToken -> Task<Result<WorkEvent list, string>>
        abstract FindLastByWorkIdByDateAsync: workId: WorkId -> DateOnly -> cancellationToken: CancellationToken -> Task<Result<WorkEvent option, string>>
        
        /// Returns work event list ordered by work id then by created at time.
        abstract FindAllByPeriodAsync: DateOnlyPeriod -> CancellationToken -> Task<Result<WorkEventList list, string>>
        
        abstract FindByActiveTimePointIdByDateAsync:
            timePointId: TimePointId
            -> kind: Kind
            -> notAfter: DateTimeOffset
            -> cancellationToken: CancellationToken
            -> Task<Result<(Work * WorkEvent) list, string>>

        abstract GetAsync: skip: int -> take: int -> cancellationToken: CancellationToken -> Task<Result<WorkEvent list, string>>
    end

type IActiveTimePointRepository =
    interface
        abstract InsertAsync: activeTimePoint: ActiveTimePoint -> cancellationToken: CancellationToken -> Task<Result<unit, string>>
        abstract InsertIfNotExistsAsync: activeTimePoint: ActiveTimePoint -> cancellationToken: CancellationToken -> Task<Result<unit, string>>
        abstract ReadAllAsync: cancellationToken: CancellationToken -> Task<Result<ActiveTimePoint list, string>>
    end

type StartRow = int

type IExcelSheet =
    interface
        abstract AddHeaders: unit -> Result<StartRow, string>
        abstract AddRows: date: DateOnly -> startTime: TimeOnly -> rows: ExcelRow seq -> StartRow -> Result<StartRow, string>
    end

type IExcelBook =
    interface
        abstract Create: filePath: string -> Result<IExcelSheet, string>
        abstract Save: excelSheet: IExcelSheet -> Result<unit, string>
    end

type IDatabaseSettings =
    interface
        abstract DatabaseFilePath : string with get, set
    end

