namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open System.Threading.Tasks
open System.Collections.Generic
open NUnit.Framework
open PomodoroWindowsTimer.WpfClient
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Abstractions

type ISut =
    inherit IScenarioContext
    inherit IAsyncDisposable
    abstract ServiceProvider: IServiceProvider
    abstract Dispatcher: TestDispatcher with get
    abstract MainModel: MainModel with get
    abstract MsgStack: Stack<MainModel.Msg> with get


module Sut =

    let run (setupf: ISut -> ISut) =

        let bootstrap =
            Bootstrap.Build<TestBootstrap>()

        let testDispatcher = new TestDispatcher()
        let dict = new Dictionary<string, obj>(5)
        let mainModel = ref Unchecked.defaultof<MainModel>
        let msgStack = Stack<MainModel.Msg>()

        try
            bootstrap.StartHost()

            let sut =
                {
                    new ISut with
                        member _.Dispatcher with get() = testDispatcher
                        member _.MainModel with get() = mainModel.Value
                        member _.MsgStack with get() = msgStack
                        member _.ServiceProvider with get() =
                            bootstrap.Host.Services
                    interface IScenarioContext with
                        member _.ScenarioContext with get() = dict
                    
                    interface IAsyncDisposable with
                        member this.DisposeAsync() =
                            task {
                                testDispatcher.Dispatch(MainModel.Msg.Terminate)
                                do! bootstrap.StopHostAsync()
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
