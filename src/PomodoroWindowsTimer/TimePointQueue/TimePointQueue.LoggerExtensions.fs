module PomodoroWindowsTimer.TimePointQueue.LoggingExtensions

open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types

let private messageScope =
    LoggerMessage.DefineScope<string>(
        "Scope of message: {TimePointQueueMsgName}"
    )

let private startHandleMessage =
    LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(0b0_0001_0001, "Start Handle TimePointQueue Message"),
        "Start handling {TimePointQueueMsgName} message..."
    )

let private startHandleWithArgMessage =
    LoggerMessage.Define<string, string>(
        LogLevel.Trace,
        new EventId(0b0_0001_0001, "Start Handle TimePointQueue Message"),
        "Start handling {TimePointQueueMsgName} message with args: {MsgArgs}..."
    )

let private timePointsMessage =
    LoggerMessage.Define<string>(
        LogLevel.Trace,
        new EventId(0b0_0001_0011, "TimePointQueue TimePoint List"),
        "TimePoint list: {TimePointList}"
    )

let private nextTimePointMessage =
    LoggerMessage.Define<string, float32>(
        LogLevel.Trace,
        new EventId(0b0_0001_0100, "Next TimePointQueue TimePoint"),
        "Replying with the next TimePoint: {TimePoint} with priority {Priority}"
    )

let private endHandleMessage =
    LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(0b0_0001_1000, "End Handle TimePointQueue Message"),
        "{TimePointQueueMsgName} has been handled."
    )

type internal LoggerExtensions () =

    [<Extension>]
    static member BeginHandle(logger: ILogger, msgName: string) =
        let scope = messageScope.Invoke(logger, msgName)
        startHandleMessage.Invoke(logger, msgName, null)
        { new System.IDisposable with
            member _.Dispose() =
                endHandleMessage.Invoke(logger, msgName, null)
                scope
                |> function NonNull s -> s.Dispose() | _ -> ()
        }

    [<Extension>]
    static member BeginHandle(logger: ILogger, msgName: string, args: obj) =
        let scope = messageScope.Invoke(logger, msgName)
        logger.LogStartHandleMsg(msgName, args)
        { new System.IDisposable with
            member _.Dispose() =
                endHandleMessage.Invoke(logger, msgName, null)
                scope
                |> function NonNull s -> s.Dispose() | _ -> ()
        }

    [<Extension>]
    static member LogStartHandleMsg(logger: ILogger, msgName: string) =
        startHandleMessage.Invoke(logger, msgName, null)

    [<Extension>]
    static member LogStartHandleMsg(logger: ILogger, msgName: string, args: obj) =
        if logger.IsEnabled(LogLevel.Trace) then
            let json = JsonHelpers.Serialize args
            startHandleWithArgMessage.Invoke(logger, msgName, json, null)
        else
            startHandleWithArgMessage.Invoke(logger, msgName, args.GetType().Name, null)

    [<Extension>]
    static member LogTimePoints(logger: ILogger, timePoints: (TimePoint * float32) seq) =
        if logger.IsEnabled(LogLevel.Trace) then
            let json = JsonHelpers.Serialize(timePoints)
            timePointsMessage.Invoke(logger, json, null)

    [<Extension>]
    static member LogNextTimePoint(logger: ILogger, tp: TimePoint, priority: float32) =
        if logger.IsEnabled(LogLevel.Trace) then
            let json = JsonHelpers.Serialize(tp)
            nextTimePointMessage.Invoke(logger, json, priority, null)
