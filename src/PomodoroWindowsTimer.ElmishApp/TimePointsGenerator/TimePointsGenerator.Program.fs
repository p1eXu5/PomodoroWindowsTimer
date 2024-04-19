module PomodoroWindowsTimer.ElmishApp.TimePointsGenerator.Program

open Elmish
open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsGenerator
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure


let parsePattern model pattern =
        Parser.parse (model.TimePointPrototypes |> List.map (fun p -> p.KindAlias |> Alias.value)) pattern


let update (patternStore: PatternStore) (timePointPrototypeStore: TimePointPrototypeStore) msg model =
    let parsePattern = parsePattern model

    let toTimePoints aliases =
        let rec running l (state: int array) res =
            match l with
            | [] -> res |> List.rev
            | head :: tail ->
                let ind = model.TimePointPrototypes |> List.findIndex (fun p -> p.KindAlias |> Alias.value |> (=) head)
                let prototype =
                    (model.TimePointPrototypes |> List.item ind |> TimePointPrototype.toTimePoint state[ind])
                do
                    Array.set state ind (state[ind] + 1)

                (prototype :: res) |> running tail state

        let state =
             model.TimePointPrototypes |> List.mapi (fun _ _ -> 1) |> List.toArray
        running aliases state []

    match msg with
    | SetPatterns xp ->
        { model with Patterns = xp }, Cmd.none, Intent.none

    | SetSelectedPattern (Some p) ->
        match parsePattern p with
        | Ok res ->
            model
            |> setSelectedPattern p
            |> setTimePoints (res |> toTimePoints)
            |> unsetIsPatternWrong
            , Cmd.none, Intent.none
        | Error _ ->
            model
            |> setSelectedPattern p
            |> setIsPatternWrong
            |> clearTimePoints
            , Cmd.none, Intent.none

    | SetSelectedPattern None ->
        model |> unsetSelectedPattern |> unsetIsPatternWrong |> clearTimePoints, Cmd.none, Intent.none

    | ProcessParseResult (Ok res) ->
        model |> setTimePoints (res |> toTimePoints) |> unsetIsPatternWrong, Cmd.none, Intent.none

    | ProcessParseResult (Error _) ->
        model |> setIsPatternWrong |> clearTimePoints, Cmd.none, Intent.none

    | SetSelectedPatternIndex ind ->
        model |> setSelectedPatternIndex ind, Cmd.none, Intent.none

    | TimePointPrototypeMsg (kind, ptMsg) ->
        let prototypeInd =
            model.TimePointPrototypes
            |> List.findIndex (fun p -> p.Kind = kind)

        let prototype =
            match ptMsg with
            | SetTimeSpan v when v <> null ->
                { (model.TimePointPrototypes |> List.item prototypeInd) with TimeSpan = System.TimeSpan.Parse(v) } |> Some
            | _ -> None

        prototype
        |> Option.map (fun p ->
            {
                model with
                    TimePointPrototypes =
                        (model.TimePointPrototypes |> List.take prototypeInd)
                        @ [p]
                        @ (model.TimePointPrototypes |> List.skip (prototypeInd + 1))
            }
            , Cmd.ofMsg (SetSelectedPattern model.SelectedPattern)
            , Intent.none
        )
        |> Option.defaultValue (model, Cmd.none, Intent.none)

    | TimePointMsg (id, tpMsg) ->
        let timePointInd =
            model.TimePoints
            |> List.findIndex (fun p -> p.Id = id)

        let timePoint =
            match tpMsg with
            | SetName v when v <> null ->
                { (model.TimePoints |> List.item timePointInd) with Name = v } |> Some
            | _ -> None

        timePoint
        |> Option.map (fun p ->
            {
                model with
                    TimePoints =
                        (model.TimePoints |> List.take timePointInd)
                        @ [p]
                        @ (model.TimePoints |> List.skip (timePointInd + 1))
            }
            , Cmd.none, Intent.none
        )
        |> Option.defaultValue (model, Cmd.none, Intent.none)

    | ApplyTimePoints ->
        let patterns = model.SelectedPattern |> Option.get |> (fun p -> p :: model.Patterns) |> List.distinct
        patternStore.Write(patterns)
        timePointPrototypeStore.Write(model.TimePointPrototypes)
        (model, Cmd.none, Request.ApplyGeneratedTimePoints |> Intent.Request )
