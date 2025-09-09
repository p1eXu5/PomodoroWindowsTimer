module PomodoroWindowsTimer.Tests.PatternParserTests

open NUnit.Framework
open FsToolkit.ErrorHandling
open p1eXu5.FSharp.Testing.ShouldExtensions
open PomodoroWindowsTimer.PatternParser
open FsUnit
open PomodoroWindowsTimer.Types

[<Test>]
let ``parsing w-b-w-b test``() =
    result {
        let input = "w-b-w-b"
        let! res = PomodoroWindowsTimer.PatternParser.parse (["w"; "b"] |> List.map Alias.createOrThrow) input
        res |> should equal (["w"; "b"; "w"; "b"] |> List.map Alias.createOrThrow)
    } |> Result.runTest

[<Test>]
let ``parsing (w-b)2-w-lb test``() =
    result {
        let input = "(w-b)2-w-lb"
        let! res = PomodoroWindowsTimer.PatternParser.parse (["w"; "b"; "lb"] |> List.map Alias.createOrThrow) input
        res |> should equal (["w"; "b"; "w"; "b"; "w"; "lb"] |> List.map Alias.createOrThrow)
    } |> Result.runTest

[<Test>]
let ``parsing w-(b-w)2-w-lb-(w-b) test``() =
    result {
        let input = "w-(b-w)2-w-lb-(w-b)"
        let! res = PomodoroWindowsTimer.PatternParser.parse (["w"; "b"; "lb"] |> List.map Alias.createOrThrow) input
        res |> should equal (["w"; "b"; "w"; "b"; "w"; "w"; "lb"; "w"; "b"] |> List.map Alias.createOrThrow)
    } |> Result.runTest


[<Test>]
let ``parsing " ( w - b ) " test``() =
    result {
        let input = " ( w - b ) "
        let! res = PomodoroWindowsTimer.PatternParser.parse (["w"; "b"] |> List.map Alias.createOrThrow) input
        res |> should equal (["w"; "b"] |> List.map Alias.createOrThrow)
    } |> Result.runTest


[<Test>]
let ``parsing " ( w - b ) 2 - w - lb " test``() =
    result {
        let input = " ( w - b ) 2 - w - lb "
        let! res = PomodoroWindowsTimer.PatternParser.parse (["w"; "b"; "lb"] |> List.map Alias.createOrThrow) input
        res |> should equal (["w"; "b"; "w"; "b"; "w"; "lb"] |> List.map Alias.createOrThrow)
    } |> Result.runTest