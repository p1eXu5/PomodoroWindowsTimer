using CommandLine;

namespace PomodoroWindowsTimer.AppAgent;

[Verb("--last-version")]
internal sealed record GetLastVersionVerb : p1eXu5.CliBootstrap.CommandLineParser.Options.CliOptions;
