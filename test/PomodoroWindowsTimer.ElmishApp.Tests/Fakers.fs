module PomodoroWindowsTimer.ElmishApp.Tests.Fakers

open System
open Bogus
open PomodoroWindowsTimer.Types


let ``0.5 sec`` = 0.5<sec>
let ``3 sec`` = 3.0<sec>

let timePointFaker namePrefix =
    let kind = faker.Random.ArrayElement([| Kind.Work; Kind.Break; Kind.LongBreak |])
    {
        Id = Guid.Parse($"00000000-0000-0000-0000-0000000000" + faker.Random.Int(0, 99).ToString("00"))
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

let breakTP (timeSpan: float<sec>) =
    {
        timePointFaker "Break"
            with
                TimeSpan = TimeSpan.FromSeconds(float timeSpan)
                Kind = Kind.Break
    }

