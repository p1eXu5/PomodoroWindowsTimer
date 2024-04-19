module PomodoroWindowsTimer.ElmishApp.TimePointsGeneratorModel.Program

open Elmish
open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsGeneratorModel
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure


let private parsePattern pattern model =
    Parser.parse (model.TimePointPrototypes |> List.map _.Prototype.KindAlias) pattern

let private toTimePoints aliases model =
    let rec running l (state: int array) res =
        match l with
        | [] -> res |> List.rev
        | head :: tail ->
            let ind = model.TimePointPrototypes |> List.findIndex (fun p -> p.Prototype.KindAlias |> (=) head)
            let prototype =
                (model.TimePointPrototypes |> List.item ind |> _.Prototype |> TimePointPrototype.toTimePoint state[ind] |> TimePointModel.init)
            do
                Array.set state ind (state[ind] + 1)

            (prototype :: res) |> running tail state

    let counts =
        model.TimePointPrototypes |> List.mapi (fun _ _ -> 1) |> List.toArray

    running aliases counts []

let update (patternStore: PatternStore) (timePointPrototypeStore: TimePointPrototypeStore) (errorMessageQueue: IErrorMessageQueue) msg model =

    match msg with
    | SetPatterns patternList ->
        { model with Patterns = patternList }, Cmd.none, Intent.none

    | SetSelectedPattern (Some p) ->
        model |> setSelectedPattern p
        , Cmd.OfFunc.perform (parsePattern p) model ProcessParsingResult 
        , Intent.None

    | Msg.ProcessParsingResult res ->
        match res with
        | Ok parsedAliases ->
            model |> unsetIsPatternWrong
            , Cmd.OfFunc.perform (toTimePoints parsedAliases) model SetGeneratedTimePoints
            , Intent.None
        | Error err ->
            errorMessageQueue.EnqueueError(err)
            model |> setIsPatternWrong, Cmd.none, Intent.None

    | Msg.SetGeneratedTimePoints tpList ->
        model |> setTimePoints tpList
        , Cmd.none, Intent.none

    | SetSelectedPattern None ->
        model |> unsetSelectedPattern |> unsetIsPatternWrong |> clearTimePoints, Cmd.none, Intent.none

    | SetSelectedPatternIndex ind ->
        model |> setSelectedPatternIndex ind, Cmd.none, Intent.none

    | TimePointPrototypeMsg (kind, ptMsg) ->
        ptMsg |> TimePointPrototypeModel.Program.update |> flip (mapPrototype kind) model
        , Cmd.ofMsg (SetSelectedPattern model.SelectedPattern)
        , Intent.none

    | TimePointMsg (id, tpMsg) ->
        tpMsg |> TimePointModel.Program.update |> flip (mapTimePoint id) model
        , Cmd.none
        , Intent.none

    | ApplyTimePoints ->
        let patterns = model.SelectedPattern |> Option.get |> (fun p -> p :: model.Patterns) |> List.distinct
        patternStore.Write(patterns)
        timePointPrototypeStore.Write(model.TimePointPrototypes |> List.map _.Prototype)
        (model, Cmd.none, Request.ApplyGeneratedTimePoints |> Intent.Request )
