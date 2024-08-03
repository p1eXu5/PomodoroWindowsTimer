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
    
open PomodoroWindowsTimer.Testing.Fakers
open PomodoroWindowsTimer.ElmishApp.Tests

type internal Sut =
    {
        LooperMock: ILooper
        TimeProvider: System.TimeProvider
        Update: Work option -> PlayerModel.Msg -> PlayerModel -> (PlayerModel * Cmd<PlayerModel.Msg> * PlayerModel.Intent)
    }

let internal sutFactory () =
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
                (WorkEventStore.init (Substitute.For<IWorkEventRepository>()))
                (Substitute.For<IThemeSwitcher>())
                (Substitute.For<ITelegramBot>())
                (Substitute.For<IUserSettings>())
                (Substitute.For<ITimePointQueue>())
                (Substitute.For<IErrorMessageQueue>())
                (Substitute.For<ILogger<PlayerModel>>())
    }