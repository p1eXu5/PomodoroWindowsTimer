namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Abstractions

type DatabaseSettingsModel =
    {
        Database: string
    }

module DatabaseSettingsModel =

    type Msg =
        | SetDatabase of string
        | SelectDatabaseFile
        | Apply
        | Cancel

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | CloseDialogRequested

    let init (databaseSettings: IDatabaseSettings) =
        {
            Database = databaseSettings.DatabaseFilePath
        }

    let withDatabase fileName model =
        { model with Database = $"Data Source={fileName}" }

