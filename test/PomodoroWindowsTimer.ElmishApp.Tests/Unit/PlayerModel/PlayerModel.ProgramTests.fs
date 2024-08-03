namespace PomodoroWindowsTimer.ElmishApp.Tests.Unit.PlayerModel

open System.ComponentModel

module ProgramTests =

    open System
    open Microsoft.Extensions.Logging

    open NUnit.Framework
    open Faqt
    open Faqt.Operators
    open p1eXu5.FSharp.Testing.ShouldExtensions
    open NSubstitute

    open Elmish
    open Elmish.Extensions

    open PomodoroWindowsTimer
    open PomodoroWindowsTimer.Abstractions
    open PomodoroWindowsTimer.Types
    open PomodoroWindowsTimer.ElmishApp
    open PomodoroWindowsTimer.ElmishApp.Abstractions
    open PomodoroWindowsTimer.ElmishApp.Models
    
    open PomodoroWindowsTimer.Testing.Fakers
    open PomodoroWindowsTimer.ElmishApp.Tests

    type private Sut =
        {
            LooperMock: ILooper
            TimeProvider: System.TimeProvider
            Update: Work option -> PlayerModel.Msg -> PlayerModel -> (PlayerModel * Cmd<PlayerModel.Msg> * PlayerModel.Intent)
        }

    let private sutFactory () =
        let looperMock = Substitute.For<ILooper>()
        let timeProvider = Substitute.For<System.TimeProvider>()
        {
            LooperMock = looperMock
            TimeProvider = timeProvider
            Update =
                PlayerModel.Program.update
                    looperMock
                    (Substitute.For<IWindowsMinimizer>())
                    timeProvider
                    (WorkEventStore.init (Substitute.For<IWorkEventRepository>()))
                    (Substitute.For<IThemeSwitcher>())
                    (Substitute.For<ITelegramBot>())
                    (Substitute.For<IUserSettings>())
                    (Substitute.For<ITimePointQueue>())
                    (Substitute.For<IErrorMessageQueue>())
                    (Substitute.For<ILogger<PlayerModel>>())
        }

    [<TestCase(0.0)>]
    [<TestCase(0.9)>]
    [<TestCase(-0.9)>]
    [<Category("no shifting")>]
    [<Category("PlayerModel")>]
    let ``01-0: PostChangeActiveTimeSpan Start -> no current work, shifting less than 1 sec, preshift state is playing -> resume, no cmd, no intent`` (offset: float) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (LooperState.Playing)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = (5.0 + offset) * 1.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update None (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = LooperState.Playing
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Resume()

    [<TestCase(0.0)>]
    [<TestCase(0.9)>]
    [<TestCase(-0.9)>]
    [<Category("no shifting")>]
    [<Category("PlayerModel")>]
    let ``01-1: PostChangeActiveTimeSpan Start -> some current work, shifting less than 1 sec, preshift state is playing -> resume, no cmd, no intent`` (offset: float) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (LooperState.Playing)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = (5.0 + offset) * 1.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (Work.generate () |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = LooperState.Playing
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Resume()


    let offsetStateTestCases : System.Collections.IEnumerable =
        seq {
            TestCaseData(0.0, LooperState.Initialized)
            TestCaseData(0.0, LooperState.Stopped)
            TestCaseData(0.9, LooperState.Initialized)
            TestCaseData(0.9, LooperState.Stopped)
            TestCaseData(-0.9, LooperState.Initialized)
            TestCaseData(-0.9, LooperState.Stopped)
        }

    [<TestCaseSource(nameof offsetStateTestCases)>]
    [<Category("no shifting")>]
    [<Category("PlayerModel")>]
    let ``01-2: PostChangeActiveTimeSpan Start -> no current work, shifting less than 1 sec, preshift state is not playing -> no resume call, no cmd, no intent`` (offset: float, state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (state)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = (5.0 + offset) * 1.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update None (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.DidNotReceive().Resume()

    [<TestCaseSource(nameof offsetStateTestCases)>]
    [<Category("no shifting")>]
    [<Category("PlayerModel")>]
    let ``01-3: PostChangeActiveTimeSpan Start -> some current work, shifting less than 1 sec, preshift state is not playing -> no resume call, no cmd, no intent`` (offset: float, state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (state)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = (5.0 + offset) * 1.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (Work.generate () |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.DidNotReceive().Resume()

    // -------------------------------

    [<Test>]
    [<Category("shifting forward")>]
    [<Category("PlayerModel")>]
    let ``02-0: PostChangeActiveTimeSpan Start -> no current work, shifting forward, preshift state is playing -> resume, no cmd, no intent`` () =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (LooperState.Playing)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = 2.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update None (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = LooperState.Playing
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Resume()

    [<Test>]
    [<Category("shifting forward")>]
    [<Category("PlayerModel")>]
    let ``02-1: PostChangeActiveTimeSpan Start -> some current work, shifting forward, preshift state is playing -> resume, no cmd, SkipOrApply intent`` () =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (LooperState.Playing)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = 2.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }

        let currentWork = Work.generate ()
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (currentWork |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = LooperState.Playing
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().Be(PlayerModel.Intent.SkipOrApplyMissingTime (currentWork.Id, timePoint.Kind, TimeSpan.FromSeconds(3)))
        sut.LooperMock.Received(1).Resume()

    let statesTestCases : System.Collections.IEnumerable =
        seq {
            TestCaseData(LooperState.Initialized)
            TestCaseData(LooperState.Stopped)
        }

    [<TestCaseSource(nameof statesTestCases)>]
    [<Category("shifting forward")>]
    [<Category("PlayerModel")>]
    let ``02-2: PostChangeActiveTimeSpan Start -> no current work, shifting forward, preshift state is nor playing -> no resume call, no cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (state)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = 2.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update None (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.DidNotReceive().Resume()

    [<TestCaseSource(nameof statesTestCases)>]
    [<Category("shifting forward")>]
    [<Category("PlayerModel")>]
    let ``02-3: PostChangeActiveTimeSpan Start -> some current work, shifting forward, preshift state is not playing -> no resume call, no cmd, SkipOrApply intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (state)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = 2.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }

        let currentWork = Work.generate ()
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (currentWork |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().Be(PlayerModel.Intent.SkipOrApplyMissingTime (currentWork.Id, timePoint.Kind, TimeSpan.FromSeconds(3)))
        sut.LooperMock.DidNotReceive().Resume()

    // -------------------------------

    [<Test>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``03-0: PostChangeActiveTimeSpan Start -> no current work, shifting backward, preshift state is playing -> resume, no cmd, no intent`` () =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

                LooperState = LooperState.TimeShifting (LooperState.Playing)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = 8.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update None (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = LooperState.Playing
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Resume()

    [<Test>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``03-1: PostChangeActiveTimeSpan Start -> some current work, shifting backward, preshift state is playing -> resume, cmd, no intent`` () =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePointWithSec 8.0<sec> |> Some

                LooperState = LooperState.TimeShifting (LooperState.Playing)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = 8.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (Work.generate () |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel.LooperState.Should().Be(LooperState.Playing)
        %afterPlayerModel.RetrieveWorkSpentTimesState.Should().BeOfCase(AsyncDeferredState.InProgress)

        %cmd.Should().HaveLength(1)
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Resume()

    [<TestCaseSource(nameof statesTestCases)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``03-2: PostChangeActiveTimeSpan Start -> no current work, shifting backward, preshift state is nor playing -> no resume call, no cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePointWithSec 8.0<sec> |> Some

                LooperState = LooperState.TimeShifting (state)
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = 8.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update None (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.DidNotReceive().Resume()

    [<TestCaseSource(nameof statesTestCases)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``03-3: PostChangeActiveTimeSpan Start -> some current work, shifting backward, preshift state is playing -> no resume call, cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePointWithSec 8.0<sec> |> Some

                LooperState = LooperState.TimeShifting state
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes =
                    {
                        PreShiftActiveRemainingSeconds = 5.0<sec>
                        NewActiveRemainingSeconds = 8.0<sec>
                    }
                    |> Some

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
            }
        let sut = sutFactory ()

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (Work.generate () |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel.LooperState.Should().Be(state)
        %afterPlayerModel.RetrieveWorkSpentTimesState.Should().BeOfCase(AsyncDeferredState.InProgress)

        %cmd.Should().HaveLength(1)
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.DidNotReceive().Resume()

    // ===============================

    let private notShiftingStates : System.Collections.IEnumerable =
        seq {
            TestCaseData(LooperState.Initialized)
            TestCaseData(LooperState.Stopped)
            TestCaseData(LooperState.Playing)
        }

    [<TestCaseSource(nameof notShiftingStates)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``04-0: PostChangeActiveTimeSpan Finish -> error -> emits OnError msg, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let retrieveWorkSpentTimesState, cts =
            AsyncDeferredState.NotRequested
            |> AsyncDeferredState.forceInProgressWithCancellation

        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePointWithSec 8.0<sec> |> Some

                LooperState = state
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes = None

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = retrieveWorkSpentTimesState
            }
        let sut = sutFactory ()
        let currentWork = Work.generate ()

        // act
        let (afterPlayerModel, cmd, intent) =
            beforePlayerModel
            |> sut.Update
                (currentWork |> Some)
                (AsyncOperation.finishWithin PlayerModel.Msg.PostChangeActiveTimeSpan cts (Error "test error"))

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                    RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
                }
            )

        %cmd.Should().HaveLength(1)
        do cmd[0] (fun msg -> %msg.Should().BeOfCase(PlayerModel.Msg.OnError))

        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.DidNotReceive().Resume()

    [<TestCaseSource(nameof notShiftingStates)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``04-1: PostChangeActiveTimeSpan Finish -> ok with empty -> no cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let retrieveWorkSpentTimesState, cts =
            AsyncDeferredState.NotRequested
            |> AsyncDeferredState.forceInProgressWithCancellation

        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePointWithSec 8.0<sec> |> Some

                LooperState = state
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes = None

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = retrieveWorkSpentTimesState
            }
        let sut = sutFactory ()
        let currentWork = Work.generate ()

        // act
        let (afterPlayerModel, cmd, intent) =
            beforePlayerModel
            |> sut.Update
                (currentWork |> Some)
                (AsyncOperation.finishWithin PlayerModel.Msg.PostChangeActiveTimeSpan cts (Ok []))

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                    RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
                }
            )

        %cmd.Should().HaveLength(1)
        do cmd[0] (fun msg -> %msg.Should().Be(PlayerModel.Msg.OnError "Work spent time list is unexpected empty!"))

        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.DidNotReceive().Resume()

    [<TestCaseSource(nameof notShiftingStates)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``04-3: PostChangeActiveTimeSpan Finish -> ok with single work -> no cmd, RollbackTime intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let retrieveWorkSpentTimesState, cts =
            AsyncDeferredState.NotRequested
            |> AsyncDeferredState.forceInProgressWithCancellation

        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePointWithSec 8.0<sec> |> Some

                LooperState = state
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes = None

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = retrieveWorkSpentTimesState
            }
        let sut = sutFactory ()

        let nowDate = System.TimeProvider.System.GetUtcNow()
        %sut.TimeProvider.GetUtcNow().Returns(nowDate)

        let currentWork = Work.generate ()
        let workSpentTime =
            {
                Work = currentWork
                SpentTime = TimeSpan.FromSeconds(3)
            }

        // act
        let (afterPlayerModel, cmd, intent) =
            beforePlayerModel
            |> sut.Update
                (currentWork |> Some)
                (AsyncOperation.finishWithin PlayerModel.Msg.PostChangeActiveTimeSpan cts (Ok [ workSpentTime ]))

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                    RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().Be(PlayerModel.Intent.RollbackTime (workSpentTime, timePoint.Kind, nowDate))
        sut.LooperMock.DidNotReceive().Resume()

    [<TestCaseSource(nameof notShiftingStates)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``04-4: PostChangeActiveTimeSpan Finish -> ok with multiple works -> no cmd, MultipleRollbackTime intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let retrieveWorkSpentTimesState, cts =
            AsyncDeferredState.NotRequested
            |> AsyncDeferredState.forceInProgressWithCancellation

        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePointWithSec 8.0<sec> |> Some

                LooperState = state
                LastAtpWhenPlayOrNextIsManuallyPressed = None

                ShiftAndPreShiftTimes = None

                DisableSkipBreak = false
                DisableMinimizeMaximizeWindows = true

                RetrieveWorkSpentTimesState = retrieveWorkSpentTimesState
            }
        let sut = sutFactory ()

        let nowDate = System.TimeProvider.System.GetUtcNow()
        %sut.TimeProvider.GetUtcNow().Returns(nowDate)

        let previousWork = Work.generate ()
        let currentWork = Work.generate ()

        let previousWorkSpentTime =
            {
                Work = previousWork
                SpentTime = TimeSpan.FromSeconds(1)
            }
        let currentWorkSpentTime =
            {
                Work = currentWork
                SpentTime = TimeSpan.FromSeconds(2)
            }

        // act
        let (afterPlayerModel, cmd, intent) =
            beforePlayerModel
            |> sut.Update
                (currentWork |> Some)
                (AsyncOperation.finishWithin PlayerModel.Msg.PostChangeActiveTimeSpan cts (Ok [ previousWorkSpentTime; currentWorkSpentTime ]))

        // assert
        %afterPlayerModel
            .Should()
            .Be(
                { beforePlayerModel with
                    ShiftAndPreShiftTimes = None
                    LooperState = state
                    RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
                }
            )

        %cmd.Should().BeEmpty()
        %intent.Should().Be(PlayerModel.Intent.MultipleRollbackTime ([ previousWorkSpentTime; currentWorkSpentTime ], timePoint.Kind, nowDate))
        sut.LooperMock.DidNotReceive().Resume()

