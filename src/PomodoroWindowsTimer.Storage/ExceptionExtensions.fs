namespace PomodoroWindowsTimer.Storage

open System
open System.Runtime.CompilerServices

/// <summary>
/// https://dev.to/techiesdiary/net-60-clean-architecture-using-repository-pattern-and-dapper-with-logging-and-unit-testing-1nd9
/// </summary>
[<Extension>]
type ExceptionExtensions() =

    static let [<Literal>] InnerExceptionName = "Inner Exception"
    static let [<Literal>] ExceptionMessageWithoutInnerException = "{0}{1}: {2}Message: {3}{4}StackTrace: {5}."
    static let [<Literal>] ExceptionMessageWithInnerException = "{0}{1}{2}"

    [<Extension>]
    static member Format(ex: Exception, message: string) =
        let mutable msgAndStackTrace =
            String.Format(
                ExceptionMessageWithoutInnerException,
                Environment.NewLine,
                message,
                Environment.NewLine,
                ex.Message,
                Environment.NewLine,
                ex.StackTrace
            )

        if ex.InnerException <> null then
            msgAndStackTrace <-
                String.Format(
                    ExceptionMessageWithInnerException,
                    msgAndStackTrace,
                    Environment.NewLine,
                    ExceptionExtensions.Format(ex.InnerException, InnerExceptionName)
                )

        msgAndStackTrace + Environment.NewLine
