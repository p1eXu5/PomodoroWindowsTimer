namespace PomodoroWindowsTimer.ElmishApp.Tests

open System.Runtime.CompilerServices
open Bogus

[<AutoOpen>]
module FakerInstance =
    let faker = new Faker("en")


[<Extension>]
type FakerExtensions =

    [<Extension>]
    static member Numeric(faker: Faker, length: int) =
        faker.Random.String2(length, "0123456789")

    [<Extension>]
    static member Numeric(faker: Faker, min: int, max: int) =
        faker.Random.String2(faker.Random.Int(min, max), "0123456789")
