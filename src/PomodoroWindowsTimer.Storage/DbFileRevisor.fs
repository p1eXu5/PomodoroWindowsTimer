namespace PomodoroWindowsTimer.Storage

open System.Threading
open System.Threading.Tasks
open PomodoroWindowsTimer.Abstractions
open System

module internal DbFileRevisor =

    open System.IO

    let private checkFileNotExistOrEmpty (dbFilePath: string) =
        if not <| File.Exists(dbFilePath) then
            true
        else
            let fileInfo = FileInfo(dbFilePath)
            fileInfo.Length = 0L

    let private checkDatabaseStructure (dbSeeder: IDbSeeder) (dbSettings: IDatabaseSettings) (cancellationToken: CancellationToken) =
        task {
            let repositoryFactory = dbSeeder.RepositoryFactory.Replicate dbSettings
            match! repositoryFactory.ReadDbTablesAsync cancellationToken with
            | Ok tables ->
                let workTableExists =
                    tables
                    |> List.exists (fun tn ->
                        tn.Equals(Tables.Work.NAME, StringComparison.Ordinal)
                    )
                if not workTableExists then return Choice3Of3 "Db file does not contain work table" 
                else
                    let workEventTableExists =
                        tables
                        |> List.exists (fun tn ->
                            tn.Equals(Tables.WorkEvent.NAME, StringComparison.Ordinal)
                        )
                    if not workEventTableExists then return Choice3Of3 "Db file does not contain work_event table"
                    else
                        let schemaVersionsTableExists =
                            tables
                            |> List.exists (fun tn ->
                                tn.Equals("SchemaVersions", StringComparison.Ordinal)
                            )
                        if schemaVersionsTableExists then
                            return Choice2Of3 ()
                        else
                            return Choice1Of3 ()

            | Error err ->
                return Choice3Of3 err
        }

    let tryUpdateDatabaseFile (dbSeeder: IDbSeeder) (dbMigrator: IDbMigrator) (dbSettings: IDatabaseSettings) (cancellationToken: CancellationToken) =
        task {
            if checkFileNotExistOrEmpty dbSettings.DatabaseFilePath then
                match! dbSeeder.SeedDatabaseAsync(dbSettings, cancellationToken) with
                | Ok () ->
                    return dbMigrator.ApplyMigrations dbSettings
                | Error err ->
                    return Error err
            else
                match! checkDatabaseStructure dbSeeder dbSettings cancellationToken with
                | Choice1Of3 _ ->
                    return dbMigrator.ApplyMigrations dbSettings
                | Choice2Of3 _ ->
                    return Ok ()
                | Choice3Of3 err ->
                    return Error err
        }

    let init (dbSeeder: IDbSeeder) (dbMigrator: IDbMigrator) =
        { new IDbFileRevisor with
            member _.TryUpdateDatabaseFileAsync dbSettings ct =
                tryUpdateDatabaseFile dbSeeder dbMigrator dbSettings ct
        }



