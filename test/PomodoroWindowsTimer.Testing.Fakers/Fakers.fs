[<AutoOpen>]
module PomodoroWindowsTimer.Testing.Fakers

open Bogus
open PomodoroWindowsTimer.Types
open System

let faker = Faker()

let generateNumber () : string option =
    let numFactory =
        [|
            fun () -> None
            fun () ->
                sprintf "WORK-%s" (faker.Random.Int(1, 9999).ToString("0000"))
                |> Some
        |]
    (faker.Random.ArrayElement(numFactory)) ()

let generateTitle () : string =
    faker.Commerce.ProductName()

let mutable private _workTPCounter = 1
let mutable private _breakTPCounter = 1
let mutable private _longBreakTPCounter = 1

let generateTimePointName () : string =
    let nameFactory =
        [|
            fun () ->
                let name = sprintf "Work %i" _workTPCounter
                _workTPCounter <- _workTPCounter + 1
                name
            fun () ->
                let name = sprintf "Break %i" _breakTPCounter
                _breakTPCounter <- _breakTPCounter + 1
                name
            fun () ->
                let name = sprintf "Long Break %i" _longBreakTPCounter
                _longBreakTPCounter <- _longBreakTPCounter + 1
                name
        |]
    (faker.Random.ArrayElement(nameFactory)) ()

// --------------------------------------

let generateWorkTimePointPrototype () =
    let tpPrototype =
        {
            Name = sprintf "Work %i" _workTPCounter
            Kind = Kind.Work
            KindAlias = "w" |> Alias.createOrThrow
            TimeSpan = TimeSpan.FromMinutes(25)
        }
    _workTPCounter <- _workTPCounter + 1
    tpPrototype

let generateBreakTimePointPrototype () =
    let tpPrototype =
        {
            Name = sprintf "Break %i" _breakTPCounter
            Kind = Kind.Break
            KindAlias = "b" |> Alias.createOrThrow
            TimeSpan = TimeSpan.FromMinutes(5)
        }
    _breakTPCounter <- _breakTPCounter + 1
    tpPrototype

let generateLongBreakTimePointPrototype () =
    let tpPrototype =
        {
            Name = sprintf "Long Break %i" _longBreakTPCounter
            Kind = Kind.LongBreak
            KindAlias = "lb" |> Alias.createOrThrow
            TimeSpan = TimeSpan.FromMinutes(20)
        }
    _longBreakTPCounter <- _longBreakTPCounter + 1
    tpPrototype

let generateTimePointPrototype () =
    let nameFactory =
        [|
            generateWorkTimePointPrototype
            generateBreakTimePointPrototype
            generateLongBreakTimePointPrototype
        |]
    (faker.Random.ArrayElement(nameFactory)) ()

// ---------------------------


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

// ---------------------------

let mutable private _workCounter = 1UL

let generateWork () =
    let date = faker.Date.RecentOffset()
    let work =
        {
            Id = _workCounter
            Number = sprintf "WORK-%i" _workCounter |> Some
            Title = faker.Commerce.ProductName()
            CreatedAt = date
            UpdatedAt = date
        }
    _workCounter <- _workCounter + 1UL
    work

// ---------------------------

let generateWorkStartedEvent () =
    WorkEvent.WorkStarted
        (faker.Date.RecentOffset(7), generateTimePointName ())

let generateBreakStartedEvent () =
    WorkEvent.BreakStarted
        (faker.Date.RecentOffset(7), generateTimePointName ())

let generateStoppedEvent () =
    WorkEvent.Stopped
        (faker.Date.RecentOffset(7))

let generateWorkEvent () : WorkEvent =
    let eventFactory =
        [|
            fun () -> generateWorkStartedEvent ()
            fun () -> generateBreakStartedEvent ()
            fun () -> generateStoppedEvent ()
        |]

    (faker.Random.ArrayElement(eventFactory)) ()