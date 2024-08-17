module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.Then

open System
open System.Threading
open Microsoft.Extensions.DependencyInjection

open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions
open NSubstitute
open p1eXu5.AspNetCore.Testing
open p1eXu5.AspNetCore.Testing.MockRepository
open Faqt
open Faqt.Operators

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE

open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers
open PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps


let ``Looper TimePointStarted event has been despatched with`` (newTimePointId: Guid) (oldTimePointId: Guid option) =
    Common.``Looper TimePointStarted event has been despatched with`` newTimePointId oldTimePointId
    |> Scenario.log (
        sprintf "Then.``%s new TimePoint Id - %A and %s``."
            (nameof Common.``Looper TimePointStarted event has been despatched with``)
            newTimePointId
            (oldTimePointId |> Option.map (sprintf "old TimmePoint Id - %A") |> Option.defaultValue "no old TimePoint")
        )

let ``Looper TimePointReduced event has been despatched with`` (timePointId: System.Guid) (expectedSeconds: float<sec>) (tolerance: float<sec>) =
    Common.``Looper TimePointReduced event has been despatched with`` timePointId expectedSeconds tolerance
    |> Scenario.log (
        sprintf "Then.``%s %A %A sec with %A tolerance``."
            (nameof Common.``Looper TimePointReduced event has been despatched with``)
            timePointId
            expectedSeconds
            tolerance
        )

/// Comparing Id's.
let rec ``Active Point is set on`` (timePoint: TimePoint) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        match sut.MainModel.Player.ActiveTimePoint with
        | Some apt ->
            apt.OriginalId |> shouldL equal timePoint.Id $"Expected Active TimePoint is {timePoint} baut was {apt}"
        | None ->
            assertionExn "Active TimePoint has not been set."
    }
    |> Scenario.log $"Then.``{nameof ``Active Point is set on``}``"

let ``LooperState is`` (looperState: LooperState) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        sut.MainModel.Player.LooperState
        |> shouldL equal looperState ("LooperState is not Initialized")
    }

[<Obsolete("Use ``MainModel.IsMinimized should be`` and ``WindowsMinimizer.MinimizeOtherAsync is called``")>]
let ``Windows should not be minimized`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        let wm = sut.MockRepository.Substitute<IWindowsMinimizer>()
        
        do
            wm.Received(0).MinimizeAllRestoreAppWindowAsync()
    }


let ``WindowsMinimizer.MinimizeOtherAsync is called`` (times: int) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        let wm = sut.MockRepository.Substitute<IWindowsMinimizer>()
        
        do
            wm.Received(times).MinimizeAllRestoreAppWindowAsync()
    }


let rec ``Telegrtam bot should not be notified`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let telegramBotStub = sut.ServiceProvider.GetRequiredService<ITelegramBot>() :?> TelegramBotStub

        telegramBotStub.MessageStack |> shouldL be Empty (nameof ``Telegrtam bot should not be notified``)
    }

let rec ``Theme should not been switched`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let themeSwitcher = sut.ServiceProvider.GetRequiredService<IThemeSwitcher>()

        themeSwitcher.ReceivedWithAnyArgs(0).SwitchTheme(Arg.Any<TimePointKind>())
    }

let rec ``Theme should been switched with`` (timePointKind: TimePointKind) (times: int<times>) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let themeSwitcher = sut.ServiceProvider.GetRequiredService<IThemeSwitcher>()

        themeSwitcher.Received(int times).SwitchTheme(timePointKind)
    }

// TODO: replace with mock check
let ``Telegrtam bot should be notified with`` (timePointName: string) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        do! Scenario.msgDispatchedWithin2Sec "SendToChatBot" (fun msg ->
            match msg with
            | MainModel.Msg.SendToChatBot msg ->
                msg.Contains(timePointName, StringComparison.Ordinal)
            | _ -> false
        )

        let telegramBotStub = sut.ServiceProvider.GetRequiredService<ITelegramBot>() :?> TelegramBotStub
        telegramBotStub.MessageStack |> shouldL not' (be Empty) ("TelegramBot message stack is empty")
    }

let rec ``Active Point remaining time is equal to or less then`` (timePoint: TimePoint) (tolerance: float<sec> option) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        match sut.MainModel.Player.ActiveTimePoint with
        | Some atp ->
            let tolerance = tolerance |> Option.map (float >> (*) 1000.0 ) |> Option.defaultValue (float Program.tickMilliseconds)
            // reminder can be appended to the next tp
            let timePointAddTick = timePoint.TimeSpan.Add(TimeSpan.FromMilliseconds(tolerance))
            atp.TimeSpan |> shouldL be (lessThanOrEqualTo timePointAddTick) $"Active TimePoint is %A{atp}"
        | None ->
            assertionExn "Active TimePoint is not set."
    }
    |> Scenario.log $"Then.``{nameof ``Active Point remaining time is equal to or less then``} {timePoint.TimeSpan}``"

let rec ``Active Point remaining time is less then`` (timePoint: TimePoint) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        match sut.MainModel.Player.ActiveTimePoint with
        | Some atp ->
            atp.RemainingTimeSpan |> shouldL be (lessThan timePoint.TimeSpan) $"Active TimePoint is %A{atp}"
        | None ->
            assertionExn "Active TimePoint is not set."
    }
    |> Scenario.log $"Then.``{nameof ``Active Point remaining time is less then``} {timePoint.TimeSpan}``"

let ``Active TimePoint remaining time is equal to`` (seconds: float<sec>) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        match sut.MainModel.Player.ActiveTimePoint with
        | Some atp ->
            atp.RemainingTimeSpan.TotalSeconds
            |> shouldL equal (float seconds)
                $"Active TimePoint time: {atp.RemainingTimeSpan.TotalSeconds} sec., expected time: {seconds} sec."
        | None ->
            assertionExn "Active TimePoint has not been set"
    }
    |> Scenario.log "Then.``Active TimePoint remaining time is equal to``"

let ``SkipOrApplyMissingTime dialog has been shown`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "SkipOrApplyMissingTime dialog has been shown" (fun mainModel ->
            match mainModel.AppDialog with
            | AppDialogModel.SkipOrApplyMissingTime _ -> true
            | _ -> false
        )
    }
    |> Scenario.log $"Then.``{nameof ``SkipOrApplyMissingTime dialog has been shown``}``"

