[<RequireQualifiedAccess>]
module PomodoroWindowsTimer.Storage.Tables

module internal ActiveTimePoint =

    open System
    open PomodoroWindowsTimer.Types

    [<CLIMutable>]
    type internal Row =
        {
            id: string
            original_id: string
            name: string
            time_span: string
            kind: string
            kind_alias: string
            created_at: int64
        }
        with
            static member ToActiveTimePoint(row: Row) : ActiveTimePoint =
                let kind =
                    if String.Equals(row.kind, Kind.Work |> Kind.displayString, StringComparison.OrdinalIgnoreCase) then
                        Kind.Work
                    elif String.Equals(row.kind, Kind.Break |> Kind.displayString, StringComparison.OrdinalIgnoreCase) then
                        Kind.Break
                    else
                        Kind.LongBreak
                {
                    Id = Guid.Parse(row.id)
                    OriginalId = Guid.Parse(row.original_id)
                    Name = row.name
                    RemainingTimeSpan = TimeSpan.Zero
                    TimeSpan = DateTime.ParseExact(row.time_span, "yyyy-MM-dd HH:mm:ss", null).TimeOfDay
                    Kind = kind
                    KindAlias = Alias.createOrThrow row.kind_alias
                }


    let [<Literal>] NAME = "active_time_point"

    module Columns =
        let private row : Row = Unchecked.defaultof<_>

        let [<Literal>] id = nameof row.id
        let [<Literal>] original_id = nameof row.original_id
        let [<Literal>] name = nameof row.name
        let [<Literal>] time_span = nameof row.time_span
        let [<Literal>] kind = nameof row.kind
        let [<Literal>] kind_alias = nameof row.kind_alias
        let [<Literal>] created_at = nameof row.created_at

module internal WorkEvent =

    [<CLIMutable>]
    type internal Row =
        {
            id: int64
            work_id: int64
            event_json: string
            created_at: int64
            active_time_point_id: string
            event_name: string
        }

    let [<Literal>] NAME = "work_event"

    module Columns =
        let private row : Row = Unchecked.defaultof<_>

        let [<Literal>] id = nameof row.id
        let [<Literal>] work_id = nameof row.work_id
        let [<Literal>] event_json = nameof row.event_json
        let [<Literal>] created_at = nameof row.created_at
        let [<Literal>] active_time_point_id = nameof row.active_time_point_id
        let [<Literal>] event_name = nameof row.event_name


module internal Work =

    open System
    open PomodoroWindowsTimer.Types

    [<CLIMutable>]
    type internal Row =
        {
            id: int64
            number: string
            title: string
            created_at: int64
            updated_at: int64
            last_event_created_at: LastEventCreatedAt
        }
    with
        static member ToWork (row: Row) : Work =
            let lastEventCreatedAt =
                match box row.last_event_created_at with
                | null -> None
                | _ -> row.last_event_created_at.Value

            {
                Id = uint64 row.id
                Number = row.number
                Title = row.title
                CreatedAt = row.created_at |> DateTimeOffset.FromUnixTimeMilliseconds
                UpdatedAt = row.updated_at |> DateTimeOffset.FromUnixTimeMilliseconds
                LastEventCreatedAt = lastEventCreatedAt
            }

    let [<Literal>] NAME = "work"

    module Columns =
        let private row : Row = Unchecked.defaultof<_>

        let [<Literal>] id = nameof row.id
        let [<Literal>] number = nameof row.number
        let [<Literal>] title = nameof row.title
        let [<Literal>] created_at = nameof row.created_at
        let [<Literal>] updated_at = nameof row.updated_at
        let [<Literal>] last_event_created_at = nameof row.last_event_created_at