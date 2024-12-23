namespace PomodoroWindowsTimer.ElmishApp

open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open PomodoroWindowsTimer.Abstractions

[<Extension>]
type DependencyInjectionExtensions () =
    [<Extension>]
    static member AddElmishAppServices(services: IServiceCollection) =
        services.TryAddSingleton<WorkEventStore>(fun sp ->
            let repositoryFactory = sp.GetRequiredService<IRepositoryFactory>()
            WorkEventStore.init repositoryFactory
        )
