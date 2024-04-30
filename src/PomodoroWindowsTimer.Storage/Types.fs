namespace PomodoroWindowsTimer.Storage

open System.Data

type internal Cfg =
    {
        DbConnection: IDbConnection
        TimeProvider: System.TimeProvider
    }