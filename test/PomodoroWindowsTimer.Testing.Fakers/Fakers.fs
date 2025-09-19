[<AutoOpen>]
module PomodoroWindowsTimer.Testing.Fakers

open Bogus
open PomodoroWindowsTimer.Types
open System

let faker = Faker()

module TimePointId =
    let generate () : TimePointId = faker.Random.Uuid()
    let generateOption () : TimePointId option =
        faker.Random.ArrayElement([|
            fun () -> faker.Random.Uuid() |> Some
            fun () -> None
        |])()

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
            Alias = "w" |> Alias.createOrThrow
            TimeSpan = TimeSpan.FromMinutes(25L)
        }
    _workTPCounter <- _workTPCounter + 1
    tpPrototype

let generateBreakTimePointPrototype () =
    let tpPrototype =
        {
            Name = sprintf "Break %i" _breakTPCounter
            Kind = Kind.Break
            Alias = "b" |> Alias.createOrThrow
            TimeSpan = TimeSpan.FromMinutes(5L)
        }
    _breakTPCounter <- _breakTPCounter + 1
    tpPrototype

let generateLongBreakTimePointPrototype () =
    let tpPrototype =
        {
            Name = sprintf "Long Break %i" _longBreakTPCounter
            Kind = Kind.LongBreak
            Alias = "lb" |> Alias.createOrThrow
            TimeSpan = TimeSpan.FromMinutes(20L)
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

let timePointFaker namePrefix =
    let kind = faker.Random.ArrayElement([| Kind.Work; Kind.Break; Kind.LongBreak |])
    let id = faker.Random.Guid()
    {
        Id = id
        Num = faker.Random.Int(1, 100)
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
                KindAlias = Kind.Work |> Kind.alias
    }

let namedWorkTP name (timeSpan: float<sec>) =
    {
        timePointFaker name
            with
                TimeSpan = TimeSpan.FromSeconds(float timeSpan)
                Kind = Kind.Work
                KindAlias = Kind.Work |> Kind.alias
    }

let breakTP (timeSpan: float<sec>) =
    {
        timePointFaker "Break"
            with
                TimeSpan = TimeSpan.FromSeconds(float timeSpan)
                Kind = Kind.Break
                KindAlias = Kind.Break |> Kind.alias
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
            Num = faker.Random.Int(1, 100)
            Name = generateTimePointName ()
            Kind = kind
            KindAlias = kind |> Kind.alias
        }

    let generate () =
        generateWith (TimeSpan.FromMinutes(faker.Random.Long(1, 100)))

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

    type private Msg =
        | GenerateWorkId of AsyncReplyChannel<WorkId>

    let private agent = MailboxProcessor<Msg>.Start(fun inbox ->
        let rec loop prevWorkId =
            async {
                let! msg = inbox.Receive()
                match msg with
                | GenerateWorkId replyChannel ->
                    replyChannel.Reply(prevWorkId + 1UL)
                    return! loop (prevWorkId + 1UL)
            }

        loop 0UL
    )

    let generate () =
        let workId = agent.PostAndReply(GenerateWorkId)
        let date = faker.Date.RecentOffset()
        {
            Id = workId
            Number = sprintf "WORK-%i" workId
            Title = faker.Commerce.ProductName()
            CreatedAt = date
            UpdatedAt = date
            LastEventCreatedAt = None
        }

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

    let generateWorkIncreased () =
        WorkEvent.WorkIncreased (faker.Date.RecentOffset(7), TimeSpan.FromMinutes(faker.Random.Long(1, 25)), TimePointId.generateOption ())

    let generateWorkIncreasedWith (date: DateOnly) (timeStr: string) =
        WorkEvent.WorkIncreased (generateCreatedAt date timeStr, TimeSpan.FromMinutes(faker.Random.Long(1, 25)), None)

    let createWorkIncreasedWithNoTimePoint (date: DateOnly) (timeStr: string) (time: TimeSpan) =
        WorkEvent.WorkIncreased (generateCreatedAt date timeStr, time, None)

    let createWorkIncreasedWith (date: DateOnly) (timeStr: string) (time: TimeSpan) (timePointId: TimePointId option) =
        WorkEvent.WorkIncreased (generateCreatedAt date timeStr, time, timePointId)

    let generateWorkReduced () =
        WorkEvent.WorkReduced (faker.Date.RecentOffset(7), TimeSpan.FromMinutes(faker.Random.Long(1, 25)), TimePointId.generateOption ())

    let createWorkReducedWith (date: DateOnly) (timeStr: string) (time: TimeSpan) (timePointId: TimePointId option) =
        WorkEvent.WorkReduced (generateCreatedAt date timeStr, time, timePointId)

    let generateBreakIncreased () =
        WorkEvent.BreakIncreased (faker.Date.RecentOffset(7), TimeSpan.FromMinutes(faker.Random.Long(1, 25)), TimePointId.generateOption ())

    let generateBreakReduced () =
        WorkEvent.BreakReduced (faker.Date.RecentOffset(7), TimeSpan.FromMinutes(faker.Random.Long(1, 25)), TimePointId.generateOption ())

    let generate () =
        let eventFactory =
            [|
                fun () -> generateWorkStarted ()
                fun () -> generateBreakStarted ()
                fun () -> generateStopped ()
                fun () -> generateWorkIncreased ()
                fun () -> generateWorkReduced ()
                fun () -> generateBreakIncreased ()
                fun () -> generateBreakReduced ()
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
        | WorkEvent.WorkReduced (createdAt, v, id) -> WorkEvent.WorkReduced (createdAt |> trim, v, id)
        | WorkEvent.WorkIncreased (createdAt, v, id) -> WorkEvent.WorkIncreased (createdAt |> trim, v, id)
        | WorkEvent.BreakReduced (createdAt, v, id) -> WorkEvent.BreakReduced (createdAt |> trim, v, id)
        | WorkEvent.BreakIncreased (createdAt, v, id) -> WorkEvent.BreakIncreased (createdAt |> trim, v, id)

[<RequireQualifiedAccess>]
module ActiveTimePoint =
    let generate () =
        TimePoint.generate ()
        |> TimePoint.toActiveTimePoint

    let withNoRemainingTimeSpan (atp: ActiveTimePoint) =
        { atp with
            RemainingTimeSpan = TimeSpan.Zero
        }