module PomodoroWindowsTimer.Storage.Initializer

open System.Data
open Dapper
open Microsoft.Data.Sqlite
open PomodoroWindowsTimer.Abstractions

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


let initdb (connectionString: string) =
    task {
        use connection = new SqliteConnection(connectionString)
        let! _ = connection.ExecuteAsync(CREATE_WORK_TABLE)
        let! _ = connection.ExecuteAsync(CREATE_WORK_EVENT_TABLE)
        return()
    }

    
let initWorkRepository (connectionString: string) (timeProvider: System.TimeProvider) =
    { new IWorkRepository with
        member _.Create number title ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                return! WorkRepository.create timeProvider (Helpers.execute dbConnection) number title ct
            }
        member _.ReadAll ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                return! WorkRepository.readAll (Helpers.select dbConnection) ct
            }
        member _.FindById id ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                return! WorkRepository.findById (Helpers.select dbConnection) id ct
            }
        member _.FindByIdOrCreate work ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                return! WorkRepository.findOrCreate timeProvider (Helpers.select dbConnection) (Helpers.execute dbConnection) work ct
            }
        member _.Update work ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                return! WorkRepository.update timeProvider (Helpers.update dbConnection) work ct
            }
    }


let initWorkEventRepository (connectionString: string) (timeProvider: System.TimeProvider) =
    { new IWorkEventRepository with
        member _.Create workId workEvent ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                return! WorkEventRepository.create timeProvider (Helpers.execute dbConnection) workId workEvent ct
            }
        member _.ReadAll ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                return! WorkEventRepository.readAll (Helpers.select dbConnection) ct
            }
    }


do
    Dapper.FSharp.SQLite.OptionTypes.register()

