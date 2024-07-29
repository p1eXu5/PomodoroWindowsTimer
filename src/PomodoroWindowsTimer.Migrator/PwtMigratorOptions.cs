using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using p1eXu5.CliBootstrap.CommandLineParser.Options;

namespace PomodoroWindowsTimer.Migrator;

internal sealed record PwtMigratorOptions : CliOptions
{
    [Option(longName: "connection", HelpText = "Connection string", Required = true)]
    public required string ConnectionString { get; init; }
}
