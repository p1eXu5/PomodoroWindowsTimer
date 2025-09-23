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
        Pattern: string option
        SelectedPatternIndex: int
        IsPatternWrong: bool
        TimePointsTime: TimePointsTime voption
    }


module TimePointsGeneratorModel =

    type Msg =
        | SetPatterns of string list
        | SetPrototypes of TimePointPrototypeModel list
        | SetGeneratedTimePoints of Result<(TimePointModel list * TimePointsTime * TimePointPrototypeModel list * Pattern), (string * TimePointPrototypeModel list * Pattern)>
        | SetSelectedPatternIndex of int
        | SetPattern of Pattern option
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
        let prototypeCmd =
            Cmd.OfFunc.either
                (fun () ->
                    timePointPrototypeStore.Read ()
                    |> List.map TimePointPrototypeModel.init
                )
                ()
                Msg.SetPrototypes
                Msg.OnExn

        let patternCmd =
            Cmd.OfFunc.either
                (fun () -> patternStore.Read ())
                ()
                Msg.SetPatterns
                Msg.OnExn

        let model =
            {
                TimePointPrototypes = []
                Patterns = []
                Pattern = None
                SelectedPatternIndex = 0
                TimePoints = []
                IsPatternWrong = false
                TimePointsTime = ValueNone
            }
        model
        , Cmd.batch [ prototypeCmd; patternCmd ]

    // -------
    // accessors
    // -------
    let withTimePointPrototypes prototypes (model: TimePointsGeneratorModel) =
        { model with TimePointPrototypes = prototypes }

    let withTimePoints timePoints (model: TimePointsGeneratorModel) =
        { model with TimePoints = timePoints }

    let mapPrototype kind f =
        map _.TimePointPrototypes withTimePointPrototypes (mapFirst (_.Prototype >> _.Kind >> (=) kind) f)

    let mapTimePoint id f =
        map _.TimePoints withTimePoints (mapFirst (_.TimePoint >> _.Id >> (=) id) f)

    let getPatterns m = m.Patterns

    let getSelectedPatternIndex m = m.SelectedPatternIndex

    let setSelectedPattern pattern m =
        { m with Pattern = pattern |> Some }

    let setSelectedPatternIndex ind m =
        { m with SelectedPatternIndex = ind }

    let unsetSelectedPattern m =
        { m with Pattern = None }

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

    let withPrototypes prototypes (m: TimePointsGeneratorModel) =
        match prototypes with
        | [] ->
            {
                m with
                    TimePointPrototypes = prototypes
                    TimePoints = []
                    TimePointsTime = ValueNone
            }
        | l -> { m with TimePointPrototypes = l }
