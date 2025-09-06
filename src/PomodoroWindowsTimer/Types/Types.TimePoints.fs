namespace PomodoroWindowsTimer.Types

open System

// TODO: rename to abbreviation
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

type TimePointPrototype =
    {
        Name: string
        Kind: Kind
        KindAlias: Alias
        TimeSpan: TimeSpan
    }

type Pattern = string

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


module Kind =
    let displayString = function
        | Work -> "WORK"
        | Break -> "BREAK"
        | LongBreak -> "LONG BREAK"

    let alias = 
        (function
            | Work -> "w"
            | Break -> "b"
            | LongBreak -> "lb")
        >> Alias.createOrThrow

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
            { Name = "Focused work"; Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(25L) }
            { Name = "Break"; Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(5L) }
            { Name = "Long break"; Kind = Kind.LongBreak; KindAlias = Kind.LongBreak |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(20L) }
        ]

    let toTimePoint ind (prototype: TimePointPrototype) =
        {
            Id = Guid.NewGuid()
            Name = sprintf "%s %i" prototype.Name ind
            TimeSpan = prototype.TimeSpan
            Kind = prototype.Kind
            KindAlias = prototype.KindAlias
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