module PomodoroWindowsTimer.ElmishApp.DatabaseSettingsModel.Program

open System

open Ookii.Dialogs.Wpf

open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.DatabaseSettingsModel

let update (databaseSettings: IDatabaseSettings) msg model =
    match msg with
    | SetDatabase filePath ->
        model |> withDatabase filePath, Intent.None

    | SelectDatabaseFile ->
        let openFileDialog = VistaOpenFileDialog()
        openFileDialog.Filter <- "Database Files (*.db)|*.db"
        openFileDialog.Title <- "Select a Database File"
        let result = openFileDialog.ShowDialog()
        match result |> Option.ofNullable with
        | Some true ->
            let fileName = openFileDialog.FileName
            if not (String.IsNullOrWhiteSpace(fileName)) && fileName.Length >= 16 then
                model |> withDatabase fileName, Intent.None
            else
                 model, Intent.None
        | _ ->
            model, Intent.None

    | Apply ->
        databaseSettings.DatabaseFilePath <- model.DatabaseFilePath
        model, Intent.CloseDialogRequested

    | Cancel ->
        model, Intent.CloseDialogRequested

