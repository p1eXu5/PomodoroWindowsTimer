using System.ComponentModel.DataAnnotations;

namespace PomodoroWindowsTimer.Installer.Configuration;

/// <summary>
/// 
/// </summary>
internal sealed record PwtGitHubClientOptions
{
    public const string SectionName = "PwtGitHubClient";

    [Required, MinLength(1)]
    public string ProductHeaderValue { get; init; } = default!;

    [Required, MinLength(1)]
    public string Owner { get; init; } = default!;


    [Required, MinLength(1)]
    public string RepositoryName { get; init; } = default!;
}
