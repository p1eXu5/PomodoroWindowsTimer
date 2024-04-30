module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.When

open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let ``Play msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Play |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Play" ((=) msg)
    }

let ``Stop msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Stop |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Stop" ((=) msg)
    }

let ``Resume msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Resume |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Resume" ((=) msg)
    }

let ``Next msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Next |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Next" ((=) msg)
    }

let ``Replay msg has been dispatched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let msg = MainModel.PlayerMsg.Replay |> MainModel.Msg.PlayerMsg
        do sut.Dispatcher.DispatchWithTimeout(msg)
        do! Scenario.msgDispatchedWithin2Sec "Replay" ((=) msg)
    }


//let ``Spent 2.5 ticks time`` () =
//    CommonSteps.``Spent 2.5 ticks time`` testDispatch


//let ``Looper is stopping`` () =
//    testDispatch.TriggerWithTimeout(Msg.Stop)

//let ``Looper is resuming`` () =
//    testDispatch.TriggerWithTimeout(Msg.Resume)

//let ``Looper is playing next`` () =
//    testDispatch.TriggerWithTimeout(Msg.Next)

//let ``Looper is replaying`` () =
//    testDispatch.TriggerWithTimeout(Msg.Replay)

//let ``TimePoint is changed on`` timePoint =
//    let tcs = TaskCompletionSource()
//    looper.AddSubscriber(fun ev ->
//        async {
//            match ev with
//            | LooperEvent.TimePointTimeReduced tp
//            | LooperEvent.TimePointStarted (tp, _) when tp.Id = timePoint.Id -> tcs.SetResult()
//            | _ -> ()
//        }
//    )
//    task {
//        let! _ = tcs.Task
//        return ()
//    }
//    |> Async.AwaitTask
//    |> Async.RunSynchronously