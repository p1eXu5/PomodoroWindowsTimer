module PomodoroWindowsTimer.ElmishApp.Tests.LoggerExtensions

open Microsoft.Extensions.Logging


let private msgDispatchingDebugMessage = LoggerMessage.Define<string>(
    LogLevel.Debug,
    new EventId(0b1_0000_0001, "Elmish Dispatching Message"),
    "Dispatched message: {Msg}."
)

let private msgDispatchedDebugMessage = LoggerMessage.Define<string>(
    LogLevel.Debug,
    new EventId(0b1_0000_0010, "Elmish Dispatched Message"),
    "Dispatched message: {Msg}."
)


open System.Runtime.CompilerServices
open PomodoroWindowsTimer

[<Extension>]
type LoggerExtensions () =
    [<Extension>]
    static member LogDispatchingMsg<'Msg when 'Msg: not null>(logger: ILogger, msg: 'Msg) =
        let msgType = msg.GetType()
        let msgName = (msgType.FullName.Replace("PomodoroWindowsTimer.ElmishApp.Models.", "").Replace("+", "."))
        msgDispatchingDebugMessage.Invoke(logger, $"{msgName} ({msg})", null)

    [<Extension>]
    static member LogDispatchedMsg<'Msg when 'Msg: not null>(logger: ILogger, msg: 'Msg) =
        let msgType = msg.GetType()
        let msgName = (msgType.FullName.Replace("PomodoroWindowsTimer.ElmishApp.Models.", "").Replace("+", "."))
        msgDispatchedDebugMessage.Invoke(logger, $"{msgName} ({msg})", null)

