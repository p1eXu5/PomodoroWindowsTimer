using Microsoft.Extensions.Logging;
using p1eXu5.CliBootstrap;
using p1eXu5.CliBootstrap.CommandLineParser;
using PomodoroWindowsTimer.Abstractions;

namespace PomodoroWindowsTimer.Migrator;

internal sealed class OptionsHandler : IOptionsHandler
{
    private readonly IDbMigrator _dbMigrator;
    private readonly ILogger<DbUp.Engine.UpgradeEngine> _logger;

    public OptionsHandler(IDbMigrator dbMigrator, ILogger<DbUp.Engine.UpgradeEngine> logger)
    {
        _dbMigrator = dbMigrator;
        _logger = logger;
    }

    public bool StopApplication { get; } = false;

    public Task HandleAsync(SuccessParsingResult successParsingResult, CancellationToken cancellationToken)
    {
        if (successParsingResult is SuccessParsingResult.Success<PwtMigratorOptions> success)
        {
            _logger.LogInformation("Starting migration...");

            var result = _dbMigrator.ApplyMigrations(success.Options);

            if (result.IsError)
            {
                Environment.Exit(-1);
            }
            else
            {
                _logger.LogInformation("Finished.");
                Environment.Exit(0);
            }
        }

        return Task.CompletedTask;
    }
}