let ``RollbackWork dialog has been shown`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "RollbackWork dialog has been shown" (fun mainModel ->
            match mainModel.AppDialog with
            | AppDialogModel.RollbackWork _ -> true
            | _ -> false
        )
    }
    |> Scenario.log $"Then.``{nameof ``RollbackWork dialog has been shown``}``"

let ``Dialog has been closed`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "Dialog has been closed" (fun mainModel ->
            match mainModel.AppDialog with
            | AppDialogModel.NoDialog -> true
            | _ -> false
        )
    }
    |> Scenario.log $"Then.``{nameof ``Dialog has been closed``}``"

let ``No event have been storred (with mock)`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        let mockWorkEventRepo = sut.MockRepository.Substitute<IWorkEventRepository>()
        do
            mockWorkEventRepo
                .DidNotReceiveWithAnyArgs()
                .InsertAsync (Arg.Any<WorkId>()) (Arg.Any<WorkEvent>()) (Arg.Any<CancellationToken>())
                |> Async.AwaitTask
                |> Async.Ignore
                |> Async.RunSynchronously
    }
    |> Scenario.log $"Then.``{nameof ``No event have been storred (with mock)``}``"

let ``BreakIncreased event has been storred (with mock)`` workIdKey time =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let work = sut.ScenarioContext[workIdKey] :?> Work
        do!
            Scenario.mockSatisfiesWithin2Sec "BreakIncreased event insertion" (fun mockRepo ->
                let mockWorkEventRepo = mockRepo.Substitute<IWorkEventRepository>()
                do
                    mockWorkEventRepo
                        .Received(1)
                        .InsertAsync
                            work.Id
                            (Verify.That<WorkEvent>(fun we ->
                                match we with
                                | BreakIncreased (_, t, _) ->
                                    %t.TotalSeconds.Should().BeInRange(float time, float (time + 0.5<sec>))
                                | _ -> assertionExn "Work event is not BreakIncreased"
                            ))
                            (Arg.Any<CancellationToken>())
                        |> Async.AwaitTask
                        |> Async.Ignore
                        |> Async.RunSynchronously
            )
    }

let ``WorkIncreased event has been storred (with mock)`` workIdKey time =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let work = sut.ScenarioContext[workIdKey] :?> Work
        do!
            Scenario.mockSatisfiesWithin2Sec "WorkIncreased event insertion" (fun mockRepo ->
                let mockWorkEventRepo = mockRepo.Substitute<IWorkEventRepository>()
                do
                    mockWorkEventRepo
                        .Received(1)
                        .InsertAsync
                            work.Id
                            (Verify.That<WorkEvent>(fun we ->
                                match we with
                                | WorkIncreased (_, t, _) ->
                                    %t.TotalSeconds.Should().BeInRange(float time, float (time + 0.5<sec>))
                                | _ -> assertionExn "Work event is not WorkIncreased"
                            ))
                            (Arg.Any<CancellationToken>())
                        |> Async.AwaitTask
                        |> Async.Ignore
                        |> Async.RunSynchronously
            )
    }

let ``No dialog has been shown`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "No dialog has been shown" (fun mainModel ->
            match mainModel.AppDialog with
            | AppDialogModel.NoDialog -> true
            | _ -> false
        )
    }
    |> Scenario.log $"Then.``{nameof ``No dialog has been shown``}``"

