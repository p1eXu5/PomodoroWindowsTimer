module PomodoroWindowsTimer.ElmishApp.TimePointsGeneratorModel.Program

open System
open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsGeneratorModel
open PomodoroWindowsTimer.Abstractions


let private parsePattern pattern model =
    PatternParser.parse (model.TimePointPrototypes |> List.map _.Prototype.Alias) pattern

let private toTimePoints aliases model =
    let rec running l (state: int array) res =
        match l with
        | [] -> res |> List.rev
        | head :: tail ->
            let ind = model.TimePointPrototypes |> List.findIndex (fun p -> p.Prototype.Alias |> (=) head)
            let prototype =
                (model.TimePointPrototypes |> List.item ind |> _.Prototype |> TimePointPrototype.toTimePoint (ind + 1) state[ind] |> TimePointModel.init)
            do
                Array.set state ind (state[ind] + 1)

            (prototype :: res) |> running tail state

    let counts =
        model.TimePointPrototypes |> List.mapi (fun _ _ -> 1) |> List.toArray

    running aliases counts []

let private toTimePointModels (patternParsedItems: PatternParsedItem list) model =
    let prototypeList =
        model.TimePointPrototypes
        |> List.map (fun tp -> tp.Prototype)

    patternParsedItems
    |> PatternParsedItem.List.timePoints prototypeList
    |> List.map TimePointModel.init

let update
    (patternStore: PatternStore)
    (timePointPrototypeStore: TimePointPrototypeStore)
    (timePointQueue: ITimePointQueue)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<TimePointsGeneratorModel>)
    msg model
    =
    match msg with
    | SetPatterns patternList ->
        { model with Patterns = patternList }, Cmd.none, Intent.None

    | SetSelectedPattern (Some p) ->
        model |> setSelectedPattern p
        , Cmd.OfFunc.perform (parsePattern p) model ProcessParsingResult 
        , Intent.None

    | Msg.ProcessParsingResult res ->
        match res with
        | Ok parsedAliases ->
            model |> unsetIsPatternWrong
            , Cmd.OfFunc.perform (toTimePointModels parsedAliases) model SetGeneratedTimePoints
            , Intent.None
        | Error err ->
            errorMessageQueue.EnqueueError(err)
            model |> setIsPatternWrong, Cmd.none, Intent.None

    | Msg.SetGeneratedTimePoints tpList ->
        model |> setTimePoints tpList
        , Cmd.none, Intent.None

    | SetSelectedPattern None ->
        model |> unsetSelectedPattern |> unsetIsPatternWrong |> clearTimePoints, Cmd.none, Intent.None

    | SetSelectedPatternIndex ind ->
        model |> setSelectedPatternIndex ind, Cmd.none, Intent.None

    | TimePointPrototypeMsg (kind, ptMsg) ->
        ptMsg |> TimePointPrototypeModel.Program.update |> flip (mapPrototype kind) model
        , Cmd.ofMsg (SetSelectedPattern model.SelectedPattern)
        , Intent.None

    | TimePointMsg (id, tpMsg) ->
        tpMsg |> TimePointModel.Program.update |> flip (mapTimePoint id) model
        , Cmd.none
        , Intent.None

    | ApplyTimePoints when not model.IsPatternWrong ->
        model
        , Cmd.batch [
            Cmd.OfFunc.attempt
                (fun (timePoints) -> timePointQueue.Reload timePoints )
                (model.TimePoints |> List.map _.TimePoint)
                Msg.OnExn

            Cmd.OfFunc.attempt
                (fun (selectedPattern, patterns) ->
                    let patterns = selectedPattern |> Option.get |> (fun p -> p :: patterns) |> List.distinct
                    patternStore.Write(patterns)
                )
                (model.SelectedPattern, model.Patterns)
                Msg.OnExn

            Cmd.OfFunc.attempt
                (fun (timePointPrototypes) ->
                    timePointPrototypeStore.Write(timePointPrototypes |> List.map _.Prototype)
                )
                (model.TimePointPrototypes)
                Msg.OnExn
        ]
        , Intent.ApplyGeneratedTimePoints

    | ApplyTimePoints ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none, Intent.None

    | Msg.OnExn ex ->
        logger.LogProgramExn ex
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none, Intent.None
