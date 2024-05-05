module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.Then

open System
open Microsoft.Extensions.DependencyInjection

open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions
open NSubstitute

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE

open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers
open PomodoroWindowsTimer.Abstractions


let ``Looper TimePointStarted event has been despatched with`` (newTimePointId: Guid) (oldTimePointId: Guid option) =
    Common.``Looper TimePointStarted event has been despatched with`` newTimePointId oldTimePointId
    |> Scenario.log (
        sprintf "Then.``%s new TimePoint Id - %A and %s``."
            (nameof Common.``Looper TimePointStarted event has been despatched with``)
            newTimePointId
            (oldTimePointId |> Option.map (sprintf "old TimmePoint Id - %A") |> Option.defaultValue "no old TimePoint")
        )

let ``Looper TimePointReduced event has been despatched with`` (activeTimePointId: System.Guid) (expectedSeconds: float<sec>) (tolerance: float<sec>) =
    Common.``Looper TimePointReduced event has been despatched with`` activeTimePointId expectedSeconds tolerance
    |> Scenario.log (
        sprintf "Then.``%s %A %A sec with %A tolerance``."
            (nameof Common.``Looper TimePointReduced event has been despatched with``)
            activeTimePointId
            expectedSeconds
            tolerance
        )


/// Comparing Id's.
let rec ``Active Point is set on`` (timePoint: TimePoint) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        match sut.MainModel.ActiveTimePoint with
        | Some apt ->
            apt.Id |> shouldL equal timePoint.Id $"Expected Active TimePoint is {timePoint} baut was {apt}"
        | None ->
            assertionExn "Active TimePoint has not been set."
    }
    |> Scenario.log $"Then.``{nameof ``Active Point is set on``}``"


let rec ``Current Work is set on`` (expectedCurrentWork: Work) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        match sut.MainModel.CurrentWork with
        | Some actualCurrentWork ->
            actualCurrentWork.Work.Number |> should equal expectedCurrentWork.Number
            actualCurrentWork.Work.Title |> should equal expectedCurrentWork.Title
            // Created/UpdatedAt dates is different cause work is created in database
        | None ->
            assertionExn "CurrentWork has not been set."
    }
    |> Scenario.log $"Then.``{nameof ``Current Work is set on``}``"



let ``LooperState is`` (looperState: LooperState) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        sut.MainModel.LooperState
        |> shouldL equal looperState ("LooperState is not Initialized")
    }

[<Obsolete("Use ``MainModel.IsMinimized should be`` and ``WindowsMinimizer.MinimizeOtherAsync is called``")>]
let ``Windows should not be minimized`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        let wm = sut.MockRepository.Substitute<IWindowsMinimizer>()
        
        do
            wm.Received(0).MinimizeOtherAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        sut.MainModel.IsMinimized
        |> shouldL be False "MainModel.IsMinimized is not false"
    }

let rec ``MinimizeWindows msg has been dispatched`` () =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "MinimizeWindows" (fun msg ->
            match msg with
            | MainModel.Msg.WindowsMsg (WindowsMsg.MinimizeWindows) -> true
            | _ -> false
        )
    }
    |> Scenario.log $"Then.``{nameof ``MinimizeWindows msg has been dispatched``}``"

let rec ``MinimizeWindows msg has not been dispatched`` () =
    scenario {
        do! Scenario.msgNotDispatchedWithin1Sec "MinimizeWindows" (fun msg ->
            match msg with
            | MainModel.Msg.WindowsMsg (WindowsMsg.MinimizeWindows) -> true
            | _ -> false
        )
    }
    |> Scenario.log $"Then.``{nameof ``MinimizeWindows msg has been dispatched``}``"

let ``WindowsMinimizer.MinimizeOtherAsync is called`` (times: int) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        let wm = sut.MockRepository.Substitute<IWindowsMinimizer>()
        
        do
            wm.Received(times).MinimizeOtherAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously
    }

[<Obsolete("Use ``MainModel.IsMinimized should be`` and ``WindowsMinimizer.MinimizeOtherAsync is called``")>]
let ``Windows should be minimized`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        let wm = sut.MockRepository.Substitute<IWindowsMinimizer>()

        (*
            TODO: uncoment when reduce window messages
            do
                wm.Received().MinimizeOtherAsync()
                |> Async.AwaitTask
                |> Async.RunSynchronously

            do
                wm.Received().RestoreMainWindow()

        *)

        sut.MainModel.IsMinimized
        |> shouldL be True "MainModel.IsMinimized is not true"
    }

let ``MainModel.IsMinimized should be`` (v: bool) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        sut.MainModel.IsMinimized
        |> shouldL equal v $"MainModel.IsMinimized is {sut.MainModel.IsMinimized}"
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

let rec ``Theme should been switched with`` (timePointKind: TimePointKind) (times: int) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let themeSwitcher = sut.ServiceProvider.GetRequiredService<IThemeSwitcher>()

        themeSwitcher.Received(times).SwitchTheme(timePointKind)
    }

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

        match sut.MainModel.ActiveTimePoint with
        | Some atp ->
            let tolerance = tolerance |> Option.map (float >> (*) 1000.0 ) |> Option.defaultValue (float Program.tickMilliseconds)
            // reminder can be appended to the next tp
            let timePointAddTick = timePoint.TimeSpan.Add(TimeSpan.FromMilliseconds(tolerance))
            atp.TimeSpan |> shouldL be (lessThanOrEqualTo timePointAddTick) $"Active TimePoint is %A{atp}"
        | None ->
            assertionExn "Active TimePoint is not set."
    }
    |> Scenario.log $"Then.``{nameof ``Active Point remaining time is equal to or less then``} {timePoint.TimeSpan}``"

let ``Active TimePoint remaining time is equal to`` (seconds: float<sec>) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        match sut.MainModel.ActiveTimePoint with
        | Some atp ->
            atp.TimeSpan.TotalSeconds
            |> shouldL equal (float seconds)
                $"Active TimePoint time: {atp.TimeSpan.TotalSeconds} sec., expected time: {seconds} sec."
        | None ->
            assertionExn "Active TimePoint has not been set"
    }
    |> Scenario.log "Then.``Active TimePoint remaining time is equal to``"


let rec ``Have no work events in db within`` (workId: uint64) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let workEventRepository = sut.ServiceProvider.GetRequiredService<IWorkEventRepository>()

        match workEventRepository.ReadAll workId with
        | Ok workEvents ->
            workEvents |> shouldL be Empty $"Work {workId} have db events"
        | Error err ->
            assertionExn err
    }
    |> Scenario.log $"Then.``{nameof ``Have no work events in db within``}`` {workId} work Id"


let rec ``Work events in db exist`` (workId: uint64) (eventExpr: #Quotations.Expr seq) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let workEventRepository = sut.ServiceProvider.GetRequiredService<IWorkEventRepository>()

        match workEventRepository.ReadAll workId with
        | Ok workEvents ->
            workEvents |> Seq.length |> shouldL equal (eventExpr |> Seq.length) $"Actual work event length is {workEvents |> Seq.length}"
            Seq.zip workEvents eventExpr
            |> Seq.iter (fun (ev, expr) -> ev |> should be (ofCase expr))
        | Error err ->
            assertionExn err
    }
    |> Scenario.log $"Then.``{nameof ``Work events in db exist``}`` {workId} work Id"


