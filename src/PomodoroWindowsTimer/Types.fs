﻿namespace PomodoroWindowsTimer.Types

open System

type [<Measure>] ms
type [<Measure>] sec

type Name = string

[<Struct>]
type DateOnlyPeriod =
    {
        Start: DateOnly
        EndInclusive: DateOnly
    }
    static member Zero =
        {
            Start = DateOnly()
            EndInclusive = DateOnly()
        }

[<Struct>]
type DateTimePeriod =
    {
        Start: DateTime
        EndInclusive: DateTime
    }
    static member Zero =
        {
            Start = DateTime()
            EndInclusive = DateTime()
        }

type Kind =
    | Work
    | Break
    | LongBreak

/// Kind alias, used in patterns.
type Alias = private Alias of string


type TimePoint =
    {
        Id: Guid
        Name: Name
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

type Work =
    {
        Id: uint64
        Number: string
        Title: string
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
    }

type WorkEvent =
    | WorkStarted of createdAt: DateTimeOffset * timePointName: string
    | BreakStarted of createdAt: DateTimeOffset * timePointName: string
    | Stopped of createdAt: DateTimeOffset
    | WorkReduced of createdAt: DateTimeOffset * value: TimeSpan
    | WorkIncreased of createdAt: DateTimeOffset * value: TimeSpan
    | BreakReduced of createdAt: DateTimeOffset * value: TimeSpan
    | BreakIncreased of createdAt: DateTimeOffset * value: TimeSpan

type WorkEventList =
    {
        Work: Work
        Events: WorkEvent list
    }

type Statistic =
    {
        Period: DateTimePeriod
        WorkTime: TimeSpan
        BreakTime: TimeSpan
        TimePointNameStack: string list
    }

type WorkStatistic =
    {
        Work: Work
        Statistic: Statistic option
    }

// ------------------------------- modules

module WorkEvent =

    let createdAt = function
        | WorkEvent.WorkStarted (dt, _)
        | WorkEvent.BreakStarted (dt, _)
        | WorkEvent.Stopped (dt)
        | WorkEvent.WorkReduced (dt,_)
        | WorkEvent.WorkIncreased (dt,_) 
        | WorkEvent.BreakReduced (dt,_)
        | WorkEvent.BreakIncreased (dt,_) -> dt

    let dateOnly = function
        | WorkEvent.WorkStarted (dt, _)
        | WorkEvent.BreakStarted (dt, _)
        | WorkEvent.Stopped (dt) 
        | WorkEvent.WorkReduced (dt,_)
        | WorkEvent.WorkIncreased (dt,_) 
        | WorkEvent.BreakReduced (dt,_)
        | WorkEvent.BreakIncreased (dt,_) ->
            DateOnly.FromDateTime(dt.DateTime)

    let localDateTime = function
        | WorkEvent.WorkStarted (dt, _)
        | WorkEvent.BreakStarted (dt, _)
        | WorkEvent.Stopped (dt)
        | WorkEvent.WorkReduced (dt,_)
        | WorkEvent.WorkIncreased (dt,_) 
        | WorkEvent.BreakReduced (dt,_)
        | WorkEvent.BreakIncreased (dt,_) ->
            dt.LocalDateTime

    let tpName = function
        | WorkEvent.WorkStarted (_, n)
        | WorkEvent.BreakStarted (_, n) -> n |> Some
        | WorkEvent.Stopped _
        | WorkEvent.WorkReduced _
        | WorkEvent.WorkIncreased _ 
        | WorkEvent.BreakReduced _
        | WorkEvent.BreakIncreased _ -> None


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


type LooperEvent =
    | TimePointStarted of ``new``: TimePoint * old: TimePoint option 
    | TimePointTimeReduced of TimePoint


module TimePointPrototype =

    let defaults =
        [
            { Name = "Focused work"; Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(25) }
            { Name = "Break"; Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(5) }
            { Name = "Long break"; Kind = Kind.LongBreak; KindAlias = Kind.LongBreak |> Kind.alias; TimeSpan = TimeSpan.FromMinutes(20) }
        ]

    let toTimePoint ind prototype =
        {
            Id = Guid.NewGuid()
            Name = sprintf "%s %i" prototype.Name ind
            TimeSpan = prototype.TimeSpan
            Kind = prototype.Kind
            KindAlias = prototype.KindAlias
        }


module TimePoint =

    let defaults =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 3"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 3"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 4"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Long Break"; TimeSpan = TimeSpan.FromMinutes(20); Kind = Kind.LongBreak; KindAlias = Kind.LongBreak |> Kind.alias }
        ]

    let testDefaults =
        [
            { Id = Guid.NewGuid(); Name = "Focused Work 1"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 1"; TimeSpan = TimeSpan.FromSeconds(4); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Focused Work 2"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Kind.Work; KindAlias = Kind.Work |> Kind.alias }
            { Id = Guid.NewGuid(); Name = "Break 2"; TimeSpan = TimeSpan.FromSeconds(4); Kind = Kind.Break; KindAlias = Kind.Break |> Kind.alias }
        ]



module Pattern =

    let defaults =
        [
            "(w-b)3-w-lb"
        ]