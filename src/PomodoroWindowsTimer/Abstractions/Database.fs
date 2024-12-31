namespace PomodoroWindowsTimer.Abstractions

open System
open System.Text
open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open System.Runtime.CompilerServices

type IDatabaseSettings =
    interface
        abstract DatabaseFilePath : string with get, set
        abstract Pooling : Nullable<bool> with get
        abstract Mode : string with get
        abstract Cache: string with get
    end

[<Extension>]
type DatabaseSettingsExtensions =
    [<Extension>]
    static member GetConnectionString(dbSettings: IDatabaseSettings) =
        if dbSettings.Mode.Equals("Memory", StringComparison.Ordinal) && dbSettings.Cache.Equals("Shared", StringComparison.Ordinal) then
            $"Data Source={dbSettings.DatabaseFilePath};Mode=Memory;Cache=Shared;"
        else
            StringBuilder($"Data Source={dbSettings.DatabaseFilePath};Pooling={dbSettings.Pooling};")
            |> fun sb ->
                if String.IsNullOrEmpty(dbSettings.Mode) then sb
                else sb.AppendFormat("Mode={0};", dbSettings.Mode)
            |> fun sb ->
                if String.IsNullOrEmpty(dbSettings.Cache) then sb
                else sb.AppendFormat("Cache={0};", dbSettings.Cache)
            |> fun sb -> sb.ToString()

    static member Create(dbFilePath: string, pooling: Nullable<bool>) =
        { new IDatabaseSettings with 
            member _.DatabaseFilePath
                with get () = dbFilePath
                and set _ = ()
            member _.Pooling with get () = pooling
            member _.Mode with get () = Unchecked.defaultof<_>
            member _.Cache with get () = Unchecked.defaultof<_>
        }


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
