[<AutoOpen>]
module PomodoroWindowsTimer.Helpers

module Unit =
    [<CompiledName("Value")>]
    let value = ()

module String =

    open System

    let equalOrigin (s1: string) (s2: string) = s1.Equals(s2, StringComparison.Ordinal)


module Option =

    let equalOrigin (s1: string option) (s2: string option) =
        match s1, s2 with
        | Some s1, Some s2 -> s1 |> String.equalOrigin s2
        | _ -> false

module DateTimeOffset =
    let [<Literal>] defaultFormat = "dd.MM.yyyy HH:mm:ss.fffff zzz"
