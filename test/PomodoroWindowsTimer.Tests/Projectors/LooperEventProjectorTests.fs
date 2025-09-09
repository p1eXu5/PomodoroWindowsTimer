namespace PomodoroWindowsTimer.Tests.Projectors

open System
open System.Collections

open NUnit.Framework
open Faqt
open Faqt.Operators

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Testing.Fakers

[<Category("Projector")>]
module LooperEventProjectorTests =

    let timePoints =
        "(w[.2]-b[.1])"

    let initSut () =
        ()

    [<Test>]
    let ``Work not set -> not work event repo methods are called`` () =
        let timePoints = "w[.2, Test Work]-b[.1, Test Break]" |> PatternParser.parse Alias.Defaults.all
        ()
