module PomodoroWindowsTimer.Tests.ParserTests

open NUnit.Framework
open FsToolkit.ErrorHandling
open ShouldExtensions
open PomodoroWindowsTimer.Parser
open FsUnit

[<Test>]
let ``parsing w-b-w-b test``() =
    result {
        let input = "w-b-w-b"
        let! res = PomodoroWindowsTimer.Parser.parse ["w"; "b"] input
        res |> should equal (["w"; "b"; "w"; "b"])
    } |> Result.runTest

[<Test>]
let ``parsing (w-b)2-w-lb test``() =
    result {
        let input = "(w-b)2-w-lb"
        let! res = PomodoroWindowsTimer.Parser.parse ["w"; "b"; "lb"] input
        res |> should equal (["w"; "b"; "w"; "b"; "w"; "lb"])
    } |> Result.runTest

[<Test>]
let ``parsing w-(b-w)2-w-lb-(w-b) test``() =
    result {
        let input = "w-(b-w)2-w-lb-(w-b)"
        let! res = PomodoroWindowsTimer.Parser.parse ["w"; "b"; "lb"] input
        res |> should equal (["w"; "b"; "w"; "b"; "w"; "w"; "lb"; "w"; "b"])
    } |> Result.runTest


[<Test>]
let ``parsing " ( w - b ) " test``() =
    result {
        let input = " ( w - b ) "
        let! res = PomodoroWindowsTimer.Parser.parse ["w"; "b"] input
        res |> should equal (["w"; "b"])
    } |> Result.runTest


[<Test>]
let ``parsing " ( w - b ) 2 - w - lb " test``() =
    result {
        let input = " ( w - b ) 2 - w - lb "
        let! res = PomodoroWindowsTimer.Parser.parse ["w"; "b"; "lb"] input
        res |> should equal (["w"; "b"; "w"; "b"; "w"; "lb"])
    } |> Result.runTest