module PomodoroWindowsTimer.Looper.LoggingExtensions

open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging

open PomodoroWindowsTimer
open Microsoft.FSharp.Reflection


let private beginMessageScope =
    LoggerMessage.DefineScope<string>(
        "Scope of Looper message: {LooperMsgName}"
    )

let private startHandleMessage = LoggerMessage.Define<string>(
    LogLevel.Debug,
    new EventId(0b0_0010_0001, "Start Handle Looper Message"),
    "Start Looper message handling: {LooperMsgName}..."
)

let private startHandleWithStateMessage = LoggerMessage.Define<string, bool, string option>(
    LogLevel.Debug,
    new EventId(0b0_0010_0001, "Start Handle Looper Message"),
    "Start handling message: {LooperMsgName}...\n     (Looper IsStopped: {IsStopped}, active TimePoint name: {ActiveTimePointName})."
)

let private looperNewState = LoggerMessage.Define<string>(
    LogLevel.Trace,
    new EventId(0b0_0010_0010, "Looper State Updated"),
    "Looper state updated to {LooperNewState}"
)

let private looperCurrentStateTraceMessage = LoggerMessage.Define<string>(
    LogLevel.Trace,
    new EventId(0b0_0010_0011, "Looper Msg Read"),
    "Looper current state:\n    {LooperCurrentState}."
)

let private looperCurrentStateDebugMessage = LoggerMessage.Define<bool, string option>(
    LogLevel.Debug,
    new EventId(0b0_0010_0011, "Looper Msg Read"),
    "Looper IsStopped: {IsStopped}, active TimePoint name: {ActiveTimePointName}."
)

let private looperStateUpdated = LoggerMessage.Define(
    LogLevel.Debug,
    new EventId(0b0_0010_0100, "Looper State Updated"),
    "Looper state updated"
)

let private endHandleMessage = LoggerMessage.Define<string>(
    LogLevel.Debug,
    new EventId(0b0_0010_0101, "End Handle Looper Message"),
    "Looper message has been handled: {LooperMsgName}."
)

let private unprocessedMsgMessage = LoggerMessage.Define<string, string>(
    LogLevel.Warning,
    new EventId(0b0_0010_0101, "Looper Unhandled Message"),
    "Looper message has been unprocessed: {LooperMsgName}, because {Reason}."
)

type internal LoggerExtensions () =

    [<Extension>]
    static member BeginMessageScope(logger: ILogger, looperMsgName: string) =
        beginMessageScope.Invoke(logger, looperMsgName)

    [<Extension>]
    static member LogStartHandleMessage(logger: ILogger, looperMsgName: string) =
        startHandleMessage.Invoke(logger, looperMsgName, null)

    [<Extension>]
    static member LogStartHandleMessage(logger: ILogger, looperMsgName: string, isLooperStopoped: bool, activeTimePointName: string option) =
        startHandleWithStateMessage.Invoke(logger, looperMsgName, isLooperStopoped, activeTimePointName, null)

    [<Extension>]
    static member LogEndHandleMessage(logger: ILogger, looperMsgName: string) =
        endHandleMessage.Invoke(logger, looperMsgName, null)

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
        looperCurrentStateTraceMessage.Invoke(logger, (JsonHelpers.Serialize state), null)

    [<Extension>]
    static member LogLooperCurrentState(logger: ILogger, isLooperStopoped: bool, activeTimePointName: string option) =
        looperCurrentStateDebugMessage.Invoke(logger, isLooperStopoped, activeTimePointName, null)

    [<Extension>]
    static member LogUnprocessedMessage(logger: ILogger, msgName: string, reason: string) =
        unprocessedMsgMessage.Invoke(logger, msgName, reason, null)

