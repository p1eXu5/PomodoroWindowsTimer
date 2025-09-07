module PomodoroWindowsTimer.Looper.LoggingExtensions

open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging

open PomodoroWindowsTimer


let private beginMessageScope =
    LoggerMessage.DefineScope<string>(
        "Scope of Looper message: {LooperMsgName}"
    )

let private startHandleMessage = LoggerMessage.Define<string>(
    LogLevel.Debug,
    new EventId(0b0_0010_0001, "Start Handle Looper Message"),
    "Start handle Looper message: {LooperMsgName}"
)

let private looperNewState = LoggerMessage.Define<string>(
    LogLevel.Trace,
    new EventId(0b0_0010_0010, "Looper State Updated"),
    "Looper state updated to {LooperNewState}"
)

let private looperCurrentState = LoggerMessage.Define<string>(
    LogLevel.Trace,
    new EventId(0b0_0010_0011, "Looper State Updated"),
    "Looper current state: {LooperCurrentState}"
)

let private looperStateUpdated = LoggerMessage.Define(
    LogLevel.Debug,
    new EventId(0b0_0010_0100, "Looper State Updated"),
    "Looper state updated"
)

type internal LoggerExtensions () =

    [<Extension>]
    static member BeginMessageScope(logger: ILogger, looperMsgName: string) =
        beginMessageScope.Invoke(logger, looperMsgName)

    [<Extension>]
    static member LogStartHandleMessage(logger: ILogger, looperMsgName: string) =
        startHandleMessage.Invoke(logger, looperMsgName, null)

    [<Extension>]
    static member LogLooperStateUpdated(logger: ILogger, state: obj) =
        if logger.IsEnabled(LogLevel.Trace) then
            looperNewState.Invoke(
                logger,
                (JsonHelpers.Serialize state),
                null
            )
        else
            looperStateUpdated.Invoke(logger, null)

    [<Extension>]
    static member LogLooperCurrentState(logger: ILogger, state: obj) =
        looperCurrentState.Invoke(logger, (JsonHelpers.Serialize state), null)

