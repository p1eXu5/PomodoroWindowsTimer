module PomodoroWindowsTimer.ElmishApp.TimePointsSettingsModel.Program

open Elmish
open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsSettingsModel


let parsePattern model pattern =
        Parser.parse (model.TimePointPrototypes |> List.map (fun p -> p.Alias |> Alias.value)) pattern


let update msg model =
    let parsePattern = parsePattern model

    let toTimePoints aliases =
        let rec running l (state: int array) res =
            match l with
            | [] -> res |> List.rev
            | head :: tail ->
                let ind = model.TimePointPrototypes |> List.findIndex (fun p -> p.Alias |> Alias.value |> (=) head)
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
        { model with Patterns = xp }, Cmd.none

    | SetSelectedPattern (Some p) ->
        match parsePattern p with
        | Ok res ->
            model
            |> setSelectedPattern p
            |> setTimePoints (res |> toTimePoints)
            |> unsetIsPatternWrong
            , Cmd.none
        | Error _ ->
            model
            |> setSelectedPattern p
            |> setIsPatternWrong
            |> clearTimePoints
            , Cmd.none

    | SetSelectedPattern None ->
        model |> unsetSelectedPattern |> unsetIsPatternWrong |> clearTimePoints, Cmd.none

    | ProcessParseResult (Ok res) ->
        model |> setTimePoints (res |> toTimePoints) |> unsetIsPatternWrong, Cmd.none

    | ProcessParseResult (Error _) ->
        model |> setIsPatternWrong |> clearTimePoints, Cmd.none

    | SetSelectedPatternIndex ind ->
        model |> setSelectedPatternIndex ind, Cmd.none

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
        )
        |> Option.defaultValue (model, Cmd.none)
