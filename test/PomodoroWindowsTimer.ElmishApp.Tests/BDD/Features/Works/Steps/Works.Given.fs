module PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps.Given

open System
open Microsoft.Extensions.DependencyInjection
open NUnit.Framework
open PomodoroWindowsTimer.Testing.Fakers

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps
open PomodoroWindowsTimer.ElmishApp.Abstractions

/// Implements the next steps:
///
/// 1. Given.``Stored TimePoints``
///
/// 2. Given.``Initialized Program``
///
/// 3. Common.``Looper TimePointStarted event has been despatched with``
let ``Program has been initialized without CurrentWork`` (timePoints: TimePoint list) =
    scenario {
        do! Given.``Stored TimePoints`` timePoints
        do! Given.``Initialized Program`` ()

        do! Common.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None

        return ()
    }

/// Implements the next steps:
///
/// 1. Given.``Stored TimePoints``
///
/// 2. Given.``Stored CurrentWork``
///
/// 3. Given.``Initialized Program``
///
/// 4. Common.``Looper TimePointStarted event has been despatched with``
///
/// 5. Common.``SetCurrentWorkIfNone msg has been dispatched with``
///
/// Returns `MainModel.CurrentWork.Value.Work`.
let ``Program has been initialized with CurrentWork`` (timePoints: TimePoint list) =
    scenario {
        let currentWork = Work.generate ()

        do! Given.``Stored TimePoints`` timePoints
        do! Given.``CurrentWork in UserSettings`` currentWork
        do! Given.``Initialized Program`` ()

        do! Common.``Looper TimePointStarted event has been despatched with`` timePoints[0].Id None
        do! Common.``SetCurrentWorkIfNone msg has been dispatched with`` currentWork

        let! (sut: ISut) = Scenario.getState

        return sut.MainModel.CurrentWork.Work.Value
    }

(* TODO:
let ``RollbackWorkStrategy is SubstractWorkAddBreak`` () =
    scenario {
        do! Scenario.replaceStateWith (fun f ->
            fun sut ->
                let (sut': #ISut) = f sut
                let userSettings = sut'.ServiceProvider.GetRequiredService<IUserSettings>()
                userSettings.RollbackWorkStrategy <- RollbackWorkStrategy.SubstractWorkAddBreak
                sut'
        )
    }
*)
