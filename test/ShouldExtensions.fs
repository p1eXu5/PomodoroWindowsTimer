namespace p1eXu5.FSharp.Testing.ShouldExtensions

open NUnit.Framework
open FsUnit
open System.Diagnostics
open NUnit.Framework.Constraints
open System.Threading.Tasks

[<AutoOpen>]
module Helpers =
    let failAssert message =
        raise (new AssertionException(message))

    let writeLine (message: string) =
        TestContext.WriteLine(message)

    let assertionExn message = raise (AssertionException(message))

[<AutoOpen>]
module FsUnit =

    let inline writeLine s = TestContext.WriteLine(sprintf "%A%A" s s)
    let inline writeLineS (s: string) = TestContext.WriteLine(s)
    let inline writeLinef<'a> (format: Printf.StringFormat<'a -> string>) s =
        TestContext.WriteLine(sprintf format s)

    let toTask computation : Task = Async.StartAsTask computation :> _


    [<DebuggerNonUserCode>]
    let shouldL (f: 'a -> #Constraint) x message (y: obj) =
        let c = f x

        let y =
            match y with
            | :? (unit -> unit) -> box(TestDelegate(y :?> unit -> unit))
            | _ -> y

        if isNull(box c) 
            then Assert.That(y, Is.Null) 
            else
                let divider = String.replicate 40 "-"
                Assert.That(y, c, fun _ -> sprintf "%s\n  %s" message divider)


[<RequireQualifiedAccess>]
module Result =

    let runTest<'Ok, 'Error> (res: Result<'Ok, 'Error>) =
        match res with
        | Result.Ok _ -> ()
        | Result.Error err -> raise (AssertionException(sprintf "%A" err))

    [<DebuggerStepThrough>]
    let inline shouldEqual expected = function
        | Result.Ok ok -> ok |> FsUnit.shouldL equal expected ""
        | Result.Error err -> raise (AssertionException($"Should be %A{expected} but there is an error: %A{err}"))

    [<DebuggerStepThrough>]
    let inline shouldBe expected = function
        | Result.Ok ok -> ok |> FsUnit.shouldL be expected ""
        | Result.Error err -> raise (AssertionException($"Should be %A{expected} but there is an error: %A{err}"))

    [<DebuggerStepThrough>]
    let inline should ``constraint`` expected = function
        | Result.Ok ok -> ok |> FsUnit.shouldL ``constraint`` expected ""
        | Result.Error err -> raise (AssertionException($"Should be %A{expected} but there is an error: %A{err}"))

    [<DebuggerStepThrough>]
    let inline shouldBeError expected = function
        | Result.Ok ok -> raise (AssertionException($"Error should be %A{expected} but there is an ok: %A{ok}"))
        | Result.Error err -> err |> FsUnit.shouldL be expected ""

    [<DebuggerStepThrough>]
    let inline shouldError ``constraint`` expected = function
        | Result.Ok ok -> raise (AssertionException($"Error should be %A{expected} but there is an ok: %A{ok}"))
        | Result.Error err -> err |> FsUnit.shouldL ``constraint`` expected ""


[<RequireQualifiedAccess>]
module TaskResult =
    let runTest taskResult =
        try
            taskResult
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> Result.runTest
        with
            | :? System.AggregateException as ae ->
                for e in ae.Flatten().InnerExceptions do
                    printfn "\nError!%s\n\n\%s" e.Message e.StackTrace
                reraise()
                // raise (AssertionException(e.Message, e))