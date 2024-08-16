﻿namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open System.Collections.Generic

open p1eXu5.FSharp.Testing.ShouldExtensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp

type LooperStub (activeTimePoint: ActiveTimePoint option) =
    member val ResumeCalledTimes : int = 0 with get, set
    member this.IsResumeCalled = this.ResumeCalledTimes > 0
    interface ILooper with
        member _.Dispose () = ()
         member _.Start() = ()
         member _.Stop() = ()
         member _.Next() = ()
         member _.Shift _ = ()
         member _.ShiftAck _ = ()
         member this.Resume() = this.ResumeCalledTimes <- this.ResumeCalledTimes + 1
         member _.AddSubscriber _ = ()
         member _.PreloadTimePoint() = ()
         member _.GetActiveTimePoint() = activeTimePoint


type UserSettingsStub () =
    let dict = Dictionary<string, obj>()

    do
        dict.Add("BotToken", faker.Random.Hash() |> box)
        dict.Add("MyChatId", faker.Numeric(9) |> box)
        dict.Add("DisableSkipBreak", false)
        dict.Add("TimePointPrototypesSettings", Option<string>.None)
        dict.Add("TimePointSettings", Option<string>.None)
        dict.Add("CurrentWork", Option<Work>.None)
        dict.Add("LastStatisticPeriod", Option<Work>.None)
        // TODO: dict.Add("RollbackWorkStrategy", RollbackWorkStrategy.Default)
        dict.Add("LastDayCount", 0)

    interface IUserSettings with
        member _.BotToken with get () = dict["BotToken"] :?> string option and set v = dict["BotToken"] <- v
        member _.MyChatId with get () = dict["MyChatId"] :?> string option and set v = dict["MyChatId"] <- v
        member _.Patterns with get () = dict["Patterns"] :?> Pattern list and set v = dict["Patterns"] <- v
        member _.TimePointPrototypesSettings with get () = dict["TimePointPrototypesSettings"] :?> string option and set v = dict["TimePointPrototypesSettings"] <- v
        member _.TimePointSettings with get () = dict["TimePointSettings"] :?> string option and set v = dict["TimePointSettings"] <- v
        member _.DisableSkipBreak with get () = dict["DisableSkipBreak"] :?> bool and set v = dict["DisableSkipBreak"] <- v
        member _.CurrentWork with get () = dict["CurrentWork"] :?> Work option and set v = dict["CurrentWork"] <- v
        member _.LastStatisticPeriod with get () = dict["LastStatisticPeriod"] :?> DateOnlyPeriod option and set v = dict["LastStatisticPeriod"] <- v
        // TODO: member _.RollbackWorkStrategy with get () = dict["RollbackWorkStrategy"] :?> RollbackWorkStrategy and set v = dict["RollbackWorkStrategy"] <- v
        member _.LastDayCount with get () = dict["LastDayCount"] :?> int and set v = dict["LastDayCount"] <- v


[<RequireQualifiedAccess>]
module ErrorMessageQueueStub =

    type T =
        {
            Name: string
            ErrorQueue: Queue<string>
        }
        interface IErrorMessageQueue with
            member this.EnqueueError errorMsg =
                this.ErrorQueue.Enqueue(errorMsg)
                writeLine (sprintf "error: IErrorMessageQueue.%s:%s    %s" this.Name Environment.NewLine errorMsg)

    let create (key: string) =
        {
            Name = key
            ErrorQueue = Queue<string>()
        }
        :> IErrorMessageQueue

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


module WorkEventStoreStub =
    let initWithWorkSpentTimeList (workSpentTimeList: WorkSpentTime list) =
        {
            StoreStartedWorkEventTask = fun _ -> task { return () }
            StoreStoppedWorkEventTask = fun _ -> task { return () }
            StoreWorkReducedEventTask = fun _ -> task { return () }
            StoreBreakReducedEventTask = fun _ -> task { return () }
            StoreBreakIncreasedEventTask = fun _ -> task { return () }
            StoreWorkIncreasedEventTask = fun _ -> task { return () }
            WorkSpentTimeListTask =
                fun _ ->
                    task {
                        return Ok workSpentTimeList
                    }
        }
