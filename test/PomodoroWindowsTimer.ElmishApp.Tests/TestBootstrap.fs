namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Elmish
open NUnit.Framework

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.WpfClient
open System.Collections.Generic
open PomodoroWindowsTimer.Types
open Serilog
open PomodoroWindowsTimer.ElmishApp.Infrastructure

type TestBootstrap () =
    inherit Bootstrap()

    override _.PreConfigureServices(hostBuilder: HostBuilderContext,  services: IServiceCollection) =
        hostBuilder.Configuration["InTest"] <- "True"
        hostBuilder.Configuration["WorkDb:ConnectionString"] <- "Data Source=InMemorySample;Mode=Memory;Cache=Shared"

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
            .AddSingleton<IWindowsMinimizer>(WindowsMinimizer.initStub "") |> ignore

        services
            .AddSingleton<IThemeSwitcher>(
                ThemeSwitcherStub.create ()
            )
            |> ignore
        ()

    override _.ConfigureLogging(loggingBuilder: ILoggingBuilder) =
        loggingBuilder.SetMinimumLevel(LogLevel.Error) |> ignore

    override _.PostConfigureHost(_: IHostBuilder) =
         ()

    member this.StartTestElmishApp (outMainModel: ref<MainModel>, msgStack: Stack<MainModel.Msg>, testDispatcher: TestDispatcher) =
        let factory = base.GetElmishProgramFactory()
        let (initMainModel, updateMainModel, _, subscribe) =
            CompositionRoot.compose
                "Pomodoro Windows Timer under tests"
                Program.tickMilliseconds
                factory.WorkRepository
                factory.WorkEventRepository
                factory.TelegramBot
                factory.WindowsMinimizer
                factory.ThemeSwitcher
                factory.UserSettings
                factory.MainErrorMessageQueue
                factory.DialogErrorMessageQueue
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

