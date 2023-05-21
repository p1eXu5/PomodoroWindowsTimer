module PomodoroWindowsTimer.ElmishApp.Tests.BDD.CommonSteps

open PomodoroWindowsTimer.ElmishApp.Tests.TestServices

let ``Spent 2.5 ticks time`` (testDispatch: TestDispatch) =
    testDispatch.WaitTimeout()