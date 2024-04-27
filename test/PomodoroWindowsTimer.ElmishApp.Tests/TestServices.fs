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
module TestUserSettings =

    let private dict = Dictionary<string, obj>()

    let private load key =
        match dict.TryGetValue(key) with
        | true, v -> v
        | false, _ -> null

    let private save key (v: obj) =
        dict.Add(key, v)

    let create () =
        { new IUserSettings with
            member _.BotToken with get () = dict["BotToken"] :?> string option and set v = dict["BotToken"] <- v
            member _.MyChatId with get () = dict["MyChatId"] :?> string option and set v = dict["MyChatId"] <- v
            member _.Patterns with get () = dict["Patterns"] :?> Pattern list and set v = dict["Patterns"] <- v
            member _.TimePointPrototypesSettings with get () = dict["TimePointPrototypesSettings"] :?> string option and set v = dict["TimePointPrototypesSettings"] <- v
            member _.TimePointSettings with get () = dict["TimePointSettings"] :?> string option and set v = dict["TimePointSettings"] <- v
            member _.DisableSkipBreak with get () = dict["DisableSkipBreak"] :?> bool and set v = dict["DisableSkipBreak"] <- v
        }

    do
        dict.Add("BotToken", faker.Random.Hash() |> box)
        dict.Add("MyChatId", faker.Numeric(9) |> box)


[<RequireQualifiedAccess>]
module TestErrorMessageQueue =

    let create () : IErrorMessageQueue =
        { new IErrorMessageQueue with
            member _.EnqueueError _ = ()
        }


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

    let sendToBot (message: Message) =
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


[<RequireQualifiedAccess>]
module TestTimePointStore =
    let create (timePoints: TimePoint list) : TimePointStore =
        {
            Read = fun () -> timePoints
            Write = fun _ -> ()
        }


// ==================
// settings
// ==================

module TestSettings =
    let getStrOpt key (dict: IDictionary<string, obj>) =
        match dict.TryGetValue(key) with
        | true, v -> v :?> string |> Some
        | _, _ -> None

    let addStrOpt key v (dict: IDictionary<string, obj>) =
        match v with
        | Some s -> dict[key] <- box s
        | None -> dict.Remove(key) |> ignore


[<RequireQualifiedAccess>]
module TestTimePointPrototypeSettings =
    let create (dict: IDictionary<string, obj>) =
        { new ITimePointPrototypesSettings with
            member _.TimePointPrototypesSettings
                with get() =
                    dict |> TestSettings.getStrOpt "TestTimePointPrototypeSettings"
                and set v =
                    dict |> TestSettings.addStrOpt "TestTimePointPrototypeSettings" v
        }

[<RequireQualifiedAccess>]
module TestTimePointSettings =
    let create dict =
        { new ITimePointSettings with
            member _.TimePointSettings
                with get() =
                    dict |> TestSettings.getStrOpt "TestTimePointSettings"
                and set v =
                    dict |> TestSettings.addStrOpt "TestTimePointSettings" v
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
                    dict["TestPatternSettings"] <- patterns
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
                    dict["TestDisableSkipBreakSettings"] <- v
        }



type TestDispatch () =
    let timeout = ((int Program.tickMilliseconds) * 2 + ((int Program.tickMilliseconds) / 2))

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
