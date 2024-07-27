namespace PomodoroWindowsTimer.Storage

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options
open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Storage.Configuration

type DbSeederHostedService(serviceProvider: IServiceProvider, appLifetime: IHostApplicationLifetime) =
    inherit BackgroundService()

    let semaphore = new SemaphoreSlim(0, 1)

    member _.Semaphore with get() = semaphore

    override _.ExecuteAsync(stoppingToken: CancellationToken) =
        task {
            use scope = serviceProvider.CreateScope()
            let workDbOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<WorkDbOptions>>();

            let workRepository = ActivatorUtilities.CreateInstance<WorkRepository>(serviceProvider, [| workDbOptions.Value |])
            let workEventRepository = ActivatorUtilities.CreateInstance<WorkRepository>(serviceProvider, [| workDbOptions.Value |])

            do LastEventCreatedAtHandler.Register()

            let! res = workRepository.CreateTableAsync(stoppingToken)
            if res |> Result.isError then
                appLifetime.StopApplication()

            let! res = workEventRepository.CreateTableAsync(stoppingToken)
            if res |> Result.isError then
                appLifetime.StopApplication()

            let _ = semaphore.Release()
            return ()
        }

    override _.Dispose() =
        semaphore.Dispose()
        base.Dispose()
