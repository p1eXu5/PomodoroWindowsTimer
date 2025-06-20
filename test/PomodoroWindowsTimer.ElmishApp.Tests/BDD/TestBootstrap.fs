namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open System.Collections.Generic
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging

open Elmish
open NUnit.Framework
open p1eXu5.AspNetCore.Testing
open p1eXu5.AspNetCore.Testing.Logging
open p1eXu5.AspNetCore.Testing.MockRepository

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Infrastructure

open PomodoroWindowsTimer.WpfClient
open System.Collections.Concurrent
open PomodoroWindowsTimer.Abstractions
open NSubstitute
open System.Threading
open PomodoroWindowsTimer.Storage.Configuration
open NPOI.XSSF.Streaming.Values
open Microsoft.Extensions.Options
open Microsoft.Extensions.Configuration

type TestBootstrap () =
    inherit Bootstrap()
    let mutable _disposed = false
    let mutable mockRepository : MockRepository = Unchecked.defaultof<_>

    // ------------------
    // database bottstrap
    // ------------------

    let mutable inMemoryConnection : SqliteConnection = Unchecked.defaultof<_>

    let createInMemoryConnection (configuration: IConfiguration) =
        let token = Guid.NewGuid().ToString("N")
        let dbFilePath = $"workdb{token}";
        configuration["WorkDb:DatabaseFilePath"] <- dbFilePath
        configuration["WorkDb:Mode"] <- "Memory"
        configuration["WorkDb:Cache"] <- "Shared"
        let connectionString = $"Data Source={dbFilePath};Mode=Memory;Cache=Shared;"
        inMemoryConnection <- new SqliteConnection(connectionString)
        inMemoryConnection.Open()

    // ------------------
    //    properties
    // ------------------

    member val MockWindowsMinimizer : IWindowsMinimizer = NSubstitute.Substitute.For<IWindowsMinimizer>() with get

    /// <summary>
    /// <see cref="MockRepository" />.
    /// </summary>
    member _.MockRepository with get() = mockRepository
    
    /// <summary>
    /// <see cref="IServiceProvider" />.
    /// </summary>
    member _.ServiceProvider with get() = base.Host.Services

    


    // ------------------
    //     overrides
    // ------------------

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="disposing"></param>
    override _.Dispose(disposing: bool) =
        if not _disposed then
            if disposing then
                match inMemoryConnection with
                | null -> ()
                | _ -> inMemoryConnection.Dispose()

            _disposed <- true

        base.Dispose(disposing)

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="services"></param>
    override _.PreConfigureServices(hostBuilder: HostBuilderContext,  services: IServiceCollection) =
        // configurations overrides:
        hostBuilder.Configuration["InTest"] <- "True"
        createInMemoryConnection (hostBuilder.Configuration)

        services
            .AddKeyedSingleton<IErrorMessageQueue>(
                "main",
                ErrorMessageQueueStub.create "main"
            ) |> ignore

        services
            .AddKeyedSingleton<IErrorMessageQueue>(
                "dialog",
                ErrorMessageQueueStub.create "dialog"
            )
            |> ignore

        services.AddSingleton<WorkEventStore>(fun sp ->
            let repositoryFactory = sp.GetRequiredService<IRepositoryFactory>()

            WorkEventStore.init repositoryFactory
        )
        |> ignore

        services.AddSingleton<IUserSettings>(fun sp ->
            let dbOptions = sp.GetRequiredService<IOptions<WorkDbOptions>>().Value
            UserSettingsStub(dbOptions) :> IUserSettings
        ) |> ignore
        services.AddSingleton<IDatabaseSettings>(fun sp ->
            sp.GetRequiredService<IUserSettings>() :?> UserSettingsStub :> IDatabaseSettings
        ) |> ignore

        services.AddSingleton<ITelegramBot>(new TelegramBotStub()) |> ignore


    override _.ConfigureLogging(_: HostBuilderContext, loggingBuilder: ILoggingBuilder) =
        loggingBuilder.SetMinimumLevel(LogLevel.Error) |> ignore


    override _.PostConfigureHost(builder: IHostBuilder) =
        builder.AddMockRepository(
            [ 
                Service<IThemeSwitcher>();
                Service<IWindowsMinimizer>();
            ],
            TestLogWriter(TestLogger<TestBootstrap>(TestContextWriters.GetInstance<TestContext>(), LogOut.All)),
            (fun mr -> mockRepository <- mr)
        )


    override _.StartHost (): unit = 
        base.StartHost()

        let dbSeeder = base.Host.Services.GetRequiredService<IDbSeeder>()

        let res = 
            dbSeeder.SeedDatabaseAsync(CancellationToken.None)
            |> Async.AwaitTask
            |> Async.RunSynchronously

        match res with
        | Error err ->
            raise (InvalidOperationException(err))
        | _ -> ()

        let dbSettings = base.Host.Services.GetRequiredService<IDatabaseSettings>()
        let migrator = base.Host.Services.GetRequiredService<IDbMigrator>()

        let res = 
            migrator.ApplyMigrations(dbSettings)

        match res with
        | Error err ->
            raise (InvalidOperationException(err))
        | _ -> ()


    member _.StartTestElmishApp (outMainModel: ref<MainModel>, msgStack: ConcurrentStack<MainModel.Msg>, testDispatcher: TestDispatcher) =
        // do this.WaitDbSeeding()

        let factory = base.GetElmishProgramFactory()
        let (initMainModel, updateMainModel, _, subscribe) =
            CompositionRoot.compose
                "Pomodoro Windows Timer under tests"
                (Func<System.Windows.Window>(fun () -> Unchecked.defaultof<System.Windows.Window>))
                factory.Looper
                factory.TimePointQueue
                factory.WorkEventStore
                factory.TelegramBot
                factory.WindowsMinimizer
                factory.ThemeSwitcher
                factory.UserSettings
                factory.MainErrorMessageQueue
                factory.DialogErrorMessageQueue
                factory.TimeProvider
                factory.ExcelBook
                factory.LoggerFactory

        let subscribe' mainModel : (SubId * Subscribe<_>) list =
            let dispatchMsgFromScenario dispatch =
                let dispatcherSubscription =
                    testDispatcher.DispatchRequested.Subscribe(fun msg -> dispatch msg)
                { new IDisposable with member _.Dispose() = dispatcherSubscription.Dispose() }

            let l = mainModel |> subscribe
            (["dispatchMsgFromScenario"], dispatchMsgFromScenario) :: l

        do
            Program.mkProgram 
                initMainModel 
                (fun (msg: MainModel.Msg) mainModel ->
                    let mainModel', mainModelCmd = mainModel |> updateMainModel msg
                    outMainModel.Value <- mainModel'
                    msgStack.Push(msg)
                    mainModel', mainModelCmd
                )
                (fun _ _ -> ())
            |> Program.withSubscription subscribe'
            |> Program.withTrace (fun msg _ _ -> TestContext.WriteLine(sprintf "trace: Program\n       Dispatched msg is:\n  %A" msg))
            |> Program.withTermination ((=) MainModel.Msg.Terminate) (fun _ -> ())
            |> Program.run

