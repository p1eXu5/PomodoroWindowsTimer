module PomodoroWindowsTimer.ElmishApp.Tests.Fakers

open System
open Bogus
open PomodoroWindowsTimer.Types

let faker = Faker("en")

let ``0.5 sec`` = TimeSpan.FromMilliseconds(500)
let ``3 sec`` = TimeSpan.FromSeconds(3)

let timePointFaker namePrefix =
    let kind = faker.Random.ArrayElement([| Kind.Work; Kind.Break; Kind.LongBreak |])
    {
        Id = faker.Random.Uuid()
        Name = (namePrefix, faker.Commerce.ProductName()) ||> sprintf "%s. %s"
        TimeSpan = faker.Date.Timespan()
        Kind = kind
        KindAlias = kind |> Kind.alias
    }

let workTP timeSpan =
    {
        timePointFaker "Work"
            with
                TimeSpan = timeSpan
                Kind = Kind.Work
    }

let breakTP timeSpan =
    {
        timePointFaker "Break"
            with
                TimeSpan = timeSpan
                Kind = Kind.Break
    }

