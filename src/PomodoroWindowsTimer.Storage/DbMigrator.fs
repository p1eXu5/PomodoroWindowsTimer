namespace PomodoroWindowsTimer.Storage

open System
open System.IO
open System.Reflection
open System.Threading.Tasks

open CliWrap
open CliWrap.Buffered
open Microsoft.Extensions.Logging

open PomodoroWindowsTimer.Abstractions

/// <summary>
/// Runs migrator.
/// </summary>
type DbMigrator(userSettings: IDatabaseSettings, logger: ILogger<DbMigrator>) =

    /// <summary>
    /// Runs migrator.
    /// </summary>
    member _.ApplyMigrationsAsync() : Task =
        task {
            let path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

            let databaseFilePath = userSettings.DatabaseFilePath
            let eqInd = databaseFilePath.IndexOf('=')
            let dbFilePath = databaseFilePath.Substring(eqInd + 1, databaseFilePath.Length - eqInd - 2)

            let connectionString =
                if not <| Path.IsPathFullyQualified(dbFilePath) then
                    "Data Source=" + Path.Combine(path, dbFilePath) + ";"
                else
                    databaseFilePath

            let migratorPath =
                Path.Combine(path, "migrator", "PomodoroWindowsTimer.Migrator.exe")

            if File.Exists(migratorPath) then
                let! res =
                    Cli
                        .Wrap(migratorPath)
                        .WithArguments([| "--connection"; connectionString |])
                        .WithWorkingDirectory(Path.Combine(path, "migrator"))
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteBufferedAsync()

                if not <| String.IsNullOrWhiteSpace(res.StandardError) then
                    logger.LogError(res.StandardError)
                else
                    logger.LogInformation(res.StandardOutput)
            else
                logger.LogError("Migrator has not been found!")
        }

    interface IDbMigrator with
        member this.ApplyMigrationsAsync () : Task = 
            this.ApplyMigrationsAsync()

