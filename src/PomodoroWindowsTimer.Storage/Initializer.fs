module PomodoroWindowsTimer.Storage.Initializer

open System.Data
open System.Threading
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
        do! connection.OpenAsync()
        let! _ = connection.ExecuteAsync(CREATE_WORK_TABLE)
        let! _ = connection.ExecuteAsync(CREATE_WORK_EVENT_TABLE)
        return()
    }

    
let initWorkRepository (connectionString: string) (timeProvider: System.TimeProvider) =
    { new IWorkRepository with
        member _.CreateAsync number title ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkRepository.createTask timeProvider (Helpers.execute dbConnection) number title ct
            }
        member _.ReadAllAsync ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkRepository.readAllTask (Helpers.selectTask dbConnection) ct
            }
        member _.FindByIdAsync id ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkRepository.findByIdTask (Helpers.selectTask dbConnection) id ct
            }
        member _.FindByIdOrCreateAsync work ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkRepository.findOrCreateTask timeProvider (Helpers.selectTask dbConnection) (Helpers.execute dbConnection) work ct
            }
        member _.UpdateAsync work ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkRepository.updateTask timeProvider (Helpers.update dbConnection) work ct
            }
    }


let initWorkEventRepository (connectionString: string) (timeProvider: System.TimeProvider) =
    { new IWorkEventRepository with
        member _.CreateAsync workId workEvent ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkEventRepository.createTask timeProvider (Helpers.execute dbConnection) workId workEvent ct
            }
        member _.ReadAllAsync workId ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkEventRepository.readAllTask (Helpers.selectTask dbConnection) workId ct
            }
        
        member _.ReadAll workId =
            use dbConnection = new SqliteConnection(connectionString)
            dbConnection.Open()
            WorkEventRepository.readAll (Helpers.select dbConnection) workId

        member _.FindByDateAsync workId date ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkEventRepository.findByDateTask timeProvider (Helpers.selectTask dbConnection) workId date ct
            }
        member _.FindByPeriodAsync workId period ct =
            task {
                use dbConnection = new SqliteConnection(connectionString)
                do! dbConnection.OpenAsync()
                return! WorkEventRepository.findByPeriodTask timeProvider (Helpers.selectTask dbConnection) workId period ct
            }
    }


do
    Dapper.FSharp.SQLite.OptionTypes.register()

