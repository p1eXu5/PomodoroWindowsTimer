namespace PomodoroWindowsTimer

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

type TimePointStore =
    {
        Read: unit -> TimePoint list
        Write: TimePoint list -> unit
    }

module TimePointStore =

    type private PrevTimePoint =
        {
            Id: TimePointId
            Name: Name
            TimeSpan: System.TimeSpan
            Kind: Kind
            KindAlias: Alias
        }

    let read (timePointSettings : ITimePointSettings) =
        timePointSettings.TimePointSettings
        |> Option.map (fun s ->
            try
                JsonHelpers.Deserialize<TimePoint list>(s)
            with _ ->
                let l =
                    JsonHelpers.Deserialize<PrevTimePoint list>(s)
                    |> List.mapi (fun ind tp ->
                        {
                            Id = tp.Id
                            Num = ind
                            Name = tp.Name
                            TimeSpan = tp.TimeSpan
                            Kind = tp.Kind
                            KindAlias = tp.KindAlias
                        }
                    )
                let s' = JsonHelpers.Serialize(l)
                timePointSettings.TimePointSettings <- s' |> Some
                l
        )
        |> Option.defaultValue TimePoint.defaults


    let write (timePointSettings : ITimePointSettings) (timePoints: TimePoint list) =
        match timePoints with
        | [] ->
            timePointSettings.TimePointSettings <- None
        | _ ->
            let s = JsonHelpers.Serialize(timePoints)
            timePointSettings.TimePointSettings <- s |> Some

    [<CompiledName("Initialize")>]
    let initialize (timePointSettings : ITimePointSettings) : TimePointStore =
        {
            Read = fun () -> read timePointSettings
            Write = write timePointSettings
        }
