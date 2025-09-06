namespace PomodoroWindowsTimer.Types

open System

type [<Measure>] ms
type [<Measure>] sec
type [<Measure>] min

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

// ------------------------------- modules

module DateOnlyPeriod =

    let create start endInclusive : DateOnlyPeriod =
        {
            Start = start
            EndInclusive = endInclusive
        }

    let isOneDay (period: DateOnlyPeriod) =
        period.Start = period.EndInclusive