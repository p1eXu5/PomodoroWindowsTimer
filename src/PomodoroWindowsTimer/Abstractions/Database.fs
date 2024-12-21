namespace PomodoroWindowsTimer.Abstractions

open System
open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Types

type IDatabaseSettings =
    interface
        abstract DatabaseFilePath : string with get, set
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

type IRepositoryFactory =
    interface
        abstract GetWorkRepository: unit -> IWorkRepository
        abstract GetWorkEventRepository: unit -> IWorkEventRepository
        abstract GetActiveTimePointRepository: unit -> IActiveTimePointRepository
    end


type IDbSeeder =
    interface
        abstract SeedDatabaseAsync: CancellationToken -> Task<Result<unit, string>>
    end

type IDbMigrator =
    interface
        abstract ApplyMigrations: dbFilePath: string -> Result<Unit, string>
        abstract ApplyMigrationsAsync: dbFilePath: string -> cancellationToken: CancellationToken -> Task<Result<Unit, string>>
    end
