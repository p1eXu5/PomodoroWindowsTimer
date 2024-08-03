namespace PomodoroWindowsTimer.ElmishApp.Tests

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

