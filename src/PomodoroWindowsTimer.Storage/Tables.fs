﻿[<RequireQualifiedAccess>]
module PomodoroWindowsTimer.Storage.Tables

module internal WorkEvent =

    [<CLIMutable>]
    type internal Row =
        {
            id: int64
            work_id: int64
            event_json: string
            created_at: int64
        }
    // with
    //     static member 

    let [<Literal>] NAME = "work_event"

    module Columns =
        let private row : Row = Unchecked.defaultof<_>

        let [<Literal>] id = nameof row.id
        let [<Literal>] work_id = nameof row.work_id
        let [<Literal>] event_json = nameof row.event_json
        let [<Literal>] created_at = nameof row.created_at


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