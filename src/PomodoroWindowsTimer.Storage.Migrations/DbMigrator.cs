namespace PomodoroWindowsTimer.Storage.Migrations;

using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DbUp;
using DbUp.Engine;
using DbUp.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.Storage.Migrations.sql;

public sealed class DbMigrator : IDbMigrator
{
    private readonly ILogger<UpgradeEngine> _upgradeEngineLogger;
    private readonly ILogger<DbMigrator> _logger;

    public DbMigrator(ILogger<UpgradeEngine> upgradeEngineLogger, ILogger<DbMigrator> logger)
    {
        _upgradeEngineLogger = upgradeEngineLogger;
        _logger = logger;
    }

    public FSharpResult<Unit, string> ApplyMigrations(string dbFilePath)
    {
        if (string.IsNullOrWhiteSpace(dbFilePath))
        {
            const string _dbFileMustBeSetError = "Database file path must be set.";
            _logger.LogError(_dbFileMustBeSetError);
            return FSharpResult<Unit, string>.NewError(_dbFileMustBeSetError);
        }

        if (!Path.IsPathFullyQualified(dbFilePath))
        {
            dbFilePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                dbFilePath
            );
        }

        if (!File.Exists(dbFilePath))
        {
            const string _dbFileNotExistError = "Database file does not exist.";
            _logger.LogError(_dbFileNotExistError);
            return FSharpResult<Unit, string>.NewError(_dbFileNotExistError);
        }

        var connectionString = "Data Source=" + dbFilePath + ";Pooling=false";

        var upgrader =
            DeployChanges.To
                .SQLiteDatabase(connectionString)
                .WithScriptsAndCodeEmbeddedInAssembly(typeof(Script005_FillActiveTimePointId).Assembly)
                .AddLogger(_upgradeEngineLogger)
                .WithTransaction()
                .Build();


        if (_logger.IsEnabled(LogLevel.Trace))
        {
            var scriptsToExecute = upgrader.GetScriptsToExecute();

            string scriptNames =
                Environment.NewLine +
                string.Join(
                    "," + Environment.NewLine,
                    (scriptsToExecute ?? new List<SqlScript>(0))
                        .Select(s => s.Name)
                );

            _logger.LogTrace("Scripts to execute: {ScriptsToExecute}", scriptNames);
        }

        DatabaseUpgradeResult result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            const string _performUpdateError = "Applying migrations is failed.";
            _logger.LogError(result.Error, _performUpdateError);
            return FSharpResult<Unit, string>.NewError(_performUpdateError);
        }
        else
        {
            _logger.LogInformation("Migrations are applied successfully.");
            return FSharpResult<Unit, string>.NewOk((Unit)default!);
        }
    }


    public Task<FSharpResult<Unit, string>> ApplyMigrationsAsync(string dbFilePath, CancellationToken cancellationToken)
    {
        var res = ApplyMigrations(dbFilePath);
        return Task.FromResult(res);
    }
}
