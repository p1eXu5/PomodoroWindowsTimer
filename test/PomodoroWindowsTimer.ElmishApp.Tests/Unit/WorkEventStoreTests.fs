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

    let private activeTimePointId = TimePointId.generate ()
    let private date = DateOnly.FromDateTime(System.TimeProvider.System.GetUtcNow().Date)
    let private ct = CancellationToken.None
    let private min = 60.0<sec>

    type CaseData =
        {
            WorkEventLists: WorkEventList list
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
            let workEventRepoMock = Substitute.For<IWorkEventRepository>()
            do 
                workEventRepoMock.FindByActiveTimePointIdByDateAsync activeTimePointId caseData.BeforeDate ct
                |> _.Returns(Ok caseData.WorkEventLists)
                |> ignore

            let workEventStore = WorkEventStore.init workEventRepoMock

            let! res = workEventStore.WorkSpentTimeListTask (activeTimePointId, caseData.BeforeDate, caseData.Diff, ct)
            match res with
            | Error err -> return assertionExn err
            | Ok ok -> return ok
        }

