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


module PlayTests =

    let private beforePlayerModel : PlayerModel =
        {
            ActiveTimePoint = None
            LooperState = LooperState.Initialized
            LastAtpWhenPlayOrNextIsManuallyPressed = None
            ShiftAndPreShiftTimes = None
            DisableSkipBreak = false
            DisableMinimizeMaximizeWindows = true
            RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
        }

    [<Test>]
    let ``01: Play -> initialized state, no atp, no work -> calls looper Next`` () =
        let sut = Sut.init ()

        // act
        let (afterPlayerModel, cmd, intent) = beforePlayerModel |> sut.Update None PlayerModel.Msg.Play

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    LooperState = LooperState.Playing
                    LastAtpWhenPlayOrNextIsManuallyPressed = None
                }
            )
        %cmd.Should().BeEmpty()
        %intent.Should().Be(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Next()

    [<Test>]
    let ``02: Play -> initialized state, atp, no work -> calls looper Next`` () =
        let sut = Sut.init ()
        let beforePlayerModel = { beforePlayerModel with ActiveTimePoint = Some (ActiveTimePoint.generate ()) }

        // act
        let (afterPlayerModel, cmd, intent) = beforePlayerModel |> sut.Update None PlayerModel.Msg.Play

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    LooperState = LooperState.Playing
                    LastAtpWhenPlayOrNextIsManuallyPressed = Some (beforePlayerModel.ActiveTimePoint.Value.Id)
                }
            )
        %cmd.Should().BeEmpty()
        %intent.Should().Be(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Next()
