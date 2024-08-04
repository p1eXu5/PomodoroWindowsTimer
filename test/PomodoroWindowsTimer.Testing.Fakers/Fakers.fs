[<AutoOpen>]
module PomodoroWindowsTimer.Testing.Fakers

open Bogus
open PomodoroWindowsTimer.Types
open System

let faker = Faker()

module TimePointId =
    let generate () : TimePointId = faker.Random.Uuid()

module WorkId =
    let generate () : WorkId = faker.Random.ULong(1UL, 100UL)

let generateNumber () : string =
    sprintf "WORK-%s" (faker.Random.Int(1, 9999).ToString("0000"))


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


module Kind =
    let generate () : Kind =
        faker.Random.ArrayElement(
            [|
                fun () -> Kind.Work
                fun () -> Kind.Break
                fun () -> Kind.LongBreak
            |]
        )()


module TimePoint =
    let generateWith timeSpan : TimePoint =
        let kind = Kind.generate ()
        {
            Id = TimePointId.generate ()
            TimeSpan = timeSpan
            Name = generateTimePointName ()
            Kind = kind
            KindAlias = kind |> Kind.alias
        }

    let generate () =
        generateWith (TimeSpan.FromMinutes(faker.Random.Int(1, 100)))

    let gewerateLongBreakTP (timeSpan: float<sec>) =
        {
            timePointFaker "LongBreak"
                with
                    TimeSpan = TimeSpan.FromSeconds(float timeSpan)
                    Kind = Kind.LongBreak
        }

let longBreakTP (seconds: float<sec>) =
    TimePoint.gewerateLongBreakTP seconds

[<RequireQualifiedAccess>]
module Work =
    let mutable private _workCounter = 1UL

    let generate () =
        let date = faker.Date.RecentOffset()
        let work =
            {
                Id = _workCounter
                Number = sprintf "WORK-%i" _workCounter
                Title = faker.Commerce.ProductName()
                CreatedAt = date
                UpdatedAt = date
                LastEventCreatedAt = None
            }
        _workCounter <- _workCounter + 1UL
        work

[<RequireQualifiedAccess>]
module WorkEvent =
    let generateCreatedAt (date: DateOnly) (timeStr: string) =
        let time = TimeOnly.ParseExact(timeStr, "HH:mm", null)
        DateTimeOffset(date, time, System.TimeProvider.System.LocalTimeZone.BaseUtcOffset)

    let generateWorkStarted () =
        WorkEvent.WorkStarted
            (faker.Date.RecentOffset(7), generateTimePointName (), TimePointId.generate ())

    /// timeStr in "HH:mm" format.
    let generateWorkStartedWith (date: DateOnly) (timeStr: string) =
        WorkEvent.WorkStarted
            (generateCreatedAt date timeStr, generateTimePointName (), TimePointId.generate ())

    let generateBreakStarted () =
        WorkEvent.BreakStarted
            (faker.Date.RecentOffset(7), generateTimePointName (), TimePointId.generate ())

    /// timeStr in "HH:mm" format.
    let generateBreakStartedWith (date: DateOnly) (timeStr: string) =
        WorkEvent.BreakStarted
            (generateCreatedAt date timeStr, generateTimePointName (), TimePointId.generate ())

    let generateStopped () =
        WorkEvent.Stopped
            (faker.Date.RecentOffset(7))

    /// timeStr in "HH:mm" format.
    let generateStoppedWith (date: DateOnly) (timeStr: string) =
        WorkEvent.Stopped (generateCreatedAt date timeStr)

    let generateWorkIncreasedWith (date: DateOnly) (timeStr: string) =
        WorkEvent.WorkIncreased (generateCreatedAt date timeStr, TimeSpan.FromMinutes(faker.Random.Int(1, 25)))

    let generate () =
        let eventFactory =
            [|
                fun () -> generateWorkStarted ()
                fun () -> generateBreakStarted ()
                fun () -> generateStopped ()
            |]

        (faker.Random.ArrayElement(eventFactory)) ()

    let generateWith (activeTimePointId: TimePointId) =
        generate () |> WorkEvent.withActiveTimePointId activeTimePointId

    let trimMicroseconds (workEvent: WorkEvent) =
        let trim (createdAt: DateTimeOffset) =
            DateTimeOffset.FromUnixTimeMilliseconds(createdAt.ToUnixTimeMilliseconds())

        match workEvent with
        | WorkEvent.WorkStarted (createdAt, n, id) -> WorkEvent.WorkStarted (createdAt |> trim, n, id)
        | WorkEvent.BreakStarted (createdAt, n, id) -> WorkEvent.BreakStarted (createdAt |> trim, n, id)
        | WorkEvent.Stopped (createdAt) -> WorkEvent.Stopped (createdAt |> trim)
        | WorkEvent.WorkReduced (createdAt, v) -> WorkEvent.WorkReduced (createdAt |> trim, v)
        | WorkEvent.WorkIncreased (createdAt, v) -> WorkEvent.WorkIncreased (createdAt |> trim, v)
        | WorkEvent.BreakReduced (createdAt, v) -> WorkEvent.BreakReduced (createdAt |> trim, v)
        | WorkEvent.BreakIncreased (createdAt, v) -> WorkEvent.BreakIncreased (createdAt |> trim, v)

[<RequireQualifiedAccess>]
module ActiveTimePoint =
    let generate () =
        TimePoint.generate ()
        |> TimePoint.toActiveTimePoint

    let withNoRemainingTimeSpan (atp: ActiveTimePoint) =
        { atp with
            RemainingTimeSpan = TimeSpan.Zero
        }