namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types
open System
open PomodoroWindowsTimer.ElmishApp.Abstractions

type TimePointsSettingsModel =
    {
        TimePointPrototypes: TimePointPrototype list
        Patterns: string list
        SelectedPattern: string option
        SelectedPatternIndex: int
        TimePoints: TimePoint list
        IsPatternWrong: bool
    }


module TimePointsSettingsModel =

    type Msg =
        | SetPatterns of string list
        | SetSelectedPatternIndex of int
        | SetSelectedPattern of string option
        | ProcessParseResult of Result<string list, string>
        | TimePointPrototypeMsg of id: Kind * TimePointPrototypeMsg
        | TimePointMsg of id: Guid * TimePointMsg
    and
        TimePointPrototypeMsg =
            | SetTimeSpan of string
    and
        TimePointMsg =
            | SetName of string

    open Elmish
    open PomodoroWindowsTimer.ElmishApp.Infrastructure

    let [<Literal>] DEFAULT_PATTERN = "(w-b)3-w-lb"

    let init (timePointPrototypeStore: TimePointPrototypeStore) (patternSettings: IPatternSettings) =
        let (patterns, cmd) =
            match patternSettings.Read () with
            | [] -> [ DEFAULT_PATTERN ], Cmd.ofMsg (Msg.SetSelectedPattern (DEFAULT_PATTERN |> Some))
            | head :: tail -> (head :: tail), Cmd.ofMsg (Msg.SetSelectedPattern (head |> Some))
        {
            TimePointPrototypes = timePointPrototypeStore.Read ()
            Patterns = patterns
            SelectedPattern = None
            SelectedPatternIndex = 0
            TimePoints = []
            IsPatternWrong = false
        }
        , cmd

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