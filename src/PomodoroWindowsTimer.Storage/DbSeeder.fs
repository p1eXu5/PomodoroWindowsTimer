namespace PomodoroWindowsTimer.Storage

open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open PomodoroWindowsTimer.Abstractions

/// <summary>
/// Seeds init database.
/// </summary>
type DbSeeder(repositoryFactory: IRepositoryFactory, logger: ILogger<DbSeeder>) =
    /// <summary>
    /// Creates two tables - <c>`work`</c> (<see cref="PomodoroWindowsTimer.Storage.WorkRepository.Sql.CREATE_TABLE" />)
    /// and <c>`work_event`</c> (<see cref="PomodoroWindowsTimer.Storage.WorkEventRepository.Sql.CREATE_TABLE" />).
    /// </summary>
    /// <param name="cancellationToken"></param>
    member _.SeedDatabaseAsync(cancellationToken: CancellationToken) : Task<Result<unit, string>> =
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

    interface IDbSeeder with
        member this.SeedDatabaseAsync (cancellationToken: CancellationToken) : Task<Result<unit,string>> = 
            this.SeedDatabaseAsync(cancellationToken)

