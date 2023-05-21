module PomodoroWindowsTimer.ElmishApp.Tests.TestServices

open System
open System.Collections.Generic
open Bogus
open FakerExtensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.ElmishApp.MainModel
open PomodoroWindowsTimer.Types

let faker = Faker("en")

[<RequireQualifiedAccess>]
module TestSettingsManager =

    let private dict = Dictionary<string, obj>()

    let private load key =
        match dict.TryGetValue(key) with
        | true, v -> v
        | false, _ -> null

    let private save key (v: obj) =
        dict.Add(key, v)

    let create () =
        { new ISettingsManager with
            member _.Load (key: string) : obj = load key
            member _.Save (key: string) (value: obj) : unit = save key value
        }

    do
        dict.Add("BotToken", faker.Random.Hash() |> box)
        dict.Add("MyChatId", faker.Numeric(9) |> box)


[<RequireQualifiedAccess>]
module TestErrorMessageQueue =

    let create () =
        { new IErrorMessageQueue with
            member _.EnqueuError _ = () }


[<RequireQualifiedAccess>]
module TestBotConfiguration =

    let create () =
        { new IBotSettings with
            member _.BotToken
                with get() = faker.Random.Hash() |> Some
                and set t = ()
            member _. MyChatId
                with get() = faker.Numeric(9) |> Some
                and set t = ()
        }


[<RequireQualifiedAccess>]
module TestBotSender =

    let messages = List<string>()

    let sendToBot _ message =
        task {
            messages.Add(message)
            return ()
        }

    let create () : BotSender =
        messages.Clear()
        sendToBot


[<RequireQualifiedAccess>]
module TestThemeSwitcher =

    let create () =
        { new IThemeSwitcher with
            member _.SwitchTheme _ = ()
        }


[<RequireQualifiedAccess>]
module TestTimePointPrototypeStore =
    let create () : TimePointPrototypeStore =
        {
            Read = fun () -> TimePointPrototype.defaults
            Write = fun _ -> ()
        }


module TestSettings =
    let get key (dict: IDictionary<string, obj>) =
        match dict.TryGetValue(key) with
        | true, v -> v :?> string |> Some
        | _, _ -> None


[<RequireQualifiedAccess>]
module TestTimePointPrototypeSettings =
    let create (dict: IDictionary<string, obj>) =
        { new ITimePointPrototypesSettings with
            member _.TimePointPrototypesSettings
                with get() =
                    dict |> TestSettings.get "TestTimePointPrototypeSettings"
                and set v =
                    dict.Add("TestTimePointPrototypeSettings", box v)
        }


[<RequireQualifiedAccess>]
module TestTimePointStore =
    let create (timePoints: TimePoint list) : TimePointStore =
        {
            Read = fun () -> timePoints
            Write = fun _ -> ()
        }

[<RequireQualifiedAccess>]
module TestTimePointSettings =
    let create dict =
        { new ITimePointSettings with
            member _.TimePointSettings
                with get() =
                    dict |> TestSettings.get "TestTimePointSettings"
                and set v =
                    dict.Add("TestTimePointSettings", box v)
        }


[<RequireQualifiedAccess>]
module TestPatternSettings =
    let create (dict: IDictionary<string, obj>) =
        { new IPatternSettings with
            member _.Patterns
                with get() =
                    match dict.TryGetValue("TestPatternSettings") with
                        | true, v -> v :?> string list
                        | _, _ -> []
                and set(patterns) =
                    dict.Add("TestPatternSettings", box patterns)
        }

module TestDisableSkipBreakSettings =
    let create (dict: IDictionary<string, obj>) =
        { new IDisableSkipBreakSettings with
            member _.DisableSkipBreak
                with get() =
                    match dict.TryGetValue("TestDisableSkipBreakSettings") with
                    | true, v -> v :?> bool
                    | _, _ -> false
                and set v =
                    dict.Add("TestTimePointSettings", box v)
        }


type TestDispatch () =
    let timeout = Program.tickMilliseconds * 2 + (Program.tickMilliseconds / 2)

    let msgDispatchRequestedEvent = new Event<_>()

    [<CLIEvent>]
    member this.MsgDispatchRequested : IEvent<Msg> = msgDispatchRequestedEvent.Publish

    member this.Trigger(message: MainModel.Msg) =
       msgDispatchRequestedEvent.Trigger(message)

    member this.TriggerWithTimeout(message: MainModel.Msg) =
        async {
           msgDispatchRequestedEvent.Trigger(message)
           do! Async.Sleep timeout
        }
        |> Async.RunSynchronously

     member this.WaitTimeout() =
        async {
           do! Async.Sleep timeout
        }
        |> Async.RunSynchronously
