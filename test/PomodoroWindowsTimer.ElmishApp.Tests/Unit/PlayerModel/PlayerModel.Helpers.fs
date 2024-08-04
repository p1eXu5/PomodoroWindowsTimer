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
        Update: Work option -> PlayerModel.Msg -> PlayerModel -> (PlayerModel * Cmd<PlayerModel.Msg> * PlayerModel.Intent)
    }


module Sut =

    let internal initWithWorkEventStore (workEventStore: WorkEventStore) =
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
                    (Substitute.For<ITelegramBot>())
                    (Substitute.For<IUserSettings>())
                    (Substitute.For<ITimePointQueue>())
                    (Substitute.For<IErrorMessageQueue>())
                    (Substitute.For<ILogger<PlayerModel>>())
        }

    let internal init () =
        initWithWorkEventStore (WorkEventStore.init (Substitute.For<IWorkEventRepository>()) (Substitute.For<IActiveTimePointRepository>()))

