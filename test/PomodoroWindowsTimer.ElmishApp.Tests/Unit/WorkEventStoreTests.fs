namespace PomodoroWindowsTimer.ElmishApp.Tests.Unit

module WorkEventStoreTests =

    open System
    open System.Threading
    open Microsoft.Extensions.Logging

    open NUnit.Framework
    open Faqt
    open Faqt.Operators
    open p1eXu5.FSharp.Testing.ShouldExtensions
    open NSubstitute

    open PomodoroWindowsTimer
    open PomodoroWindowsTimer.Abstractions
    open PomodoroWindowsTimer.Types
    open PomodoroWindowsTimer.ElmishApp
    open PomodoroWindowsTimer.ElmishApp.Abstractions
    open PomodoroWindowsTimer.ElmishApp.Models
    
    open PomodoroWindowsTimer.Testing.Fakers
    open PomodoroWindowsTimer.ElmishApp.Tests

    let private workId = WorkId.generate ()
    let private anyWorkEvent = Arg.Any<WorkEvent>()

    let private activeTimePointId = TimePointId.generate ()
    let private date = DateOnly.FromDateTime(System.TimeProvider.System.GetUtcNow().Date)
    let private ct = CancellationToken.None
    let private min = 60.0<sec>

    type CaseData =
        {
            WorkEventLists: WorkEventList list
            Kind: Kind
            BeforeDate: DateTimeOffset
            Diff: float<sec>
        }

    let private testCases : System.Collections.IEnumerable =
        seq {
            let work1 = Work.generate ()
            TestCaseData(
                {
                    WorkEventLists =
                        [
                            {
                                Work = work1
                                Events =
                                    [
                                        WorkEvent.generateWorkStartedWith date "12:00"
                                        WorkEvent.generateStoppedWith date "12:10"
                                    ]
                                    |> List.rev
                            }
                        ]
                    Kind = Kind.Work
                    BeforeDate =  WorkEvent.generateCreatedAt date "12:10"
                    Diff = 5.0 * min
                }
            ).Returns(
                [
                    {
                        Work = work1
                        SpentTime = TimeSpan.FromMinutes(5)
                    }
                ]
            ).SetName("01: single Work, single start-stop")

            TestCaseData(
                {
                    WorkEventLists =
                        [
                            {
                                Work = work1
                                Events =
                                    [
                                        WorkEvent.generateWorkStartedWith date "12:07"
                                        WorkEvent.generateStoppedWith date "12:08"
                                        WorkEvent.generateWorkStartedWith date "12:09"
                                        WorkEvent.generateStoppedWith date "12:10"
                                    ]
                                    |> List.rev
                            }
                        ]
                    Kind = Kind.Work
                    BeforeDate =  WorkEvent.generateCreatedAt date "12:10"
                    Diff = 5.0 * min
                }
            ).Returns(
                [
                    {
                        Work = work1
                        SpentTime = TimeSpan.FromMinutes(2)
                    }
                ]
            ).SetName("02: single Work, severtal start-stop")

            let work2 = Work.generate ()
            TestCaseData(
                {
                    WorkEventLists =
                        [
                            {
                                Work = work1
                                Events =
                                    [
                                        WorkEvent.generateBreakStartedWith date "12:07"
                                        WorkEvent.generateStoppedWith date "12:08"
                                        WorkEvent.generateBreakStartedWith date "12:09"
                                        WorkEvent.generateStoppedWith date "12:10"
                                    ]
                                    |> List.rev
                            }
                            {
                                Work = work2
                                Events =
                                    [
                                        WorkEvent.generateBreakStartedWith date "11:50"
                                        WorkEvent.generateStoppedWith date "12:06"
                                    ]
                                    |> List.rev
                            }
                        ]
                    Kind = Kind.Break
                    BeforeDate =  WorkEvent.generateCreatedAt date "12:10"
                    Diff = 5.0 * min
                }
            ).Returns(
                [
                    {
                        Work = work1
                        SpentTime = TimeSpan.FromMinutes(2)
                    }
                    {
                        Work = work2
                        SpentTime = TimeSpan.FromMinutes(1)
                    }
                ]
            ).SetName("03: two Works, severtal start-stop")
        }


    [<TestCaseSource(nameof testCases)>]
    let ``WorkSpentTimeListTask -> repo returns single work event -> returns list with single item with TimeSpent equal to diff`` (caseData: CaseData) =
        task {
            let mockActiveTimePointRepo = Substitute.For<IActiveTimePointRepository>()
            let mockWorkEventRepo = Substitute.For<IWorkEventRepository>()
            do 
                mockWorkEventRepo.FindByActiveTimePointIdByDateAsync activeTimePointId caseData.Kind caseData.BeforeDate ct
                |> _.Returns(Ok caseData.WorkEventLists)
                |> ignore

            let workEventStore = WorkEventStore.init mockWorkEventRepo mockActiveTimePointRepo

            let! res = workEventStore.WorkSpentTimeListTask (activeTimePointId, caseData.Kind, caseData.BeforeDate, caseData.Diff, ct)
            match res with
            | Error err -> return assertionExn err
            | Ok ok -> return ok
        }

    [<Test>]
    let ``04: StoreStartedWorkEventTask -> by default -> calls IActiveTimePointRepository.InsertIfNotExistsAsync`` () =
        task {
            let activeTimePoint = ActiveTimePoint.generate ()
            let date = System.TimeProvider.System.GetUtcNow()

            let mockActiveTimePointRepo = Substitute.For<IActiveTimePointRepository>()
            do
                mockActiveTimePointRepo.InsertIfNotExistsAsync activeTimePoint ct
                |> _.Returns(Ok ())
                |> ignore

            let mockWorkEventRepo = Substitute.For<IWorkEventRepository>()
            do
                mockWorkEventRepo.InsertAsync workId anyWorkEvent ct
                |> _.Returns(Ok 1UL)
                |> ignore

            let workEventStore = WorkEventStore.init mockWorkEventRepo mockActiveTimePointRepo

            // act
            do! workEventStore.StoreStartedWorkEventTask (workId, date, activeTimePoint)

            let! _ = mockActiveTimePointRepo.Received(1).InsertIfNotExistsAsync activeTimePoint ct
            let! _ = mockWorkEventRepo.ReceivedWithAnyArgs(1).InsertAsync workId anyWorkEvent ct
            return ()
        }

    [<Test>]
    let ``05: StoreStartedWorkEventTask -> active time point inssertion is failed -> throws`` () =
        task {
            let activeTimePoint = ActiveTimePoint.generate ()
            let date = System.TimeProvider.System.GetUtcNow()

            let mockActiveTimePointRepo = Substitute.For<IActiveTimePointRepository>()
            do
                mockActiveTimePointRepo.InsertIfNotExistsAsync activeTimePoint ct
                |> _.Returns(Error "test error")
                |> ignore

            let mockWorkEventRepo = Substitute.For<IWorkEventRepository>()

            let workEventStore = WorkEventStore.init mockWorkEventRepo mockActiveTimePointRepo

            // act
            let _ = Assert.ThrowsAsync<InvalidOperationException>(fun () -> workEventStore.StoreStartedWorkEventTask (workId, date, activeTimePoint))

            let! _ = mockActiveTimePointRepo.Received(1).InsertIfNotExistsAsync activeTimePoint ct
            let! _ = mockWorkEventRepo.DidNotReceiveWithAnyArgs().InsertAsync workId anyWorkEvent ct
            return ()
        }

