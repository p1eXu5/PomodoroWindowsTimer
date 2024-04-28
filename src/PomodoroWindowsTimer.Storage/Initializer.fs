module PomodoroWindowsTimer.Storage.Initializer

open System.Data
open Dapper

[<Literal>]
let internal CREATE_WORK_TABLE =
    """
    CREATE TABLE IF NOT EXISTS work (
          id INTEGER PRIMARY KEY AUTOINCREMENT
        , number TEXT
        , title TEXT NOT NULL
        , created_at INTEGER NOT NULL
        , updated_at INTEGER

        , UNIQUE(number, title)
    )
    """

[<Literal>]
let internal CREATE_WORK_EVENT_TABLE =
    """
    CREATE TABLE IF NOT EXISTS work_event (
          id INTEGER PRIMARY KEY AUTOINCREMENT
        , work_id INTEGER NOT NULL
        , event_json TEXT NOT NULL
        , created_at INTEGER NOT NULL

        , FOREIGN KEY (work_id)
              REFERENCES work (id)
                 ON DELETE CASCADE 
                 ON UPDATE NO ACTION
    )
    """


let initdb (connection: IDbConnection) =
    task {
        let! _ = connection.ExecuteAsync(CREATE_WORK_TABLE)
        let! _ = connection.ExecuteAsync(CREATE_WORK_EVENT_TABLE)
        return()
    }


do
    Dapper.FSharp.SQLite.OptionTypes.register()

