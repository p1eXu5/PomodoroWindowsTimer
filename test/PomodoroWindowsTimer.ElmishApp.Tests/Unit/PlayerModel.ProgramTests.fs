namespace PomodoroWindowsTimer.ElmishApp.Tests.Unit.PlayerModel

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
            Update: Work option -> PlayerModel.Msg -> PlayerModel -> (PlayerModel * Cmd<PlayerModel.Msg> * PlayerModel.Intent)
        }

    let private sutFactory () =
        let looperMock = Substitute.For<ILooper>()
        {
            LooperMock = looperMock
            Update =
                PlayerModel.Program.update
                    looperMock
                    (Substitute.For<IWindowsMinimizer>())
                    (Substitute.For<System.TimeProvider>())
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
        %intent.Should().Be(PlayerModel.Intent.SkipOrApplyMissingTime (currentWork.Id, TimeSpan.FromSeconds(3), timePoint.Kind))
        sut.LooperMock.Received(1).Resume()

    let statesTestCases : System.Collections.IEnumerable =
        seq {
            TestCaseData(LooperState.Initialized)
            TestCaseData(LooperState.Stopped)
        }

    [<TestCaseSource(nameof statesTestCases)>]
    [<Category("shifting forward")>]
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
        %intent.Should().Be(PlayerModel.Intent.SkipOrApplyMissingTime (currentWork.Id, TimeSpan.FromSeconds(3), timePoint.Kind))
        sut.LooperMock.DidNotReceive().Resume()

    // -------------------------------

    [<Test>]
    [<Category("shifting backward")>]
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
    let ``03-1: PostChangeActiveTimeSpan Start -> some current work, shifting backward, preshift state is playing -> resume, cmd, no intent`` () =
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
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (Work.generate () |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel.LooperState.Should().Be(LooperState.Playing)
        %afterPlayerModel.RetrieveWorkSpentTimesState.Should().BeOfCase(AsyncDeferredState.InProgress)

        %cmd.Should().HaveLength(1)
        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Resume()

    [<TestCaseSource(nameof statesTestCases)>]
    [<Category("shifting backward")>]
    let ``03-2: PostChangeActiveTimeSpan Start -> no current work, shifting backward, preshift state is nor playing -> no resume call, no cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

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
    let ``03-3: PostChangeActiveTimeSpan Start -> some current work, shifting backward, preshift state is playing -> no resume call, cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10))
        let beforePlayerModel : PlayerModel =
            {
                ActiveTimePoint = timePoint |> TimePoint.toActiveTimePoint |> Some

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