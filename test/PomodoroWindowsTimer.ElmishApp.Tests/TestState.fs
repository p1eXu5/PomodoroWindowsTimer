namespace PomodoroWindowsTimer.ElmishApp.Tests

type TestState<'StateIn, 'StateOut, 'V> = TestState of ('StateIn -> 'StateOut * 'V)


module TestState =

    let retn v = (fun s -> (s, v)) |> TestState

    let bind f (TestState m) =
        fun stateIn ->
            let (s, v) = m stateIn
            let (TestState fm) = f v
            fm s
        |> TestState

    let run state (TestState f) =
        f state |> snd

    let replace newState =
        fun _ ->
            (newState, ())
        |> TestState

    let inline getState<'State> : TestState<'State, 'State, 'State> =
        fun stateIn ->
            (stateIn, stateIn)
        |> TestState

module TestStateCE =
    open TestState

    type TestStateBuilder() =
        member _.Return(v) = retn v
        member _.ReturnFrom(m) = m
        member _.Delay(m) = m
        member _.Run(delayed) = delayed ()
        member _.Zero() = () |> retn
        member _.Bind(m, f) = bind f m
        // member __.Combine (f, g) = f |> bind (fun () -> g)
            

    let test = TestStateBuilder()
