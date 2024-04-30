namespace PomodoroWindowsTimer.ElmishApp.Tests

open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp

type TestDispatcher () =
    let timeout = ((int Program.tickMilliseconds) * 2 + ((int Program.tickMilliseconds) / 2))

    let dispatchRequestedEvent = new Event<_>()

    [<CLIEvent>]
    member _.DispatchRequested : IEvent<MainModel.Msg> = dispatchRequestedEvent.Publish

    member _.Dispatch(message: MainModel.Msg) =
       dispatchRequestedEvent.Trigger(message)

    member this.DispatchWithTimeout(message: MainModel.Msg) =
        async {
           dispatchRequestedEvent.Trigger(message)
           do! Async.Sleep timeout
        }
        |> Async.RunSynchronously

     member this.WaitTimeout() =
        async {
           do! Async.Sleep timeout
        }
        |> Async.RunSynchronously

