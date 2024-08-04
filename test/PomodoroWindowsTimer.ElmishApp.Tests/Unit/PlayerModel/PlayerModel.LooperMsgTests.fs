namespace PomodoroWindowsTimer.ElmishApp.Tests.Unit.PlayerModel

open System
open System.Threading

open NUnit.Framework
open Faqt
open Faqt.Operators
open NSubstitute
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
    
open PomodoroWindowsTimer.Testing.Fakers
open PomodoroWindowsTimer.ElmishApp.Tests
open FsUnit.TopLevelOperators


module LooperMsgTests =

    let private beforePlayerModel timePoint : PlayerModel =
        {
            ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some
            LooperState = LooperState.Initialized
            LastAtpWhenPlayOrNextIsManuallyPressed = None
            ShiftAndPreShiftTimes = None
            DisableSkipBreak = false
            DisableMinimizeMaximizeWindows = true
            RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
        }

    // [<Test>]
    let ``01-0: LooperMsg -> error -> emits OnError msg, no intent`` () =
        ()