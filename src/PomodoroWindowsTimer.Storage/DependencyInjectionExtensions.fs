namespace PomodoroWindowsTimer.Storage

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
        SqlMapper.LastEventCreatedAtHandler.Register()

        let workDbOptionsSection = configuration.GetSection(WorkDbOptions.SectionName)

        if workDbOptionsSection.Exists() then
            services
                .AddOptions<WorkDbOptions>()
                .Bind(configuration.GetSection(WorkDbOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart()
                |> ignore
        else
            services
                .AddOptions<WorkDbOptions>()
                .Configure(fun options ->
                    options.ConnectionString <- "Data Source=work.db;"
                )
                .ValidateDataAnnotations()
                .ValidateOnStart()
                |> ignore

        services.TryAddSingleton<IRepositoryFactory, RepositoryFactory>()
        services.TryAddSingleton<IDbSeeder, DbSeeder>()
        services.TryAddSingleton<IDbMigrator, PomodoroWindowsTimer.Storage.Migrations.DbMigrator>()

        // if configuration.GetValue<bool>("InTest") |> not then
        //     services.AddHostedService<DbSeederHostedService>()
        // else
        //     services.AddHostedService<TestDbSeederHostedService>()

