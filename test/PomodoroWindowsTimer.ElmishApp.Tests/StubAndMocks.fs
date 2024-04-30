﻿namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open System.Collections.Generic

open p1eXu5.FSharp.Testing.ShouldExtensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Abstractions


type UserSettingsStub () =
    let dict = Dictionary<string, obj>()

    do
        dict.Add("BotToken", faker.Random.Hash() |> box)
        dict.Add("MyChatId", faker.Numeric(9) |> box)
        dict.Add("DisableSkipBreak", false)
        dict.Add("TimePointPrototypesSettings", Option<string>.None)
        dict.Add("TimePointSettings", Option<string>.None)
        dict.Add("CurrentWork", Option<Work>.None)

    interface IUserSettings with
        member _.BotToken with get () = dict["BotToken"] :?> string option and set v = dict["BotToken"] <- v
        member _.MyChatId with get () = dict["MyChatId"] :?> string option and set v = dict["MyChatId"] <- v
        member _.Patterns with get () = dict["Patterns"] :?> Pattern list and set v = dict["Patterns"] <- v
        member _.TimePointPrototypesSettings with get () = dict["TimePointPrototypesSettings"] :?> string option and set v = dict["TimePointPrototypesSettings"] <- v
        member _.TimePointSettings with get () = dict["TimePointSettings"] :?> string option and set v = dict["TimePointSettings"] <- v
        member _.DisableSkipBreak with get () = dict["DisableSkipBreak"] :?> bool and set v = dict["DisableSkipBreak"] <- v
        member _.CurrentWork with get () = dict["CurrentWork"] :?> Work option and set v = dict["CurrentWork"] <- v


[<RequireQualifiedAccess>]
module ErrorMessageQueueStub =

    let create (key: string) =
        { new IErrorMessageQueue with
            member _.EnqueueError errorMsg =
                writeLine (sprintf "error: IErrorMessageQueue.%s:%s    %s" key Environment.NewLine errorMsg)
        }

[<RequireQualifiedAccess>]
module ThemeSwitcherStub =

    let create () =
        { new IThemeSwitcher with
            member _.SwitchTheme _ = ()
        }


type TelegramBotStub () =
    member val MessageStack : Stack<string> = Stack<string>() with get

    interface ITelegramBot with
        member this.SendMessage message =
            task {
                this.MessageStack.Push(message)
            }

