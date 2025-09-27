module PomodoroWindowsTimer.ElmishApp.Tests.LoggerExtensions

open Microsoft.Extensions.Logging

let private msgTraceMessage = LoggerMessage.Define<string, string>(
    LogLevel.Trace,
    new EventId(0b1_0000_0001, "Elmish Message"),
    "Dispatched message: {MsgType}:\n{Msg}"
)

let private msgDebugMessage = LoggerMessage.Define<string>(
    LogLevel.Debug,
    new EventId(0b1_0000_0001, "Elmish Message"),
    "Dispatched message: {Msg}."
)

open System.Runtime.CompilerServices
open PomodoroWindowsTimer

[<Extension>]
type LoggerExtensions () =
    [<Extension>]
    static member LogMsg<'Msg when 'Msg: not null>(logger: ILogger, msg: 'Msg) =
        if logger.IsEnabled(LogLevel.Trace) then
            msgTraceMessage.Invoke(
                logger,
                (msg.GetType().FullName.Replace("PomodoroWindowsTimer.ElmishApp.Models.", "").Replace("+", ".")),
                (JsonHelpers.Serialize msg),
                null
            )
        else
            msgDebugMessage.Invoke(
                logger,
                (msg.GetType().FullName.Replace("PomodoroWindowsTimer.ElmishApp.Models.", "").Replace("+", ".")),
                null
            )

