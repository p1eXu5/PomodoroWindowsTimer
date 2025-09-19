namespace PomodoroWindowsTimer.Abstractions

open System
open System.Text
open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open System.Runtime.CompilerServices
open System.Diagnostics.CodeAnalysis

type IDatabaseSettings =
    interface
        abstract DatabaseFilePath : string with get, set
        abstract Pooling : Nullable<bool> with get
        [<MaybeNull>]
        abstract Mode : string | null with get
        [<MaybeNull>]
        abstract Cache: string | null with get
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
        abstract FindWelByPeriodAsync: DateOnlyPeriod -> CancellationToken -> Task<Result<WorkEventList list, string>>
        /// Returns work event list ordered descended by created at
        abstract FindByPeriodAsync: DateOnlyPeriod -> CancellationToken -> Task<Result<WorkAndEvent list, string>>
        
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
        abstract Replicate: IDatabaseSettings -> IRepositoryFactory
        abstract ReadDbTablesAsync: ?cancellationToken: CancellationToken -> Task<Result<string list, string>>
        abstract ReadRowCountAsync: tableName: string * ?cancellationToken: CancellationToken -> Task<Result<int, string>>
        abstract GetWorkRepository: unit -> IWorkRepository
        abstract GetWorkEventRepository: unit -> IWorkEventRepository
        abstract GetActiveTimePointRepository: unit -> IActiveTimePointRepository
    end


type IDbSeeder =
    interface
        abstract RepositoryFactory: IRepositoryFactory with get
        abstract SeedDatabaseAsync: CancellationToken -> Task<Result<unit, string>>
        abstract SeedDatabaseAsync: dbSettings: IDatabaseSettings * cancellationToken: CancellationToken -> Task<Result<unit, string>>
    end

type IDbMigrator =
    interface
        abstract ApplyMigrations: dbSettings: IDatabaseSettings -> Result<Unit, string>
        abstract ApplyMigrationsAsync: dbSettings: IDatabaseSettings -> cancellationToken: CancellationToken -> Task<Result<Unit, string>>
    end

type IDbFileRevisor =
    interface
        abstract TryUpdateDatabaseFileAsync: IDatabaseSettings -> CancellationToken -> Task<Result<unit, string>>
    end
