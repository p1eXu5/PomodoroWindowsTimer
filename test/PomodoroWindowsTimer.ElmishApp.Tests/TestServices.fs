﻿module PomodoroWindowsTimer.ElmishApp.Tests.TestServices

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
                with get() = faker.Random.Hash()
                and set t = ()
            member _. MyChatId
                with get() = faker.Numeric(9)
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


[<RequireQualifiedAccess>]
module TestTimePointStore =
    let create (timePoints: TimePoint list) : TimePointStore =
        {
            Read = fun () -> timePoints
            Write = fun _ -> ()
        }


[<RequireQualifiedAccess>]
module TestPatternSettings =
    let create () =
        { new IPatternSettings with
            member _.Read() = [TimePointsSettingsModel.DEFAULT_PATTERN]
            member _.Write(_) = ()
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
