using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient.Services.Settings;

public sealed class TimePointPrototypesSettings : ITimePointPrototypesSettings
{
    private readonly ISettingsManager _settingsManager;

    public TimePointPrototypesSettings(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    FSharpOption<string> ITimePointPrototypesSettings.TimePointPrototypesSettings
    {
        get
        {
            var t = _settingsManager.Load("TimePointPrototypes") as string;
            if (string.IsNullOrWhiteSpace(t))
            {
                return FSharpOption<string>.None;
            }

            return FSharpOption<string>.Some(t);
        }

        set
        {
            if (FSharpOption<string>.get_IsSome(value))
            {
                _settingsManager.Save("TimePointPrototypes", value.Value);
            }
            else
            {
                _settingsManager.Save("TimePointPrototypes", null);
            }
        }
    }
}
