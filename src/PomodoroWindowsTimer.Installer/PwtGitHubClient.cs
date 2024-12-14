using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using PomodoroWindowsTimer.Installer.Abstractions;
using PomodoroWindowsTimer.Installer.Configuration;

namespace PomodoroWindowsTimer.Installer;

internal sealed class PwtGitHubClient : IPwtGitHubClient
{
    private readonly IOptions<PwtGitHubClientOptions> _pwtGitHubClientOptions;
    private readonly ILogger<PwtGitHubClient> _logger;

    public PwtGitHubClient(IOptions<PwtGitHubClientOptions> pwtGitHubClientOptions, ILogger<PwtGitHubClient> logger)
    {
        _pwtGitHubClientOptions = pwtGitHubClientOptions;
        _logger = logger;
    }

    public async Task GetLastVersionAsync(CancellationToken _)
    {
        var client = CreateClient();

        var options = _pwtGitHubClientOptions.Value;
        var releases = await client.Repository.Release.GetAll(options.Owner, options.RepositoryName);

        var latest = releases[0];
        _logger.LogInformation(
            "The latest release is tagged at {TagName} and is named {Name}",
            latest.TagName,
            latest.Name);
    }

    private IGitHubClient CreateClient()
    {
        var options = _pwtGitHubClientOptions.Value;
        return new GitHubClient(new ProductHeaderValue(options.ProductHeaderValue));
    }
}
