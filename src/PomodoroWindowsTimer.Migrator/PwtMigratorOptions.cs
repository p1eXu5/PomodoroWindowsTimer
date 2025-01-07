using CommandLine;
using p1eXu5.CliBootstrap.CommandLineParser.Options;
using PomodoroWindowsTimer.Abstractions;

namespace PomodoroWindowsTimer.Migrator;

internal sealed record PwtMigratorOptions : CliOptions, IDatabaseSettings
{
    [Option(longName: "database-file-path", HelpText = "Database file path", Required = true)]
    public required string DatabaseFilePath { get; set; }

    [Option(longName: "pooling", HelpText = "Pooling", Required = false)]
    public bool? Pooling { get; set; }

    public string? Mode { get; }

    public string? Cache { get; }
}
