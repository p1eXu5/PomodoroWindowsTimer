namespace PomodoroWindowsTimer.ElmishApp.Tests

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

/// <summary>
/// Defines <see cref="TestDispatcher.DispatchRequested" /> event that triggers
/// <c>dispatchMsgFromScenario</c> subscription.
/// </summary>
type TestDispatcher () =
    let timeout = ((int Program.tickMilliseconds) * 2 + ((int Program.tickMilliseconds) / 2))

    let dispatchRequestedEvent = new Event<_>()

    [<CLIEvent>]
    member _.DispatchRequested : IEvent<MainModel.Msg> = dispatchRequestedEvent.Publish

    member _.Dispatch(message: MainModel.Msg) =
       dispatchRequestedEvent.Trigger(message)

    member _.DispatchWithTimeout(message: MainModel.Msg) =
        async {
           dispatchRequestedEvent.Trigger(message)
           do! Async.Sleep timeout
        }
        |> Async.RunSynchronously

    /// <summary>
    /// Waits <c>2 * tickMilliseconds + (tickMilliseconds / 2)</c> times.
    /// </summary>
    /// <seealso cref="Program.tickMilliseconds" />
    member _.WaitTimeout() =
        async {
           do! Async.Sleep timeout
        }
        |> Async.RunSynchronously

    member _.WaitTimeout(seconds: float<sec>) =
        async {
           do! Async.Sleep (int (float seconds * 1000.0))
        }
        |> Async.RunSynchronously

