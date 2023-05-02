namespace PomodoroWindowsTimer.ElmishApp.Tests.BDD

open System
open System.Threading.Tasks
open NUnit.Framework
open FsUnit.TopLevelOperators
open ShouldExtensions
open Bogus

open Elmish
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests.TestServices


module StopResumeScenarios =
    let faker = Faker("en")

    let ``0.5 sec`` = TimeSpan.FromMilliseconds(500)
    let ``3 sec`` = TimeSpan.FromSeconds(3)

    let timePointFaker namePrefix =
        {
            Id = faker.Random.Uuid()
            Name = (namePrefix, faker.Commerce.ProductName()) ||> sprintf "%s. %s"
            TimeSpan = faker.Date.Timespan()
            Kind = faker.Random.ArrayElement([| Kind.Work; Kind.Break |])
        }

    let workTP timeSpan =
        {
            timePointFaker "Work"
                with
                    TimeSpan = timeSpan
                    Kind = Kind.Work
        }

    let breakTP timeSpan =
        {
            timePointFaker "Break"
                with
                    TimeSpan = timeSpan
                    Kind = Kind.Break
        }


    let mutable model = Unchecked.defaultof<_>
    let mutable looper = Unchecked.defaultof<_>

    let testDispatch = TestDispatch()
    let mutable testDispatchSubscriber = Unchecked.defaultof<_>

    module Given =
        let ``Elmish Program with`` (timePoints: TimePoint list) =
            let timePointQueue = new TimePointQueue(timePoints)
            looper <- new Looper((timePointQueue :> ITimePointQueue), Program.tickMilliseconds)

            let subscribe _ =
                let looperEffect dispatch =
                    let onLooperEvt =
                        fun evt ->
                            async {
                                do dispatch (MainModel.Msg.LooperMsg evt)
                            }

                    looper.AddSubscriber(onLooperEvt)

                let sendExtMsgEffect dispatch =
                    testDispatchSubscriber <- testDispatch.MsgDispatchRequested.Subscribe(fun msg -> dispatch msg)

                [ looperEffect; sendExtMsgEffect ]

            looper.Start()

            let testBotConfiguration = TestBotConfiguration.create ()
            let sendToBot = TestBotSender.create ()

            Program.mkProgram 
                (fun () ->
                    MainModel.init
                        (TestSettingsManager.create ())
                        testBotConfiguration
                        (TestErrorMessageQueue.create ())
                        timePoints
                ) 
                (MainModel.Program.update
                    testBotConfiguration
                    sendToBot
                    looper
                    Windows.simWindowsMinimizer
                    (TestThemeSwitcher.create ())
                )
                (fun m _ -> model <- m)
            |> Program.withSubscription subscribe
            |> Program.withTrace (fun msg _ -> TestContext.WriteLine(sprintf "%A" msg))
            |> Program.run


    [<TearDown>]
    let disposeLoop () =
        (looper :> IDisposable).Dispose()
        testDispatchSubscriber.Dispose()


    module When =
        let ``Spent 2.5 ticks time`` () =
            testDispatch.WaitTimeout()

        let ``Looper starts playing`` () =
            testDispatch.TriggerWithTimeout(Msg.Play)

        let ``Looper is stopping`` () =
            testDispatch.TriggerWithTimeout(Msg.Stop)

        let ``Looper is resuming`` () =
            testDispatch.TriggerWithTimeout(Msg.Resume)

        let ``Looper is playing next`` () =
            testDispatch.TriggerWithTimeout(Msg.Next)

        let ``Looper is replaying`` () =
            testDispatch.TriggerWithTimeout(Msg.Replay)

        let ``TimePoint is changed on`` timePoint =
            let tcs = TaskCompletionSource()
            looper.AddSubscriber(fun ev ->
                async {
                    match ev with
                    | LooperEvent.TimePointTimeReduced tp
                    | LooperEvent.TimePointStarted (tp, _) when tp.Id = timePoint.Id -> tcs.SetResult()
                    | _ -> ()
                }
            )
            task {
                let! _ = tcs.Task
                return ()
            }
            |> Async.AwaitTask
            |> Async.RunSynchronously
            

    module Then =

        let rec ``Active Point is set on`` timePoint =
            model.ActiveTimePoint
            |> Option.map (fun atp -> atp.Id = timePoint.Id)
            |> Option.defaultValue false
            |> shouldL be True (sprintf "%s:\n%A" (nameof ``Active Point is set on``) timePoint)

        let rec ``Active Point remaining time is equal to or less then`` timePoint =
            model.ActiveTimePoint
            |> Option.map (fun atp -> atp.TimeSpan <= timePoint.TimeSpan.Add(TimeSpan.FromMilliseconds(Program.tickMilliseconds)))
            |> Option.defaultValue false
            |> shouldL be True (nameof ``Active Point remaining time is equal to or less then``)

        let rec ``LooperState is Playing`` () =
            model.LooperState
            |> shouldL be (ofCase <@ LooperState.Playing @>) (nameof ``LooperState is Playing``)

        let rec ``LooperState is Stopped`` () =
            model.LooperState
            |> shouldL be (ofCase <@ LooperState.Stopped @>) (nameof ``LooperState is Stopped``)

        let rec ``LooperState is Initialized`` () =
            model.LooperState
            |> shouldL be (ofCase <@ LooperState.Initialized @>) (nameof ``LooperState is Initialized``)

        let rec ``Windows should be minimized`` () =
            model.IsMinimized
            |> shouldL be True (nameof ``Windows should be minimized``)

        let rec ``Windows should not be minimized`` () =
            model.IsMinimized
            |> shouldL be False (nameof ``Windows should not be minimized``)

        let rec ``Telegrtam bot should not be notified`` () =
            TestBotSender.messages |> shouldL be Empty (nameof ``Telegrtam bot should not be notified``)

        let rec ``Telegrtam bot should be notified`` () =
            TestBotSender.messages |> shouldL not' (be Empty) (nameof ``Telegrtam bot should be notified``)


    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC01 - Initialization Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Spent 2.5 ticks time`` () // wait when looper preload time point and send event

        Then.``Active Point is set on`` timePoints.Head
        Then.``LooperState is Initialized`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC02 - Start Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC03 - Start-Stop Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is stopping`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Stopped`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC04 - Start-Stop-Resume Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is stopping`` ()
        When.``Looper is resuming`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC05 - Start Work Next Break TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC06 - Start-Stop-Next Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is stopping`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC07 - Work to Breate TimePoint transition Scenario`` () =
        let timePoints = [ workTP ``0.5 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``TimePoint is changed on`` timePoints[1]

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC08 - Start-Replay Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is replaying`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    // =====================================
    //               Break
    // =====================================

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC11 - Initialization Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Spent 2.5 ticks time`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``LooperState is Initialized`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC12 - Start Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC13 - Start-Stop Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is stopping`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Stopped`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC14 - Start-Stop-Resume Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is stopping`` ()
        When.``Looper is resuming`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC15 - Start Break Next Work TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC16 - Start-Stop-Break Work TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``Looper is stopping`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC17 - Breate to Work TimePoint transition Scenario`` () =
        let timePoints = [ breakTP ``0.5 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper starts playing`` ()
        When.``TimePoint is changed on`` timePoints[1]

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()
        Then.``Telegrtam bot should be notified`` ()

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC18 - Start-Replay Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Spent 2.5 ticks time`` ()

        When.``Looper starts playing`` ()
        When.``Looper is replaying`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()
        Then.``Telegrtam bot should not be notified`` ()