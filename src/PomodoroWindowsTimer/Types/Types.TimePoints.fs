namespace PomodoroWindowsTimer.Types

open System

/// Kind alias, used in patterns.
type Alias = private Alias of string

type TimePointId = Guid

type Kind =
    | Work
    | Break
    | LongBreak

type TimePoint =
    {
        Id: TimePointId
        Num: int
        Name: Name
        TimeSpan: TimeSpan
        Kind: Kind
        KindAlias: Alias
        // TODO: AutoProlongation : bool
    }

type ActiveTimePoint =
    {
        Id: TimePointId
        OriginalId: TimePointId
        Name: Name
        RemainingTimeSpan: TimeSpan
        TimeSpan: TimeSpan
        Kind: Kind
        KindAlias: Alias
    }

/// <summary>
/// Could be set in settings for App.
///
/// For example: <br/>
///
/// <code>
/// | Name   | Alias | TimeSpan | Kind  | <br/>
/// | ------ | ----- | -------- | ----- | <br/>
/// | Coffee |   c   |      :15 | Break |
/// </code>
/// </summary>
type TimePointPrototype =
    {
        Name: string
        Kind: Kind
        Alias: Alias
        TimeSpan: TimeSpan
        // TODO: AutoProlongation : bool
    }

type Pattern = string

[<RequireQualifiedAccess>]
type PatternParsedItem =
    | Alias of Alias
    | AliasTimeSpan of Alias * TimeSpan
    | AliasTimeSpanName of Alias * TimeSpan * Name

[<Struct>]
type TimePointsTime =
    {
        WorkTime: TimeSpan
        BreakTime: TimeSpan
    }

// ------------------------------- modules

module Alias =

    let create str =
        let maxLen = 10
        if String.IsNullOrEmpty(str) then
            Error "Alias must not be null or empty"
        elif str.Length > maxLen then
            let msg = sprintf "Alias must not be more than %i chars" maxLen 
            Error msg
        elif str |> Seq.exists (fun ch -> (not <| Char.IsLetter(ch)) || (not <| Char.IsLower(ch))) then
            Error "Alias must contain only lower letters"
        else
            Ok (Alias str)

    let orThrow = function
        | Ok alias -> alias
        | Error err -> failwith err

    let createOrThrow str =
        str |> create |> orThrow

    let value (Alias v) = v

    module Defaults =

        let work = "w" |> Alias
        let ``break`` = "b" |> Alias
        let longBreak = "lb" |> Alias

        let all = [ work; ``break``; longBreak ]

module Kind =
    let displayString = function
        | Work -> "WORK"
        | Break -> "BREAK"
        | LongBreak -> "LONG BREAK"

    let alias = 
        (function
            | Work -> Alias.Defaults.work
            | Break -> Alias.Defaults.``break``
            | LongBreak -> Alias.Defaults.longBreak)

    [<CompiledName("ToShortString")>]
    let toShortString = function
        | Work -> "W"
        | Break -> "BR"
        | LongBreak -> "LB"

    [<CompiledName("IsBreak")>]
    let isBreak = function
        | Work -> false
        | Break
        | LongBreak -> true

    [<CompiledName("IsWork")>]
    let isWork = isBreak >> not


module TimePointPrototype =

    let defaults =
        [
            { Name = "Focused work"; Kind = Kind.Work; Alias = Kind.Work |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(25L) }
            { Name = "Break"; Kind = Kind.Break; Alias = Kind.Break |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(5L) }
            { Name = "Long break"; Kind = Kind.LongBreak; Alias = Kind.LongBreak |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(20L) }
        ]

    let toTimePoint num nameInd (prototype: TimePointPrototype) =
        {
            Id = Guid.NewGuid()
            Num = num
            Name = sprintf "%s %i" prototype.Name nameInd
            TimeSpan = prototype.TimeSpan
            Kind = prototype.Kind
            KindAlias = prototype.Alias
        }


