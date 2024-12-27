namespace PomodoroWindowsTimer.Storage

open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open PomodoroWindowsTimer.Abstractions

/// <summary>
/// Seeds init database.
/// </summary>
type DbSeeder(repositoryFactory: IRepositoryFactory, logger: ILogger<DbSeeder>) =

    let seedDatabaseAsync (repositoryFactory: IRepositoryFactory) (cancellationToken: CancellationToken) =
        task {
            let workRepository = repositoryFactory.GetWorkRepository() :?> WorkRepository
            let! res = workRepository.CreateTableAsync(cancellationToken)
            if res |> Result.isError then
                res |> Result.mapError (fun err -> logger.LogError(err)) |> ignore
                return res
            else
                let workEventRepository = repositoryFactory.GetWorkEventRepository() :?> WorkEventRepository
                let! res = workEventRepository.CreateTableAsync(cancellationToken)
                res |> Result.mapError (fun err -> logger.LogError(err)) |> ignore
                return res
        }

    member _.RepositoryFactory with get () = repositoryFactory

    /// <summary>
    /// Creates two tables - <c>`work`</c> (<see cref="PomodoroWindowsTimer.Storage.WorkRepository.Sql.CREATE_TABLE" />)
    /// and <c>`work_event`</c> (<see cref="PomodoroWindowsTimer.Storage.WorkEventRepository.Sql.CREATE_TABLE" />).
    /// </summary>
    /// <param name="cancellationToken"></param>
    member _.SeedDatabaseAsync(cancellationToken: CancellationToken) : Task<Result<unit, string>> =
        seedDatabaseAsync repositoryFactory cancellationToken

    member _.SeedDatabaseAsync(databaseSettings: IDatabaseSettings, cancellationToken: CancellationToken) =
        let repositoryFactory = repositoryFactory.Replicate(databaseSettings)
        seedDatabaseAsync repositoryFactory cancellationToken


    interface IDbSeeder with
        member this.RepositoryFactory = this.RepositoryFactory
        member this.SeedDatabaseAsync (cancellationToken: CancellationToken) : Task<Result<unit,string>> = 
            this.SeedDatabaseAsync(cancellationToken)
        member this.SeedDatabaseAsync(databaseSettings: IDatabaseSettings, cancellationToken: CancellationToken) =
            this.SeedDatabaseAsync(databaseSettings, cancellationToken)
