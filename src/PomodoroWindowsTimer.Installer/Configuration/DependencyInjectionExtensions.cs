using Microsoft.Extensions.DependencyInjection;
using PomodoroWindowsTimer.Installer.Abstractions;

namespace PomodoroWindowsTimer.Installer.Configuration;

public static class DependencyInjectionExtensions
{
    public static void AddInstallerServices(this IServiceCollection services)
    {
        services
            .AddOptions<PwtGitHubClientOptions>()
            .BindConfiguration(PwtGitHubClientOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<IPwtGitHubClient, PwtGitHubClient>();
    }
}
