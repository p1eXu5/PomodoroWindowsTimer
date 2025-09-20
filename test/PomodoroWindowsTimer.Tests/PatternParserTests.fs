module PomodoroWindowsTimer.Tests.PatternParserTests

open System
open System.Collections

open NUnit.Framework
open FsUnit
open FsToolkit.ErrorHandling
open p1eXu5.FSharp.Testing.ShouldExtensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.PatternParser

let cases : IEnumerable =
    [
        TestCaseData(
            "w-b-w-b",
            [
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
            ]
        ).SetName("01: parsing w-b-w-b test")

        TestCaseData(
            " ( w - b ) ",
            [
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
            ]
        ).SetName("02: parsing ' ( w - b ) ' test")

        TestCaseData(
            "(w-b)2-w-lb",
            [
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "lb"
            ]
        ).SetName("03: parsing (w-b)2-w-lb test")

        TestCaseData(
            " ( w - b ) 2 - w - lb ",
            [
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "lb"
            ]
        ).SetName("04: parsing ' ( w - b ) 2 - w - lb ' test")

        TestCaseData(
            "w-(b-w)2-w-lb-(w-b)",
            [
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "lb"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
            ]
        ).SetName("05: parsing w-(b-w)2-w-lb-(w-b) test")

        TestCaseData(
            "b[:20]",
            [
                PatternParsedItem.ofAliasTimeSpanOrThrow "b" (TimeSpan(0, 20, 0))
            ]
        ).SetName("06: parsing b[:20] test")

        TestCaseData(
            "(b[20.])",
            [
                PatternParsedItem.ofAliasTimeSpanOrThrow "b" (TimeSpan(0, 0, 20))
            ]
        ).SetName("07: parsing (b[20.]) test")

        TestCaseData(
            "(b[:20]-w[20.])1-lb[:20:20]-w[1:30:0]-lb[.123]-w[:20:0.100]",
            [
                PatternParsedItem.ofAliasTimeSpanOrThrow "b" (TimeSpan(0, 20, 0))
                PatternParsedItem.ofAliasTimeSpanOrThrow "w" (TimeSpan(0, 0, 20))
                PatternParsedItem.ofAliasTimeSpanOrThrow "lb" (TimeSpan(0, 20, 20))
                PatternParsedItem.ofAliasTimeSpanOrThrow "w" (TimeSpan(1, 30, 0))
                PatternParsedItem.ofAliasTimeSpanOrThrow "lb" (TimeSpan(0, 0, 0, 0, 123))
                PatternParsedItem.ofAliasTimeSpanOrThrow "w" (TimeSpan(0, 0, 20, 0, 100))
            ]
        ).SetName("08: parsing (b[:20]-w[20.])1-lb[:20:20]-w[1:30:0]-lb[.123]-w[:20:0.100] test")

        TestCaseData(
            "b[:20, Test Break]",
            [
                PatternParsedItem.ofAliasTimeSpanNameOrThrow "b" (TimeSpan(0, 20, 0)) "Test Break"
            ]
        ).SetName("09: parsing b[:20, Test Break] test")

        TestCaseData(
            "((w-b)2)2",
            [
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
                PatternParsedItem.ofAliasOrThrow "w"
                PatternParsedItem.ofAliasOrThrow "b"
            ]
        ).SetName("10: parsing '((w-b)2)2'")
    ]

[<TestCaseSource(nameof cases)>]
let ``parsing tests``(input, expected) =
    result {
        let! res = PomodoroWindowsTimer.PatternParser.parse Alias.Defaults.all input
        res |> should equal expected
    } |> Result.runTest
