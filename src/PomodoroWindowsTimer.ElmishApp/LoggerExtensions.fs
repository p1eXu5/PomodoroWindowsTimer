module PomodoroWindowsTimer.ElmishApp.Logging

open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging
open PomodoroWindowsTimer

[<AutoOpen>]
module MessageScope =
    let private loggerMessage = LoggerMessage.DefineScope<string>(
        "Scope of Message: {Message}."
    )

    type LoggerExtensions () =
        [<Extension>]
        static member BeginMessageScope<'Msg>(logger: ILogger, msg: 'Msg) =
            loggerMessage.Invoke(logger, typedefof<'Msg>.Name)


[<AutoOpen>]
module UnprocessedMessage =
    let private loggerMessage = LoggerMessage.Define<string, string>(
        LogLevel.Warning,
        new EventId(0b0_1111_0001, "Unprocessabele Elmish Message"),
        "Unprocessabele message: {Message}. Model: {Model}."
    )

    type LoggerExtensions () =
        [<Extension>]
        static member LogUnprocessedMessage<'Msg,'Model>(logger: ILogger, msg: 'Msg, model: 'Model) =
            if logger.IsEnabled(LogLevel.Trace) then
                loggerMessage.Invoke(
                    logger,
                    (JsonHelpers.Serialize msg),
                    (JsonHelpers.Serialize model),
                    null
                )
            else
                loggerMessage.Invoke(logger, typedefof<'Msg>.Name, typedefof<'Model>.Name, null)


