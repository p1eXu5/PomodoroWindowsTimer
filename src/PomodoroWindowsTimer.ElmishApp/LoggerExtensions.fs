module PomodoroWindowsTimer.ElmishApp.Logging

open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging
open PomodoroWindowsTimer

[<AutoOpen>]
module UnprocessedMessage =
    let private loggerMessage = LoggerMessage.Define<string, string>(
        LogLevel.Warning,
        new EventId(0b0_1111_0001, "Unprocessabele Elmish Message"),
        "Unprocessabele message - {Message}. Model - {Model}."
    )

    type LoggerExtensions () =
        [<Extension>]
        static member LogUnprocessedMessage(logger: ILogger, message: obj, model: obj) =
            if logger.IsEnabled(LogLevel.Trace) then
                loggerMessage.Invoke(
                    logger,
                    (JsonHelpers.Serialize message),
                    (JsonHelpers.Serialize model),
                    null
                )
            else
                loggerMessage.Invoke(logger, message.GetType().Name, model.GetType().Name, null)
