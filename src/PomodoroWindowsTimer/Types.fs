namespace PomodoroWindowsTimer.Types

open System

type Name = string

type Kind =
    | Work
    | Break
    | LongBreak

type TimePoint =
    {
        Id: Guid
        Name: Name
        TimeSpan: TimeSpan
        Kind: Kind
    }

/// Kind alias, used in patterns.
type Alias = private Alias of string


type TimePointPrototype =
    {
        Kind: Kind
        Alias: Alias
        TimeSpan: TimeSpan
    }


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

    let value (Alias v) = v


module TimePointPrototype =

    let orThrow = function
        | Ok alias -> alias
        | Error err -> failwith err

    let defaults =
        [
            { Kind = Kind.Work; Alias = "w" |> Alias.create |> orThrow; TimeSpan = TimeSpan.FromMinutes(25) }
            { Kind = Kind.Break; Alias = "b" |> Alias.create |> orThrow; TimeSpan = TimeSpan.FromMinutes(5) }
            { Kind = Kind.LongBreak; Alias = "lb" |> Alias.create |> orThrow; TimeSpan = TimeSpan.FromMinutes(20) }
        ]

    let toTimePoint ind prototype =
        {
            Id = Guid.NewGuid()
            Name = sprintf "%O %i" prototype.Kind ind
            TimeSpan = prototype.TimeSpan
            Kind = prototype.Kind
        }


module TimePoint =

    let defaults =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Break }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Break }
            { Id = Guid.NewGuid(); Name = "Focused Work 3"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 3"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Break }
            { Id = Guid.NewGuid(); Name = "Focused Work 4"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Long Break"; TimeSpan = TimeSpan.FromMinutes(20); Kind = Break }
        ]

    let testDefaults =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromSeconds(4); Kind = Break }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Work }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromSeconds(4); Kind = Break }
        ]