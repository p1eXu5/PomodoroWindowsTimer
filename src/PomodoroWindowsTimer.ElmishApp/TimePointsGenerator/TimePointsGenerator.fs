namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types
open System
open PomodoroWindowsTimer.ElmishApp.Abstractions

type TimePointsGenerator =
    {
        TimePointPrototypes: TimePointPrototype list
        Patterns: string list
        SelectedPattern: string option
        SelectedPatternIndex: int
        TimePoints: TimePoint list
        IsPatternWrong: bool
    }


module TimePointsGenerator =

    type Msg =
        | SetPatterns of string list
        | SetSelectedPatternIndex of int
        | SetSelectedPattern of Pattern option
        | ProcessParseResult of Result<string list, string>
        | TimePointPrototypeMsg of id: Kind * TimePointPrototypeMsg
        | TimePointMsg of id: Guid * TimePointMsg
        | ApplyTimePoints
    and
        TimePointPrototypeMsg =
            | SetName of string
            | SetTimeSpan of string
    and
        TimePointMsg =
            | SetName of string

    type Request =
        | ApplyGeneratedTimePoints


    open Elmish
    open PomodoroWindowsTimer.ElmishApp.Infrastructure

    let init (timePointPrototypeStore: TimePointPrototypeStore) (patternStore: PatternStore) =
        let (patterns, cmd) =
            match patternStore.Read () with
            | [] -> ([], Cmd.none)
            | l -> (l, Cmd.ofMsg (Msg.SetSelectedPattern (l |> List.head |> Some)))

        let model =
            {
                TimePointPrototypes = timePointPrototypeStore.Read ()
                Patterns = patterns
                SelectedPattern = None
                SelectedPatternIndex = 0
                TimePoints = []
                IsPatternWrong = false
            }
        model, cmd

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
        { m with TimePoints = [] }

    let setIsPatternWrong m =
        { m with IsPatternWrong = true }

    let unsetIsPatternWrong m =
        { m with IsPatternWrong = false }

    let applyMsg (m: TimePointsGenerator) =
        if not m.IsPatternWrong then
            match m.TimePoints with
            | [] -> None
            | _ -> Some Msg.ApplyTimePoints
        else None