﻿namespace PomodoroWindowsTimer.Storage

open System.Runtime.CompilerServices
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions

open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Storage.Configuration

[<Extension>]
type DependencyInjectionExtensions() =
    
    [<Extension>]
    static member AddWorkEventStorage(services: IServiceCollection,  configuration: IConfiguration) =
        LastEventCreatedAtHandler.Register()

        services
            .AddOptions<WorkDbOptions>()
            .Bind(configuration.GetSection(WorkDbOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart()
            |> ignore

        services.TryAddSingleton<WorkRepository>()
        services.TryAddSingleton<IWorkRepository, WorkRepository>()
        services.TryAddSingleton<WorkEventRepository>()
        services.TryAddSingleton<IWorkEventRepository, WorkEventRepository>()

        services.AddHostedService<DbSeederHostedService>()

