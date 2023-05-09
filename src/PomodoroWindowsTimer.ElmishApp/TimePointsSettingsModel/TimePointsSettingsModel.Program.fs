module PomodoroWindowsTimer.ElmishApp.TimePointsSettingsModel.Program

open Elmish
open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsSettingsModel


let update msg model =
    let parsePattern pattern =
        Parser.parse (model.TimePointPrototypes |> List.map (fun p -> p.Alias |> Alias.value)) pattern

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
    | SetSelectedPattern (Some p) ->
        model |> setSelectedPattern p, Cmd.OfFunc.perform parsePattern p Msg.ProcessParseResult
    
    | SetSelectedPattern None ->
        model |> unsetSelectedPattern |> unsetIsPatternWrong |> clearTimePoints, Cmd.none

    | ProcessParseResult (Ok res) ->
        model |> setTimePoints (res |> toTimePoints) |> unsetIsPatternWrong, Cmd.none

    | ProcessParseResult (Error _) ->
        model |> setIsPatternWrong |> clearTimePoints, Cmd.none

    | SetSelectedPatternIndex ind ->
        model |> setSelectedPatternIndex ind, Cmd.none

    | ParsePattern _
    | _ ->
        model, Cmd.none

