namespace PomodoroWindowsTimer.Storage

open System
open System.Threading
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open PomodoroWindowsTimer.Storage


type TestDbSeederHostedService(serviceProvider: IServiceProvider, appLifetime: IHostApplicationLifetime) =
    inherit BackgroundService()

    let semaphore = new SemaphoreSlim(0, 1)

    member _.Semaphore with get() = semaphore

    override _.ExecuteAsync(stoppingToken: CancellationToken) =
        task {
            let workRepository = serviceProvider.GetRequiredService<WorkRepository>()
            let workEventRepository = serviceProvider.GetRequiredService<WorkEventRepository>()
            let activeTimePointRepository = serviceProvider.GetRequiredService<ActiveTimePointRepository>()

            let! res = workRepository.CreateTableAsync(stoppingToken)
            if res |> Result.isError then
                appLifetime.StopApplication()

            let! res = activeTimePointRepository.CreateTableAsync(stoppingToken)
            if res |> Result.isError then
                appLifetime.StopApplication()

            let! res = workEventRepository.CreateActualTableAsync(stoppingToken)
            if res |> Result.isError then
                appLifetime.StopApplication()

            let _ = semaphore.Release()
            return ()
        }

    override _.Dispose() =
        semaphore.Dispose()
        base.Dispose()
