using PomodoroWindowsTimer.Migrator;

await new PwtMigratorBootstrap().RunAsync(args).ConfigureAwait(false);