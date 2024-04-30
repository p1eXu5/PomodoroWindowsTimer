namespace PomodoroWindowsTimer.ElmishApp.Tests

open System
open System.Threading.Tasks
open System.Collections.Generic
open FsToolkit.ErrorHandling
open p1eXu5.FSharp.Testing.ShouldExtensions

type IScenarioContext =
    interface
        abstract ScenarioContext: IDictionary<string, obj>
    end

type Scenario<'StateIn, 'StateOut, 'V> = Scenario of ('StateIn -> 'StateOut * Result<'V, exn>)


module Scenario =

    let retn v = (fun s -> (s, Ok v)) |> Scenario

    let bind f (Scenario m) =
        fun stateIn ->
            let (s, vRes) = m stateIn
            match vRes with
            | Ok v ->
                let (Scenario fm) = f v
                fm s
            | Error err ->
                (Unchecked.defaultof<_>, err |> Error)
        |> Scenario

    let run state (Scenario f) =
        f state |> snd

    let replaceState newState =
        fun _ ->
            (newState, Ok ())
        |> Scenario

    let replaceStateWith<'StateIn, 'StateOut> (newStatef: 'StateIn -> 'StateOut) =
        fun stateIn ->
            try
                (stateIn |> newStatef, Ok ())
            with ex ->
                (Unchecked.defaultof<_>, Error ex)
        |> Scenario

    let inline getState<'State> : Scenario<'State, 'State, 'State> =
        fun stateIn ->
            (stateIn, Ok stateIn)
        |> Scenario


    let runTestAsync (Scenario f) =
        taskResult {
            let (state, res) = f id
            match box state with
            | :? IAsyncDisposable as disp ->
                writeLine "scenario: Disposing..."
                do! disp.DisposeAsync() // may be not needed in elmish v4
                writeLine "scenario: Disposed."
                writeLine "scenario: finished."
                return! res
            | _ ->
                writeLine "scenario: finished."
                return! res
        }
        |> TaskResult.runTest

    let store<'State when 'State :> IScenarioContext> key value : Scenario<'State, 'State, unit> =
        fun (stateIn: 'State) ->
            do stateIn.ScenarioContext.Add(key, value)
            (stateIn, Ok ())
        |> Scenario

    let restore<'State when 'State :> IScenarioContext> key : Scenario<'State, 'State, obj> =
        fun (stateIn: 'State) ->
            try
                (stateIn, stateIn.ScenarioContext[key] |> Ok)
            with ex ->
                (stateIn, Error ex)
        |> Scenario


module ScenarioCE =

    open Scenario

    type ScenarioCEBuilder() =
        member _.Return(v) = retn v
        member _.ReturnFrom(m) = m
        member _.Delay(m) = m
        member _.Run(delayed) = delayed ()
        member _.Zero() = () |> retn
        member _.Bind(m, f) = bind f m
        member _.Combine(Scenario m, delayed) =
            fun stateIn ->
                let (stateOut, vRes) = m stateIn
                match vRes with
                | Ok _ ->
                    let (Scenario fm) = delayed ()
                    fm stateOut
                | Error err ->
                    (Unchecked.defaultof<_>, err |> Error)
            |> Scenario


    let scenario = ScenarioCEBuilder()
