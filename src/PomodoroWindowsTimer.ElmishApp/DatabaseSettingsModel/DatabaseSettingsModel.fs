namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Abstractions

type DatabaseSettingsModel =
    {
        DatabaseFilePath: string
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
            DatabaseFilePath = databaseSettings.DatabaseFilePath
        }

    let withDatabase dbFilePath model =
        { model with DatabaseFilePath = dbFilePath }

