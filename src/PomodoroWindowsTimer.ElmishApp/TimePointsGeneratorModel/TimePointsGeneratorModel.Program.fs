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


let private toTimePointModels timePointPrototypes (patternParsedItems: PatternParsedItem list) =
    let prototypeList =
        timePointPrototypes
        |> List.map (fun tp -> tp.Prototype)

    patternParsedItems
    |> PatternParsedItem.List.timePoints prototypeList
    |> fun (l, times) ->
        l |> List.map TimePointModel.init
        , times

let private parsePattern pattern timePointPrototypes =
    PatternParser.parse (timePointPrototypes |> List.map _.Prototype.Alias) pattern
    |> Result.map (toTimePointModels timePointPrototypes)
    |> Result.map (fun (tpList, times) -> (tpList, times, timePointPrototypes, pattern))
    |> Result.mapError (fun err -> (err, timePointPrototypes, pattern))


let update
    (patternStore: PatternStore)
    (timePointPrototypeStore: TimePointPrototypeStore)
    (timePointQueue: ITimePointQueue)
    (errorMessageQueue: IErrorMessageQueue)
    (logger: ILogger<TimePointsGeneratorModel>)
    updateTimePointModel
    msg model
    =
    match msg with
    | SetPatterns patternList ->
        { model with Patterns = patternList }
        , (
            match patternList with
            | [] -> Cmd.none
            | head :: _ -> Cmd.ofMsg (SetPattern (Some head))
        )
        , Intent.None

    | Msg.SetPrototypes prototypes ->
        model
        |> withPrototypes prototypes
        |> fun m ->
            m
            , Cmd.batch [
                (
                    match prototypes, m.Pattern with
                    | l, Some p -> Cmd.OfFunc.perform (parsePattern p) l SetGeneratedTimePoints
                    | _ -> Cmd.none
                )
                Cmd.OfFunc.attempt (fun p -> timePointPrototypeStore.Write(p |> List.map _.Prototype)) prototypes Msg.OnExn
            ]
            , Intent.None

    | SetPattern (Some p) ->
        model |> setSelectedPattern p
        , (
            match model.TimePointPrototypes with
            | [] -> Cmd.none
            | l -> Cmd.OfFunc.perform (parsePattern p) l SetGeneratedTimePoints 
        )
        , Intent.None

    | SetPattern None ->
        model |> unsetSelectedPattern |> unsetIsPatternWrong |> clearTimePoints, Cmd.none, Intent.None

    | Msg.SetGeneratedTimePoints res ->
        match res with
        | Ok (tpList, times, prototypes, pattern) when model.TimePointPrototypes = prototypes && model.Pattern |> Option.map (fun p -> p = pattern) |> Option.defaultValue false ->
            model
            |> unsetIsPatternWrong
            |> setTimePoints tpList
            |> setTimes times
            , Cmd.none
            , Intent.None
        | Error (err, prototypes, pattern) when model.TimePointPrototypes = prototypes && model.Pattern |> Option.map (fun p -> p = pattern) |> Option.defaultValue false ->
            errorMessageQueue.EnqueueError(err)
            model
            |> setIsPatternWrong
            |> clearTimePoints
            , Cmd.none, Intent.None

        | _ ->
            logger.LogUnprocessedMessage(msg, model)
            model, Cmd.none, Intent.None

    | SetSelectedPatternIndex ind ->
        model |> setSelectedPatternIndex ind, Cmd.none, Intent.None

    | TimePointPrototypeMsg (kind, ptMsg) ->
        ptMsg |> TimePointPrototypeModel.Program.update |> flip (mapPrototype kind) model
        , Cmd.ofMsg (SetPattern model.Pattern)
        , Intent.None

    | TimePointMsg (id, tpMsg) ->
        tpMsg |> TimePointModel.Program.update |> flip (mapTimePoint id) model
        , Cmd.none
        , Intent.None

    | ApplyTimePoints when not model.IsPatternWrong && model.Pattern.IsSome ->
        model
        , Cmd.batch [
            Cmd.OfFunc.attempt
                (fun (timePoints) ->
                    timePointQueue.Reload timePoints
                )
                (model.TimePoints |> List.map _.TimePoint)
                Msg.OnExn

            (
                let pattern = model.Pattern |> Option.get
                match model.Patterns |> List.tryFindIndex ((=) pattern) with
                | Some ind ->
                    Cmd.OfFunc.attempt
                        (fun (patterns, ind) ->
                            patternStore.Write((patterns |> List.item ind) :: (patterns |> List.removeAt ind)))
                        (model.Patterns, ind)
                        Msg.OnExn
                | None ->
                    Cmd.OfFunc.attempt
                        (fun (patterns) -> patternStore.Write(patterns))
                        (pattern :: model.Patterns)
                        Msg.OnExn
            )
        ]
        , Intent.ApplyGeneratedTimePoints

    | ApplyTimePoints ->
        logger.LogUnprocessedMessage(msg, model)
        model, Cmd.none, Intent.None

    | RequestCancelling ->
        model, Cmd.none, Intent.CancelTimePointGeneration

    | Msg.OnExn ex ->
        logger.LogProgramExn ex
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none, Intent.None
