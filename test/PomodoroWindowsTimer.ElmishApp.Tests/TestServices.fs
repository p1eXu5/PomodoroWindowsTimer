module PomodoroWindowsTimer.ElmishApp.Tests.TestServices

open System
open System.Collections.Generic
open Bogus
open FakerExtensions
open PomodoroWindowsTimer.ElmishApp.Abstractions

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
        { new IBotConfiguration with
            member _.BotToken
                with get() = faker.Random.Hash()
                and set t = ()
            member _. MyChatId
                with get() = faker.Numeric(9)
                and set t = ()
        }


[<RequireQualifiedAccess>]
module TestBotSender =

    let sendToBot _ _ =
        task {
            return ()
        }