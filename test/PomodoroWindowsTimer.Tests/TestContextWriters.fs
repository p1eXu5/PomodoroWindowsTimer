[<AutoOpen>]
module PomodoroWindowsTimer.Tests.TestContextWriters

open NUnit.Framework
open p1eXu5.AspNetCore.Testing.Logging

let tcw = TestContextWriters.DefaultWith(TestContext.Progress, TestContext.Out)