namespace CycleBell.Types

open System

type Name = string

type Kind =
    | Work
    | Break

type TimePoint =
    {
        Name: Name
        TimeSpan: TimeSpan
        Kind: Kind
    }