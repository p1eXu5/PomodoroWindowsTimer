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

    let toTimePoint ind (prototype: TimePointPrototype) =
        {
            Id = Guid.NewGuid()
            Name = sprintf "%s %i" prototype.Name ind
            TimeSpan = prototype.TimeSpan
            Kind = prototype.Kind
            KindAlias = prototype.Alias
        }


module TimePoint =

    [<CompiledName("Defaults")>]
    let defaults =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromMinutes(25L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromMinutes(5L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromMinutes(25L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromMinutes(5L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 3"; TimeSpan = TimeSpan.FromMinutes(25L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 3"; TimeSpan = TimeSpan.FromMinutes(5L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 4"; TimeSpan = TimeSpan.FromMinutes(25L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Long Break"; TimeSpan = TimeSpan.FromMinutes(20L); Kind = Kind.LongBreak; KindAlias = Kind.LongBreak |> Kind.alias }
        ]

    let testDefaults =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromSeconds(5L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromSeconds(4L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromSeconds(5L); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromSeconds(4L); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
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