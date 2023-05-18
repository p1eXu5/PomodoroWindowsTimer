using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.WpfClient.Extensions;

namespace PomodoroWindowsTimer.WpfClient.Services.Settings;

public abstract class SettingsBase
{
    protected SettingsBase(ISettingsManager settingsManager)
    {
        SettingsManager = settingsManager;
    }

    protected ISettingsManager SettingsManager { get; }

    protected void SaveValue(string key, FSharpOption<string> value)
    {
        if (FSharpOption<string>.get_IsSome(value))
        {
            SettingsManager.Save(key, value.Value);
        }
        else
        {
            SettingsManager.Save(key, null);
        }
    }

    protected FSharpOption<string> LoadValue(string key)
    {
        var value = SettingsManager.Load(key) as string;
        return value.ToFSharpOption();
    }
}
