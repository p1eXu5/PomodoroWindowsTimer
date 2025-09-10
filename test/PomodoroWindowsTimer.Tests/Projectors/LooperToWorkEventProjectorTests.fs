namespace PomodoroWindowsTimer.Tests.Projectors

open System
open System.Collections

open NUnit.Framework
open Faqt
open Faqt.Operators
open FsToolkit.ErrorHandling
open p1eXu5.AspNetCore.Testing.Logging
open p1eXu5.FSharp.Testing.ShouldExtensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Testing.Fakers
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper

[<Category("Projector")>]
module LooperToWorkEventProjectorTests =

    let timePoints =
        "(w[.2]-b[.1])"

    let initSut () =
        ()

    [<Test>]
    let ``Work not set -> not work event repo methods are called`` () =
        result {
            let! timePoints = 
                "w[.2, Test Work]-b[.1, Test Break]"
                |> PatternParser.parse Alias.Defaults.all
                |> Result.map (PatternParsedItem.List.timePoints TimePointPrototype.defaults)

            use timePointQueue = new TimePointQueue(
                timePoints,
                TestLogger<TimePointQueue>(TestContextWriters.GetInstance<TestContext>())
            )

            use looper = new Looper(
                timePointQueue,
                System.TimeProvider.System,
                30<ms>,
                TestLogger<Looper>(TestContextWriters.GetInstance<TestContext>())
            )

            // let looperToWorkEventProjector = LooperToWorkEventProjector
            return Ok()
        }
        |> Result.runTest
