namespace PomodoroWindowsTimer.ElmishApp.Tests.Unit.PlayerModel

open PomodoroWindowsTimer.Abstractions

module PostChangeActiveTimeSpanTests =

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

    [<TestCase(0.0)>]
    [<TestCase(0.9)>]
    [<TestCase(-0.9)>]
    [<Category("no shifting")>]
    [<Category("PlayerModel")>]
    let ``01-0: PostChangeActiveTimeSpan Start -> no current work, shifting less than 1 sec, preshift state is playing -> resume, no cmd, no intent`` (offset: float) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

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
    let ``01-1: PostChangeActiveTimeSpan Start -> some current work, shifting less than 1 sec, preshift state is playing -> resume, store strarted cmd, no intent`` (offset: float) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

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

        %cmd.Should().HaveLength(1)
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
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

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
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

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
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

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
    let ``02-1: PostChangeActiveTimeSpan Start -> some current work, shifting forward, preshift state is playing -> resume, store started cmd, SkipOrApply intent`` () =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()
        let now = System.TimeProvider.System.GetUtcNow()
        %sut.TimeProvider.GetUtcNow().Returns(now)

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

        %cmd.Should().HaveLength(1)
        %intent.Should().Be(PlayerModel.Intent.SkipOrApplyMissingTime (currentWork, timePoint.Kind, beforePlayerModel.ActiveTimePoint.Value.Id, TimeSpan.FromSeconds(3L), now))
        sut.LooperMock.Received(1).Resume()

    let initializedAndStoppedStates : System.Collections.IEnumerable =
        seq {
            TestCaseData(LooperState.Initialized)
            TestCaseData(LooperState.Stopped)
        }

    [<TestCaseSource(nameof initializedAndStoppedStates)>]
    [<Category("shifting forward")>]
    [<Category("PlayerModel")>]
    let ``02-2: PostChangeActiveTimeSpan Start -> no current work, shifting forward, preshift state is not playing -> no resume call, no cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

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

    [<TestCaseSource(nameof initializedAndStoppedStates)>]
    [<Category("shifting forward")>]
    [<Category("PlayerModel")>]
    let ``02-3: PostChangeActiveTimeSpan Start -> some current work, shifting forward, preshift state is not playing -> no resume call, no cmd, SkipOrApply intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()
        let now = System.TimeProvider.System.GetUtcNow()
        %sut.TimeProvider.GetUtcNow().Returns(now)

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
        %intent.Should().Be(PlayerModel.Intent.SkipOrApplyMissingTime (currentWork, timePoint.Kind, beforePlayerModel.ActiveTimePoint.Value.Id, TimeSpan.FromSeconds(3L), now))
        sut.LooperMock.DidNotReceive().Resume()

    // -------------------------------

    [<Test>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``03-0: PostChangeActiveTimeSpan Start -> no current work, shifting backward, preshift state is playing -> resume, no cmd, no intent`` () =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

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
    let ``03-1: PostChangeActiveTimeSpan Start -> some current work, shifting backward, preshift state is playing -> resume, cmd with StoreStoppedWorkEventTask, no intent`` () =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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

        let workSpentTimeList : WorkSpentTime list = []

        let mockWorkRepository = Substitute.For<IWorkRepository>()
        let mockWorkEventRepository = Substitute.For<IWorkEventRepository>()
        let sut = Sut.initWithWorkEventStore (WorkEventStoreStub.initWithWorkSpentTimeList mockWorkRepository mockWorkEventRepository workSpentTimeList)

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (Work.generate () |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel.LooperState.Should().Be(LooperState.Playing)
        %afterPlayerModel.RetrieveWorkSpentTimesState.Should().BeOfCase(AsyncDeferredState.InProgress)

        %cmd.Should().HaveLength(2)

        use semaphore = new SemaphoreSlim(0, 1)
        do cmd[1] (fun msg ->
            %msg.Should().Be(
                PlayerModel.Msg.PostChangeActiveTimeSpan (
                    AsyncOperation.Finish (
                        Ok workSpentTimeList
                        , afterPlayerModel.RetrieveWorkSpentTimesState |> AsyncDeferredState.tryCts |> Option.get
                    )
                )
            )

            %semaphore.Release()
        )
        semaphore.Wait()

        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.Received(1).Resume()

    [<TestCaseSource(nameof initializedAndStoppedStates)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``03-2: PostChangeActiveTimeSpan Start -> no current work, shifting backward, preshift state is not playing -> no resume call, no cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

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

    [<TestCaseSource(nameof initializedAndStoppedStates)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``03-3: PostChangeActiveTimeSpan Start -> some current work, shifting backward, preshift state is playing -> no resume call, cmd, no intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let workSpentTimeList : WorkSpentTime list = []
        let mockWorkRepository = Substitute.For<IWorkRepository>()
        let mockWorkEventRepository = Substitute.For<IWorkEventRepository>()
        let sut = Sut.initWithWorkEventStore (WorkEventStoreStub.initWithWorkSpentTimeList mockWorkRepository mockWorkEventRepository workSpentTimeList)

        // act
        let (afterPlayerModel, cmd, intent) =  beforePlayerModel |> sut.Update (Work.generate () |> Some) (AsyncOperation.startUnit PlayerModel.Msg.PostChangeActiveTimeSpan)

        // assert
        %afterPlayerModel.LooperState.Should().Be(state)
        %afterPlayerModel.RetrieveWorkSpentTimesState.Should().BeOfCase(AsyncDeferredState.InProgress)

        use semaphore = new SemaphoreSlim(0, 1)
        do cmd[0] (fun msg ->
            %msg.Should().Be(
                PlayerModel.Msg.PostChangeActiveTimeSpan (
                    AsyncOperation.Finish (
                        Ok workSpentTimeList
                        , afterPlayerModel.RetrieveWorkSpentTimesState |> AsyncDeferredState.tryCts |> Option.get
                    )
                )
            )

            %semaphore.Release()
        )
        semaphore.Wait()

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
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()
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
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()
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

        %cmd.Should().HaveLength(0)
        // do cmd[0] (fun msg -> %msg.Should().Be(PlayerModel.Msg.OnError "Work spent time list is unexpected empty!"))

        %intent.Should().BeOfCase(PlayerModel.Intent.None)
        sut.LooperMock.DidNotReceive().Resume()

    [<TestCaseSource(nameof notShiftingStates)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``04-3: PostChangeActiveTimeSpan Finish -> ok with single work -> no cmd, RollbackTime intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

        let nowDate = System.TimeProvider.System.GetUtcNow()
        %sut.TimeProvider.GetUtcNow().Returns(nowDate)

        let currentWork = Work.generate ()
        let workSpentTime =
            {
                Work = currentWork
                SpentTime = TimeSpan.FromSeconds(3L)
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
        %intent.Should().Be(PlayerModel.Intent.RollbackTime (workSpentTime, timePoint.Kind, beforePlayerModel.ActiveTimePoint.Value.Id, nowDate))
        sut.LooperMock.DidNotReceive().Resume()

    [<TestCaseSource(nameof notShiftingStates)>]
    [<Category("shifting backward")>]
    [<Category("PlayerModel")>]
    let ``04-4: PostChangeActiveTimeSpan Finish -> ok with multiple works -> no cmd, MultipleRollbackTime intent`` (state: LooperState) =
        let timePoint = TimePoint.generateWith (TimeSpan.FromSeconds(10L))
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
        let sut = Sut.init ()

        let nowDate = System.TimeProvider.System.GetUtcNow()
        %sut.TimeProvider.GetUtcNow().Returns(nowDate)

        let previousWork = Work.generate ()
        let currentWork = Work.generate ()

        let previousWorkSpentTime =
            {
                Work = previousWork
                SpentTime = TimeSpan.FromSeconds(1L)
            }
        let currentWorkSpentTime =
            {
                Work = currentWork
                SpentTime = TimeSpan.FromSeconds(2L)
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
        %intent.Should().Be(PlayerModel.Intent.MultipleRollbackTime ([ previousWorkSpentTime; currentWorkSpentTime ], timePoint.Kind, beforePlayerModel.ActiveTimePoint.Value.Id, nowDate))
        sut.LooperMock.DidNotReceive().Resume()

