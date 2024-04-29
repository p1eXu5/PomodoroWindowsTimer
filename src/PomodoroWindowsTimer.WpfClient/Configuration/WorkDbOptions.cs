using System.ComponentModel.DataAnnotations;

namespace PomodoroWindowsTimer.WpfClient.Configuration;

internal sealed class WorkDbOptions
{
    [Required]
    [MinLength(10)]
    public string ConnectionString { get; init; } = default!;
}
