namespace CycleBell.Types

open System

type Name = string

type TimePoint =
    {
        Name: Name
        TimeSpan: TimeSpan
    }