module TimePoint =

    module Name =

        let [<Literal>] WORK = "Focused Work"
        let [<Literal>] BREAK = "Break"
        let [<Literal>] LONG_BREAK = "Long Break"

        let ofKind (kind: Kind) =
            match kind with
            | Kind.Work -> WORK
            | Kind.Break -> BREAK
            | Kind.LongBreak -> LONG_BREAK

    let create num name timeSpan kind =
        {
            Id = Guid.NewGuid()
            Num = num
            Name = name
            TimeSpan = timeSpan
            Kind = kind
            KindAlias = kind |> Kind.alias
        }

    [<CompiledName("Defaults")>]
    let defaults =
        [
            { Id = Guid.NewGuid(); Num = 1; Name = "Focused Work 1"; TimeSpan = TimeSpan.FromMinutes(25L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 2; Name = "Break 1"; TimeSpan = TimeSpan.FromMinutes(5L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 3; Name = "Focused Work 2"; TimeSpan = TimeSpan.FromMinutes(25L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 4; Name = "Break 2"; TimeSpan = TimeSpan.FromMinutes(5L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 5; Name = "Focused Work 3"; TimeSpan = TimeSpan.FromMinutes(25L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 6; Name = "Break 3"; TimeSpan = TimeSpan.FromMinutes(5L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 7; Name = "Focused Work 4"; TimeSpan = TimeSpan.FromMinutes(25L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 8; Name = "Long Break"; TimeSpan = TimeSpan.FromMinutes(20L); Kind = Kind.LongBreak; KindAlias = Kind.LongBreak |> Kind.alias }
        ]

    let testDefaults =
        [
            { Id = Guid.NewGuid(); Num = 1; Name = "Focused Work 1"; TimeSpan = TimeSpan.FromSeconds(5L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 2; Name = "Break 1"; TimeSpan = TimeSpan.FromSeconds(4L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 3; Name = "Focused Work 2"; TimeSpan = TimeSpan.FromSeconds(5L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Num = 4; Name = "Break 2"; TimeSpan = TimeSpan.FromSeconds(4L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
        ]

    [<CompiledName("ToActiveTimePointWith")>]
    let toActiveTimePointWith (runningTimeSpan: TimeSpan) (timePoint: TimePoint) =
        {
            Id = Guid.NewGuid()
            OriginalId = timePoint.Id
            Name = timePoint.Name
            RemainingTimeSpan = runningTimeSpan
            TimeSpan = timePoint.TimeSpan
            Kind = timePoint.Kind
            KindAlias = timePoint.KindAlias
        }

    let toActiveTimePointWithSec (runningSeconds: float<sec>) (timePoint: TimePoint) =
        toActiveTimePointWith (TimeSpan.FromSeconds(float runningSeconds)) timePoint

    [<CompiledName("ToActiveTimePoint")>]
    let toActiveTimePoint (timePoint: TimePoint) =
        toActiveTimePointWith timePoint.TimeSpan timePoint


module Pattern =

    let defaults =
        [
            "(w-b)3-w-lb"
        ]

module PatternParsedItem =

    let ofAliasOrThrow = Alias.createOrThrow >> PatternParsedItem.Alias

    let ofAliasTimeSpanOrThrow (aliasStr: string) (ts: TimeSpan) =
        let alias = aliasStr |> Alias.createOrThrow
        PatternParsedItem.AliasTimeSpan (alias, ts)

    let ofAliasTimeSpanNameOrThrow (aliasStr: string) (ts: TimeSpan) (name: string) =
        let alias = aliasStr |> Alias.createOrThrow
        PatternParsedItem.AliasTimeSpanName (alias, ts, name)

    module List =

        let timePoints (prototypeList: TimePointPrototype list) (patternParsedItems: PatternParsedItem list) =
            let prototypeMap =
                prototypeList
                |> List.map (fun p -> (p.Alias, p))
                |> Map.ofList

            let nameMap =
                prototypeList
                |> List.map (fun p -> (p.Name, 0))
                |> Map.ofList

            let addTime (tp: TimePoint) (times: TimePointsTime) =
                match tp.Kind with
                | Kind.Work -> { times with WorkTime = times.WorkTime + tp.TimeSpan }
                | Kind.Break | Kind.LongBreak -> { times with BreakTime = times.BreakTime + tp.TimeSpan }

            patternParsedItems
            |> List.fold (fun (s: {| Ind: int; NameMap: Map<string, int>; Times: TimePointsTime; Result: TimePoint list |}) item ->
                let idx = s.Ind + 1
                match item with
                | PatternParsedItem.Alias alias ->
                    let prototype = prototypeMap[alias]
                    let nameInd = s.NameMap[prototype.Name] + 1
                    let tp = prototype |> TimePointPrototype.toTimePoint idx nameInd
                    {| s with
                        Ind = idx + 1
                        NameMap = s.NameMap |> Map.add prototype.Name nameInd
                        Result = tp :: s.Result
                        Times = s.Times |> addTime tp
                    |}

                | PatternParsedItem.AliasTimeSpan (alias, ts) ->
                    let prototype = prototypeMap[alias]
                    let nameInd = s.NameMap[prototype.Name] + 1
                    let tp = 
                        prototype |> TimePointPrototype.toTimePoint idx nameInd
                        |> fun t -> { t with TimeSpan = ts }
                    {| s with
                        Ind = idx + 1
                        NameMap = s.NameMap |> Map.add prototype.Name nameInd
                        Result = tp :: s.Result
                        Times = s.Times |> addTime tp
                    |}

                | PatternParsedItem.AliasTimeSpanName (alias, ts, name) ->
                    let prototype = prototypeMap[alias]
                    let nameInd = s.NameMap[prototype.Name] + 1
                    let tp =
                        prototype |> TimePointPrototype.toTimePoint idx nameInd
                        |> fun t -> { t with TimeSpan = ts; Name = sprintf "%s %i" name (idx) }
                    {| s with
                        Ind = idx + 1
                        NameMap = s.NameMap |> Map.add prototype.Name nameInd
                        Result = tp :: s.Result
                        Times = s.Times |> addTime tp
                    |}

            ) {| Ind = 0; NameMap = nameMap; Times = { WorkTime = TimeSpan.Zero; BreakTime = TimeSpan.Zero }; Result = [] |}
            |> fun s ->
                (s.Result |> List.rev, s.Times)

