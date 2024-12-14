using p1eXu5.CliBootstrap.CommandLineParser;
using PomodoroWindowsTimer.Installer.Abstractions;

namespace PomodoroWindowsTimer.AppAgent;

internal class CliOptionsHandler : p1eXu5.CliBootstrap.IOptionsHandler
{
    private readonly IPwtGitHubClient _pwtGitHubClient;

    public CliOptionsHandler(IPwtGitHubClient pwtGitHubClient)
    {
        _pwtGitHubClient = pwtGitHubClient;
    }

    public bool StopApplication { get; } = true;

    public async Task HandleAsync(SuccessParsingResult successParsingResult, CancellationToken cancellationToken)
    {
        switch (successParsingResult)
        {
            case SuccessParsingResult.Success<GetLastVersionVerb> opts:
                await _pwtGitHubClient.GetLastVersionAsync(cancellationToken);
                break;
        }
    }
}
