namespace PomodoroWindowsTimer.Storage

open System
open System.IO
open System.Reflection
open System.Threading.Tasks

open CliWrap
open CliWrap.Buffered
open Microsoft.Extensions.Logging

open PomodoroWindowsTimer.Abstractions
open System.Threading

/// <summary>
/// Runs migrator.
/// </summary>
type CliDbMigrator(logger: ILogger<CliDbMigrator>) =

    /// <summary>
    /// Runs migrator.
    /// </summary>
    member _.ApplyMigrationsAsync(databaseSettings: IDatabaseSettings, cancellationToken: CancellationToken) : Task<Result<unit, string>> =
        task {
            let path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

            let databaseFilePath = databaseSettings.DatabaseFilePath
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
                        .ExecuteBufferedAsync(cancellationToken)

                if not <| String.IsNullOrWhiteSpace(res.StandardError) then
                    logger.LogError(res.StandardError)
                    return Error (res.StandardError)
                else
                    logger.LogInformation(res.StandardOutput)
                    return Ok ()
            else
                logger.LogError("Migrator has not been found!")
                return Error "Migrator has not been found!"
        }

    interface IDbMigrator with
        member this.ApplyMigrations (databaseSettings: IDatabaseSettings) : Result<unit, string> = 
            this.ApplyMigrationsAsync(databaseSettings, CancellationToken.None)
            |> Async.AwaitTask
            |> Async.RunSynchronously

        member this.ApplyMigrationsAsync (databaseSettings: IDatabaseSettings) ct : Task<Result<unit, string>> = 
            this.ApplyMigrationsAsync(databaseSettings, ct)

