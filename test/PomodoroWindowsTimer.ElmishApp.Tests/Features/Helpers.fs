[<AutoOpen>]
module PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

open System
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp.Tests

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

    let msgDispatchedWithin2Sec msgDescription msgPredicate =
        scenario {
            let! (state: ISut) = Scenario.getState
            let msgPresents = SpinWait.SpinUntil(
                Func<bool>(fun () -> state.MsgStack |> Seq.exists msgPredicate),
                200000)
            msgPresents |> shouldL be True $"%s{msgDescription} has not been dispatched within 2 seconds."
        }

    let msgNotDispatchedWithin1Sec msgDescription msgPredicate =
        scenario {
            let! (state: ISut) = Scenario.getState
            let msgPresents = SpinWait.SpinUntil(
                Func<bool>(fun () -> state.MsgStack |> Seq.exists msgPredicate),
                1000)
            msgPresents |> shouldL be False $"%s{msgDescription} has been dispatched within 2 seconds."
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
            do writeLine $"scenario: {scenatioName}"
            do writeLine  "          Starting..."
            let! v = m
            do writeLine $"scenario: {scenatioName}"
            do writeLine  "          Finished."
            return v
        }


