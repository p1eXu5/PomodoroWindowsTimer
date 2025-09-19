[<AutoOpen>]
module PomodoroWindowsTimer.ElmishApp.Tests.Unit.PlayerModel.Helpers

open Microsoft.Extensions.Logging
open NSubstitute
open Elmish

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models


type internal Sut =
    {
        LooperMock: ILooper
        TimeProvider: System.TimeProvider
        Update: PlayerModel.Msg -> PlayerModel -> (PlayerModel * Cmd<PlayerModel.Msg> * PlayerModel.Intent)
    }


module Sut =

    let internal initWithWorkEventStore (workEventStore: WorkEventStore) : Sut =
        let looperMock = Substitute.For<ILooper>()
        let timeProvider = Substitute.For<System.TimeProvider>()
        {
            LooperMock = looperMock
            TimeProvider = timeProvider
            Update =
                PlayerModel.Program.update
                    looperMock
                    (Substitute.For<IWindowsMinimizer>())
                    timeProvider
                    workEventStore
                    (Substitute.For<IThemeSwitcher>())
                    (Substitute.For<IPlayerUserSettings>())
                    (Substitute.For<ITimePointQueue>())
                    (Substitute.For<IErrorMessageQueue>())
                    (Substitute.For<ILogger<PlayerModel>>())
        }

    let internal init () =
        let mockWorkRepository = Substitute.For<IWorkRepository>()
        let mockWorkEventRepository = Substitute.For<IWorkEventRepository>()
        let mockActiveTimePointRepository = Substitute.For<IActiveTimePointRepository>()

        let mockRepositoryFactory = Substitute.For<IRepositoryFactory>()

        mockRepositoryFactory.GetWorkRepository()
            .Returns(mockWorkRepository)
            |> ignore

        mockRepositoryFactory.GetWorkEventRepository()
            .Returns(mockWorkEventRepository)
            |> ignore

        mockRepositoryFactory.GetActiveTimePointRepository()
            .Returns(mockActiveTimePointRepository)
            |> ignore

        initWithWorkEventStore (WorkEventStore.init mockRepositoryFactory)

