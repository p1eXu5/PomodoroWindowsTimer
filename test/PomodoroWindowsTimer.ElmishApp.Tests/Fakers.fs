module PomodoroWindowsTimer.ElmishApp.Tests.Fakers

open System
open Bogus
open PomodoroWindowsTimer.Types


let ``0.5 sec`` = 0.5<sec>
let ``3 sec`` = 3.0<sec>

let mutable private num = 1

let timePointFaker namePrefix =
    let kind = faker.Random.ArrayElement([| Kind.Work; Kind.Break; Kind.LongBreak |])
    let id = Guid.Parse($"00000000-0000-0000-0000-0000000000" + num.ToString("00"))
    num <- num + 1
    {
        Id = id
        Name = (namePrefix, faker.Commerce.ProductName()) ||> sprintf "%s. %s"
        TimeSpan = faker.Date.Timespan()
        Kind = kind
        KindAlias = kind |> Kind.alias
    }

let workTP (timeSpan: float<sec>) =
    {
        timePointFaker "Work"
            with
                TimeSpan = TimeSpan.FromSeconds(float timeSpan)
                Kind = Kind.Work
    }

let namedWorkTP name (timeSpan: float<sec>) =
    {
        timePointFaker name
            with
                TimeSpan = TimeSpan.FromSeconds(float timeSpan)
                Kind = Kind.Work
    }

let breakTP (timeSpan: float<sec>) =
    {
        timePointFaker "Break"
            with
                TimeSpan = TimeSpan.FromSeconds(float timeSpan)
                Kind = Kind.Break
    }

let longBreakTP (timeSpan: float<sec>) =
    {
        timePointFaker "LongBreak"
            with
                TimeSpan = TimeSpan.FromSeconds(float timeSpan)
                Kind = Kind.LongBreak
    }

