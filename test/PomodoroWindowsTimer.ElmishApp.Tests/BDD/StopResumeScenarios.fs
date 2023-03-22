namespace PomodoroWindowsTimer.ElmishApp.Tests.BDD

open NUnit.Framework
open FsUnit.TopLevelOperators
open Elmish
open Bogus
open PomodoroWindowsTimer.Types
open System
open PomodoroWindowsTimer.ElmishApp.Tests.TestServices
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.Abstrractions
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open ShouldExtensions

type TestDispatch () =
    let timeout = Program.tickMilliseconds * 2 + (Program.tickMilliseconds / 2)

    let msgDispatchRequestedEvent = new Event<_>()

    [<CLIEvent>]
    member this.MsgDispatchRequested = msgDispatchRequestedEvent.Publish

    member this.Trigger(message: MainModel.Msg) =
       msgDispatchRequestedEvent.Trigger(message)

    member this.TriggerWithTimeout(message: MainModel.Msg) =
        async {
           msgDispatchRequestedEvent.Trigger(message)
           do! Async.Sleep timeout
        }
        |> Async.RunSynchronously

module StopResumeScenarios =
    let faker = Faker("en")

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

    [<TearDown>]
    let disposeLoop () =
        (looper :> IDisposable).Dispose()

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
                    testDispatch.MsgDispatchRequested.Add(fun msg -> dispatch msg)

                [ looperEffect; sendExtMsgEffect ]

            looper.Start()

            let testBotConfiguration = TestBotConfiguration.create ()

            Program.mkProgram 
                (fun () ->
                    MainModel.init
                        (TestSettingsManager.create ())
                        testBotConfiguration
                        (TestErrorMessageQueue.create ())
                        timePoints
                ) 
                (MainModel.Program.update testBotConfiguration TestBotSender.sendToBot looper Infrastructure.simWindowsMinimizer)
                (fun m _ -> model <- m)
            |> Program.withSubscription subscribe
            |> Program.withTrace (fun msg _ -> TestContext.WriteLine(sprintf "%A" msg))
            |> Program.run


    module When =
        let ``Looper is running`` () =
            testDispatch.TriggerWithTimeout(Msg.Play)

        let ``Looper is stopping`` () =
            testDispatch.TriggerWithTimeout(Msg.Stop)

        let ``Looper is resuming`` () =
            testDispatch.TriggerWithTimeout(Msg.Resume)

        let ``Looper is playing next`` () =
            testDispatch.TriggerWithTimeout(Msg.Next)


    module Then =

        let rec ``Active Point is set on`` timePoint =
            model.ActiveTimePoint
            |> Option.map (fun atp -> atp.Id = timePoint.Id)
            |> Option.defaultValue false
            |> shouldL be True (sprintf "%s:\n%A" (nameof ``Active Point is set on``) timePoint)

        let rec ``Active Point remaining time is equal to or less then`` timePoint =
            model.ActiveTimePoint
            |> Option.map (fun atp -> atp.TimeSpan <= timePoint.TimeSpan)
            |> Option.defaultValue false
            |> shouldL be True (nameof ``Active Point remaining time is equal to or less then``)

        let rec ``LooperState is Playing`` () =
            model.LooperState
            |> shouldL be (ofCase <@ LooperState.Playing @>) (nameof ``LooperState is Playing``)

        let rec ``LooperState is Stopped`` () =
            model.LooperState
            |> shouldL be (ofCase <@ LooperState.Stopped @>) (nameof ``LooperState is Stopped``)

        let rec ``Windows should be minimized`` () =
            model.IsMinimized
            |> shouldL be True (nameof ``Windows should be minimized``)

        let rec ``Windows should not be minimized`` () =
            model.IsMinimized
            |> shouldL be False (nameof ``Windows should not be minimized``)


    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC01 - Initialization Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        Then.``Active Point is set on`` timePoints.Head
        Then.``LooperState is Stopped`` ()
        Then.``Windows should not be minimized`` ()


    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC02 - Start Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()


    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC03 - Start-Stop Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()
        When.``Looper is stopping`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Stopped`` ()
        Then.``Windows should not be minimized`` ()


    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC04 - Start-Stop-Resume Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()
        When.``Looper is stopping`` ()
        When.``Looper is resuming`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()


    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC05 - Start-Next Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()


    [<Test>]
    [<Category("Work TimePoint Scenarios")>]
    let ``UC06 - Start-Stop-Next Work TimePoint Scenario`` () =
        let timePoints = [ workTP ``3 sec``; breakTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()
        When.``Looper is stopping`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()


    // =====================================
    //               Break
    // =====================================

    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC11 - Initialization Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        Then.``Active Point is set on`` timePoints.Head
        Then.``LooperState is Stopped`` ()
        Then.``Windows should not be minimized`` ()


    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC12 - Start Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()


    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC13 - Start-Stop Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()
        When.``Looper is stopping`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Stopped`` ()
        Then.``Windows should not be minimized`` ()


    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC14 - Start-Stop-Resume Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()
        When.``Looper is stopping`` ()
        When.``Looper is resuming`` ()

        Then.``Active Point is set on`` timePoints.Head
        Then.``Active Point remaining time is equal to or less then`` timePoints.Head
        Then.``LooperState is Playing`` ()
        Then.``Windows should be minimized`` ()


    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC15 - Start-Next Break TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()


    [<Test>]
    [<Category("Break TimePoint Scenarios")>]
    let ``UC16 - Start-Stop-Break Work TimePoint Scenario`` () =
        let timePoints = [ breakTP ``3 sec``; workTP ``3 sec`` ]
        Given.``Elmish Program with`` timePoints

        When.``Looper is running`` ()
        When.``Looper is stopping`` ()
        When.``Looper is playing next`` ()

        Then.``Active Point is set on`` timePoints[1]
        Then.``Active Point remaining time is equal to or less then`` timePoints[1]
        Then.``LooperState is Playing`` ()
        Then.``Windows should not be minimized`` ()

