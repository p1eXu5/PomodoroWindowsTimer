namespace PomodoroWindowsTimer.Installer.Abstractions;

public interface IPwtGitHubClient
{
    Task GetLastVersionAsync(CancellationToken cancellationToken);
}
