﻿[<AutoOpen>]
module PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

open System
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.Types

type [<Measure>] times

module MainModel =

    open PomodoroWindowsTimer.ElmishApp.Models
    ()

    module Msg =
        ()

    module MsgWith =
        ()


module Scenario =

    open System.Threading
    open FsUnit
    open p1eXu5.FSharp.Testing.ShouldExtensions
    open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE

    [<Literal>]
    let private ``1sec`` = 1000

    [<Literal>]
    let private ``2sec`` = 2000

    let msgDispatchedWithin2Sec msgDescription msgPredicate =
        scenario {
            let! (state: ISut) = Scenario.getState
            let msgPresents = SpinWait.SpinUntil(
                Func<bool>(fun () -> state.MsgStack |> Seq.exists msgPredicate),
                ``2sec``)
            msgPresents |> shouldL be True $"%s{msgDescription} has not been dispatched within 2 seconds."
        }

    let msgDispatchedWithin2SecT msgDescription (times: int<times>) msgPredicate =
        scenario {
            let! (state: ISut) = Scenario.getState
            let msgPresents = SpinWait.SpinUntil(
                Func<bool>(fun () -> state.MsgStack |> Seq.filter msgPredicate |> Seq.length |> (=) (int times)),
                ``2sec``)
            msgPresents |> shouldL be True $"%s{msgDescription} has not been dispatched within 2 seconds."
        }

    let msgDispatchedWithin (delay: float<sec>) msgDescription msgPredicate =
        scenario {
            let! (state: ISut) = Scenario.getState
            let msgPresents = SpinWait.SpinUntil(
                Func<bool>(fun () -> state.MsgStack |> Seq.exists msgPredicate),
                int (float delay * 1000.0))
            msgPresents |> shouldL be True $"%s{msgDescription} has not been dispatched within {delay} seconds."
        }

    let msgNotDispatchedWithin1Sec msgDescription msgPredicate =
        scenario {
            let! (state: ISut) = Scenario.getState
            let msgPresents = SpinWait.SpinUntil(
                Func<bool>(fun () -> state.MsgStack |> Seq.exists msgPredicate),
                ``1sec``)
            msgPresents |> shouldL be False $"%s{msgDescription} has been dispatched within 2 seconds."
        }

    let modelSatisfiesWithin2Sec modelDescription modelPredicate =
        scenario {
            let! (state: ISut) = Scenario.getState
            let msgPresents = SpinWait.SpinUntil(
                Func<bool>(fun () -> state.MainModel |> modelPredicate),
                ``2sec``)
            msgPresents |> shouldL be True $"%A{modelDescription} is not satisfies modelPredicate. MainModel:\n{state.MainModel}"
        }

    let dispatch msg =
        scenario {
            let! (state: #ISut) = Scenario.getState
            do state.Dispatcher.Dispatch msg
        }

    let dispatchAndWait msgDescription msg =
        scenario {
            do! dispatch msg
            do! msgDispatchedWithin2Sec msgDescription ((=) msg)
        }

    let log scenatioName (m: Scenario<_,_,_>) =
        scenario {
            do writeLine $"``scenario``: {scenatioName}"
            do writeLine  "              Starting..."
            let! v = m
            do writeLine $"``scenario``: {scenatioName}"
            do writeLine  "              Finished."
            return v
        }


