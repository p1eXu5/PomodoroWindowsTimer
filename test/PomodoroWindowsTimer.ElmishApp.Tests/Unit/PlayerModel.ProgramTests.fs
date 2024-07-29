namespace PomodoroWindowsTimer.ElmishApp.Tests.Unit.PlayerModel

open PomodoroWindowsTimer.ElmishApp.Tests

module ProgramTests =

    open NUnit.Framework
    open Faqt
    open Faqt.Operators
    open p1eXu5.FSharp.Testing.ShouldExtensions

    open PomodoroWindowsTimer.ElmishApp.Models

    let private userSettings = UserSettingsStub ()
    let private initPlayerModel = PlayerModel.init userSettings

    [<Test>]
    let ``01: PostChangeActiveTimeSpan -> no current work -> returns no Intent`` () =
        let playerModel = initPlayerModel
        ()