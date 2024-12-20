namespace PomodoroWindowsTimer.Storage

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open PomodoroWindowsTimer.Abstractions

type DbSeeder(repositoryFactory: IRepositoryFactory, logger: ILogger<DbSeeder>) =
    member _.SeedDatabaseAsync(cancellationToken: CancellationToken) : Task<Result<unit, string>> =
        task {
            let workRepository = repositoryFactory.GetWorkRepository() :?> WorkRepository
            let! res = workRepository.CreateTableAsync(cancellationToken)
            if res |> Result.isError then
                return res
            else
                let workEventRepository = repositoryFactory.GetWorkEventRepository() :?> WorkEventRepository
                let! res = workEventRepository.CreateTableAsync(cancellationToken)
                return res
        }

