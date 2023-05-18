using System.Linq;
using Microsoft.FSharp.Collections;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using System.Collections.Specialized;

namespace PomodoroWindowsTimer.WpfClient.Services.Settings;

public sealed class PatternSettings : IPatternSettings
{
    private readonly ISettingsManager _settingsManager;

    public PatternSettings(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    public FSharpList<string> Read()
    {
        var coll = _settingsManager.Load("Patterns") as StringCollection;
        if (coll is null)
        {
            return FSharpList<string>.Empty;
        }

        return SeqModule.ToList(coll.Cast<string>());
    }

    public void Write(FSharpList<string> list)
    {
        StringCollection coll = new StringCollection();
        foreach (var item in list)
        {
            coll.Add(item);
        }
        _settingsManager.Save("Patterns", coll);
    }
}
