namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open System.Collections.Generic
open System.Collections.Concurrent
open System.Threading
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

open Elmish

open NUnit.Framework
open NSubstitute
open p1eXu5.AspNetCore.Testing
open p1eXu5.AspNetCore.Testing.Logging
open p1eXu5.AspNetCore.Testing.MockRepository
open PomodoroWindowsTimer.ElmishApp.Tests.LoggerExtensions

open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.Storage.Configuration
open PomodoroWindowsTimer.WpfClient


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

    let _lockObj = new Lock()

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

    /// <summary>
    /// Configures test logging.
    /// </summary>
    override _.ConfigureLogging(_: HostBuilderContext, loggingBuilder: ILoggingBuilder) =
        loggingBuilder
            .ClearProviders()
            .AddTestLogger(TestContextWriters.GetInstance<TestContext>(), LogOut.All)
            .SetMinimumLevel(LogLevel.Trace)
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("DbUp", LogLevel.Warning)
            .AddFilter("PomodoroWindowsTimer.Storage", LogLevel.Warning)
            .AddFilter("PomodoroWindowsTimer.TimePointQueue.TimePointQueue", LogLevel.Debug)
            .AddFilter("PomodoroWindowsTimer.Looper.Looper", LogLevel.Debug)
            .AddFilter("PomodoroWindowsTimer.ElmishApp.Models", LogLevel.Debug)
            .AddFilter("TestElmishProgram", LogLevel.Debug)
            |> ignore

    /// <summary>
    /// Adds MockRepository.
    /// </summary>
    override _.PostConfigureHost(builder: IHostBuilder) =
        base.PostConfigureHost(builder)
        builder.AddMockRepository(
            [ 
                Service<IThemeSwitcher>();
                Service<IWindowsMinimizer>();
            ],
            TestLogWriter(TestLogger<TestBootstrap>(TestContextWriters.GetInstance<TestContext>(), LogOut.All)),
            (fun mr -> mockRepository <- mr)
        )

    /// <summary>
    /// 1. Calls base.StartHost()
    ///
    /// 1. Obtains IDbSeeder and seeds database
    ///
    /// 2. Applies DB migrations through IDbMigrator
    /// </summary>
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

    /// 
    member _.StartTestElmishApp (
        outMainModel: ref<MainModel>,
        msgStack: ConcurrentStack<MainModel.Msg>,
        testDispatcher: TestDispatcher
    ) =
        // do this.WaitDbSeeding()

        let factory = base.GetElmishProgramFactory()

        let looper = factory.Looper
        let timePointQueue = factory.TimePointQueue
        let userSettings = factory.UserSettings

        let (initMainModel, updateMainModel, _, _) =
            CompositionRoot.compose
                "Pomodoro Windows Timer under tests"
                (Func<System.Windows.Window>(fun () -> Unchecked.defaultof<System.Windows.Window>))
                looper
                timePointQueue
                factory.WorkEventStore
                factory.TelegramBot
                factory.WindowsMinimizer
                factory.ThemeSwitcher
                userSettings
                factory.MainErrorMessageQueue
                factory.DialogErrorMessageQueue
                factory.TimeProvider
                factory.ExcelBook
                factory.LoggerFactory

        let logger = base.GetLoggerFactory().CreateLogger("TestElmishProgram");

        // subscriptions
        let subscribe _ : (SubId * Subscribe<_>) list =
            let looperSubscription dispatch =
                let onLooperEvt =
                    fun evt ->
                        logger.LogDebug($"Dispatching LooperEvent:\n{evt}...")
                        do dispatch (evt |> MainModel.Msg.LooperMsg)
                looper.AddSubscriber(onLooperEvt)
                { new IDisposable with 
                    member _.Dispose() =
                        ()
                }

            let timePointQueueTimePointsChangedSubscription dispatch =
                let onTimePointChanged timePoints =
                    do dispatch (timePoints |> MainModel.Msg.TimePointsChangedQueueMsg)
                timePointQueue.TimePointsChanged.Subscribe onTimePointChanged

            let timePointQueueTimePointsLoopComplettedSubscription dispatch =
                let onTimePointChanged () =
                    do dispatch MainModel.Msg.TimePointsLoopComplettedQueueMsg
                timePointQueue.TimePointsLoopCompletted.Subscribe onTimePointChanged

            let playerUserSettingsSubscription dispatch =
                let onSettingsChanged () =
                    do dispatch (MainModel.Msg.PlayerUserSettingsChanged)
                userSettings.PlayerUserSettingsChanged.Subscribe onSettingsChanged

            let dispatchMsgFromScenario dispatch =
                let dispatcherSubscription =
                    testDispatcher.DispatchRequested.Subscribe(fun msg -> dispatch msg)
                { new IDisposable with member _.Dispose() = dispatcherSubscription.Dispose() }

            [
                ["Looper"], looperSubscription
                ["TimePointQueue.TimePointsChanged"], timePointQueueTimePointsChangedSubscription
                ["TimePointQueue.TimePointsLoopCompletted"], timePointQueueTimePointsLoopComplettedSubscription
                ["PlayerUserSettings"], playerUserSettingsSubscription
                ["dispatchMsgFromScenario"], dispatchMsgFromScenario
            ]

        let syncDispatch dispatch msg =
            lock _lockObj (fun() ->
                msgStack.Push(msg)
                dispatch msg
            )


        async {
            do
                Program.mkProgram 
                    initMainModel 
                    updateMainModel
                    (fun m _ -> outMainModel.Value <- m )
                |> Program.withSubscription subscribe
                |> Program.withTrace (fun msg _ _ -> logger.LogMsg(msg))
                |> Program.withTermination ((=) MainModel.Msg.Terminate) (fun _ -> ())
                |> Program.withErrorHandler (fun (err, ex) -> logger.LogError(ex, err))
                |> Program.runWithDispatch syncDispatch ()
        }
        |> Async.Start

        factory.Looper.Start()

