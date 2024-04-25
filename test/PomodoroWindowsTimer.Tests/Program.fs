open p1eXu5.AspNetCore.Testing.Logging
open NUnit.Framework

module Program =

    [<EntryPoint>]
    let main _ =
        TestContextWriters.DefaultWith(TestContext.Progress, TestContext.Out) |> ignore
        0
