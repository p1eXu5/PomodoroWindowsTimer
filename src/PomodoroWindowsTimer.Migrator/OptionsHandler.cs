using System.Reflection;
using DbUp;
using DbUp.Extensions.Logging;
using Microsoft.Extensions.Logging;
using p1eXu5.CliBootstrap;
using p1eXu5.CliBootstrap.CommandLineParser;

namespace PomodoroWindowsTimer.Migrator;

internal sealed class OptionsHandler : IOptionsHandler
{
    private readonly ILogger<DbUp.Engine.UpgradeEngine> _logger;

    public OptionsHandler(ILogger<DbUp.Engine.UpgradeEngine> logger)
    {
        _logger = logger;
    }

    public bool StopApplication { get; } = false;

    public Task HandleAsync(SuccessParsingResult successParsingResult, CancellationToken cancellationToken)
    {
        if (successParsingResult is SuccessParsingResult.Success<PwtMigratorOptions> success)
        {
            _logger.LogInformation("Strarting migration...");

            var upgrader =
                DeployChanges.To
                    .SQLiteDatabase(success.Options.ConnectionString)
                    .WithScriptsAndCodeEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .AddLogger(_logger)
                    .WithTransaction()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
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
