using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient.Services.Settings;

public sealed class TimePointSettings : SettingsBase, ITimePointSettings
{
    private const string TIME_POINTS_KEY = "TimePoints";

    public TimePointSettings(ISettingsManager settingsManager)
        : base(settingsManager)
    {
    }

    FSharpOption<string> ITimePointSettings.TimePointSettings
    {
        get => LoadValue(TIME_POINTS_KEY);
        set => SaveValue(TIME_POINTS_KEY, value);
    }
}
