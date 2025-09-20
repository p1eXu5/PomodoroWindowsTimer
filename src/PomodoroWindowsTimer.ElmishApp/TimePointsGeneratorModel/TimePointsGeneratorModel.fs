namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types

type TimePointsGeneratorModel =
    {
        TimePointPrototypes: TimePointPrototypeModel list
        TimePoints: TimePointModel list
        Patterns: string list
        SelectedPattern: string option
        SelectedPatternIndex: int
        IsPatternWrong: bool
        TimePointsTime: TimePointsTime voption
    }


module TimePointsGeneratorModel =

    type Msg =
        | SetPatterns of string list
        | ProcessParsingResult of Result<PatternParsedItem list, string>
        | SetGeneratedTimePoints of (TimePointModel list * TimePointsTime)
        | SetSelectedPatternIndex of int
        | SetSelectedPattern of Pattern option
        | TimePointPrototypeMsg of id: Kind * TimePointPrototypeModel.Msg
        | TimePointMsg of id: Guid * TimePointModel.Msg
        | ApplyTimePoints
        | RequestCancelling
        | OnExn of exn
    and
        TimePointMsg =
            | SetName of string

    [<RequireQualifiedAccess>]
    [<Struct>]
    type Intent =
        | None
        | ApplyGeneratedTimePoints
        | CancelTimePointGeneration

    [<AutoOpen>]
    module Helpers =
        let inline withNoIntent (model, cmd) = (model, cmd, Intent.None)

    open Elmish
    open Elmish.Extensions
    open PomodoroWindowsTimer.ElmishApp.Infrastructure

    let init (timePointPrototypeStore: TimePointPrototypeStore) (patternStore: PatternStore) =
        let (patterns, cmd) =
            match patternStore.Read () with
            | [] -> ([], Cmd.none)
            | l -> (l, Cmd.ofMsg (Msg.SetSelectedPattern (l |> List.head |> Some)))

        let model =
            {
                TimePointPrototypes = timePointPrototypeStore.Read () |> List.map TimePointPrototypeModel.init
                Patterns = patterns
                SelectedPattern = None
                SelectedPatternIndex = 0
                TimePoints = []
                IsPatternWrong = false
                TimePointsTime = ValueNone
            }
        model, cmd

    let withTimePointPrototypes prototypes (model: TimePointsGeneratorModel) =
        { model with TimePointPrototypes = prototypes }

    let withTimePoints timePoints (model: TimePointsGeneratorModel) =
        { model with TimePoints = timePoints }

    let mapPrototype kind f =
        map _.TimePointPrototypes withTimePointPrototypes (mapFirst (_.Prototype >> _.Kind >> (=) kind) f)

    let mapTimePoint id f =
        map _.TimePoints withTimePoints (mapFirst (_.TimePoint >> _.Id >> (=) id) f)

    // -------
    // helpers
    // -------
    let getPatterns m = m.Patterns

    let getSelectedPatternIndex m = m.SelectedPatternIndex

    let setSelectedPattern pattern m =
        { m with SelectedPattern = pattern |> Some }

    let setSelectedPatternIndex ind m =
        { m with SelectedPatternIndex = ind }

    let unsetSelectedPattern m =
        { m with SelectedPattern = None }

    let setTimePoints tpx m =
        { m with TimePoints = tpx }

    let clearTimePoints m =
        { m with TimePoints = []; TimePointsTime = ValueNone }

    let setIsPatternWrong m =
        { m with IsPatternWrong = true }

    let unsetIsPatternWrong m =
        { m with IsPatternWrong = false }

    let setTimes times (m: TimePointsGeneratorModel) =
        { m with TimePointsTime = times |> ValueSome }
