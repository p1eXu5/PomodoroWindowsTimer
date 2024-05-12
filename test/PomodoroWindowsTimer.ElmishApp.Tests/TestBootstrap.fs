namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open System.Collections.Generic
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
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

type TestBootstrap () =
    inherit Bootstrap()
    let mutable _disposed = false

    let mutable mockRepository : MockRepository = Unchecked.defaultof<_>

    // in-memory database
    let mutable inMemoryConnection : SqliteConnection = Unchecked.defaultof<_>

    member _.MockRepository with get() = mockRepository
    
    member val MockWindowsMinimizer : IWindowsMinimizer = NSubstitute.Substitute.For<IWindowsMinimizer>() with get

    override _.Dispose(disposing: bool) =
        if not _disposed then
            if disposing then
                match inMemoryConnection with
                | null -> ()
                | _ -> inMemoryConnection.Dispose()

            _disposed <- true

        base.Dispose(disposing)

    override this.PreConfigureServices(hostBuilder: HostBuilderContext,  services: IServiceCollection) =
        hostBuilder.Configuration["InTest"] <- "True"

        let token = Guid.NewGuid().ToString("N")
        let connectionString = $"Data Source=workdb{token};Mode=Memory;Cache=Shared"
        inMemoryConnection <- new SqliteConnection(connectionString)
        inMemoryConnection.Open()

        hostBuilder.Configuration["WorkDb:ConnectionString"] <- connectionString

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

        services
            .AddSingleton<IUserSettings>(new UserSettingsStub()) |> ignore

        services
            .AddSingleton<ITelegramBot>(new TelegramBotStub()) |> ignore

        services
            .AddSingleton<Func<System.Windows.Window, IWindowsMinimizer>>(fun _ ->
                Func<System.Windows.Window, IWindowsMinimizer>(fun _ -> this.MockWindowsMinimizer)
            )
            |> ignore

        services
            .AddSingleton<IThemeSwitcher>(
                ThemeSwitcherStub.create ()
            )
            |> ignore
        ()

    override _.ConfigureLogging(loggingBuilder: ILoggingBuilder) =
        loggingBuilder.SetMinimumLevel(LogLevel.Error) |> ignore

    override _.PostConfigureHost(builder: IHostBuilder) =
        builder.AddMockRepository(
            [ Service<IThemeSwitcher>() ],
            TestLogWriter(TestLogger<TestBootstrap>(TestContextWriters.Default, LogOut.All)),
            (fun mr -> mockRepository <- mr)
        )


    member this.StartTestElmishApp (outMainModel: ref<MainModel>, msgStack: Stack<MainModel.Msg>, testDispatcher: TestDispatcher) =
        do this.WaitDbSeeding()
        
        let factory = base.GetElmishProgramFactory()
        let (initMainModel, updateMainModel, _, subscribe) =
            CompositionRoot.compose
                "Pomodoro Windows Timer under tests"
                (Func<System.Windows.Window>(fun () -> Unchecked.defaultof<System.Windows.Window>))
                factory.Looper
                factory.TimePointQueue
                factory.WorkRepository
                factory.WorkEventRepository
                factory.TelegramBot
                (factory.WindowsMinimizer.Invoke Unchecked.defaultof<System.Windows.Window>)
                factory.ThemeSwitcher
                factory.UserSettings
                factory.MainErrorMessageQueue
                factory.DialogErrorMessageQueue
                factory.TimeProvider
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

