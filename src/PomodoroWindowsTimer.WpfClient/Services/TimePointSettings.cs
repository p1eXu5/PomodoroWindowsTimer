using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient.Services;

public sealed class TimePointSettings : ITimePointSettings
{
    private readonly ISettingsManager _settingsManager;

    public TimePointSettings(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    FSharpOption<string> ITimePointSettings.TimePointSettings
    {
        get
        {
            var t = _settingsManager.Load("TimePoints") as string;
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
                _settingsManager.Save("TimePoints", value.Value);
            }
            else
            {
                _settingsManager.Save("TimePoints", null);
            }
        }
    }
}
