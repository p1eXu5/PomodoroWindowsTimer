namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Collections.Concurrent

open NUnit.Framework
open p1eXu5.AspNetCore.Testing
open p1eXu5.AspNetCore.Testing.MockRepository

open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.WpfClient

type ISut =
    inherit IScenarioContext
    inherit IAsyncDisposable
    abstract ServiceProvider: IServiceProvider
    abstract Dispatcher: TestDispatcher with get
    abstract MainModel: MainModel with get
    abstract MsgStack: ConcurrentStack<MainModel.Msg> with get
    abstract MockRepository: MockRepository with get


module Sut =

    let run (setupf: ISut -> ISut) =

        let bootstrap =
            Bootstrap.Build<TestBootstrap>()

        let testDispatcher = new TestDispatcher()
        let dict = new Dictionary<string, obj>(5)
        let mainModel = ref Unchecked.defaultof<MainModel>
        let msgStack = ConcurrentStack<MainModel.Msg>()

        try
            bootstrap.StartHost()

            // make substitutions:
            let _ = bootstrap.MockRepository.TrySubstitute<IWindowsMinimizer>()
            let _ = bootstrap.MockRepository.TrySubstitute<IThemeSwitcher>()

            let sut =
                {
                    new ISut with
                        member _.Dispatcher with get() = testDispatcher
                        member _.MainModel with get() = mainModel.Value
                        member _.MsgStack with get() = msgStack
                        member _.ServiceProvider with get() =
                            bootstrap.Host.Services
                        member _.MockRepository with get() =
                            bootstrap.MockRepository
                    interface IScenarioContext with
                        member _.ScenarioContext with get() = dict
                    
                    interface IAsyncDisposable with
                        member this.DisposeAsync() =
                            task {
                                do testDispatcher.Dispatch(MainModel.Msg.Terminate)
                                do! bootstrap.StopHostAsync()
                                do (bootstrap :> IDisposable).Dispose()
                                return ()
                            }
                            |> ValueTask
                }
                |> setupf

            bootstrap.StartTestElmishApp(mainModel, msgStack, testDispatcher)

            sut
        with ex ->
            TestContext.WriteLine(ex)
            bootstrap.Host.StopAsync() |> Async.AwaitTask |> Async.RunSynchronously
            reraise ()
