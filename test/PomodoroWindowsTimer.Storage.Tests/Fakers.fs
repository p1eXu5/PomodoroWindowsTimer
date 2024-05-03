[<AutoOpen>]
module PomodoroWindowsTimer.Storage.Tests.Fakers

open Bogus
open PomodoroWindowsTimer.Types

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

let mutable private workCounter = 0
let mutable private breakCounter = 0
let mutable private longBreakCounter = 0

let generateTimePointName () : string =
    let nameFactory =
        [|
            fun () ->
                let name = sprintf "Work %i" workCounter
                workCounter <- workCounter + 1
                name
            fun () ->
                let name = sprintf "Break %i" breakCounter
                breakCounter <- breakCounter + 1
                name
            fun () ->
                let name = sprintf "Long Break %i" longBreakCounter
                longBreakCounter <- longBreakCounter + 1
                name
        |]
    (faker.Random.ArrayElement(nameFactory)) ()

let generateWorkStartedEvent () =
    WorkEvent.WorkStarted
        (faker.Date.RecentOffset(), generateTimePointName ())

let generateBreakStartedEvent () =
    WorkEvent.BreakStarted
        (faker.Date.RecentOffset(), generateTimePointName ())

let generateStoppedEvent () =
    WorkEvent.Stopped
        (faker.Date.RecentOffset())

let generateWorkEvent () : WorkEvent =
    let eventFactory =
        [|
            fun () -> generateWorkStartedEvent ()
            fun () -> generateBreakStartedEvent ()
            fun () -> generateStoppedEvent ()
        |]

    (faker.Random.ArrayElement(eventFactory)) ()

