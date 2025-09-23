module PomodoroWindowsTimer.ElmishApp.Logging

open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging
open PomodoroWindowsTimer

let private mssageScope = LoggerMessage.DefineScope<string>(
    "Scope of Message: {Msg}."
)

/// Define LoggerMessage for 'Msg and 'Model string representations.
let private unprocessedMessage = LoggerMessage.Define<string, string>(
    LogLevel.Debug,
    new EventId(0b0_1111_0001, "Unprocessabele Elmish Message"),
    "Unprocessabele message: {Msg}. Model: {Model}."
)

let private modelProgramError = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(0b0_1111_0010, "Model Program Error"),
    "Failed to update model. {Error}"
)

let private modelProgramExn = LoggerMessage.Define(
    LogLevel.Error,
    new EventId(0b0_1111_0011, "Model Program Exception"),
    "Failed to update model."
)

type LoggerExtensions () =
    [<Extension>]
    static member BeginMessageScope<'Msg>(logger: ILogger, _: 'Msg) =
        mssageScope.Invoke(logger, typedefof<'Msg>.Name)

    [<Extension>]
    static member LogUnprocessedMessage<'Msg,'Model when 'Msg: not null and 'Model: not null>(logger: ILogger, msg: 'Msg, model: 'Model) =
        if logger.IsEnabled(LogLevel.Trace) then
            unprocessedMessage.Invoke(
                logger,
                (JsonHelpers.Serialize msg),
                (JsonHelpers.Serialize model),
                null
            )
        else
            unprocessedMessage.Invoke(logger, typedefof<'Msg>.Name, typedefof<'Model>.Name, null)

    [<Extension>]
    static member LogProgramError(logger: ILogger, err: string) =
        modelProgramError.Invoke(logger, err, null)

    [<Extension>]
    static member LogProgramExn(logger: ILogger, ex: exn) =
        modelProgramExn.Invoke(logger, ex)


