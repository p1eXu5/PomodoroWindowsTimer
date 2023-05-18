using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient.Services.Settings
{
    public sealed class DisableSkipBreakSettings : IDisableSkipBreakSettings
    {
        private const string DISABLE_SKIP_BREAK_KEY = "DisableSkipBreak";
        private readonly ISettingsManager _settingsManager;

        public DisableSkipBreakSettings(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public bool DisableSkipBreak
        {
            get => (bool)_settingsManager.Load(DISABLE_SKIP_BREAK_KEY);
            set => _settingsManager.Save(DISABLE_SKIP_BREAK_KEY, value);
        }
    }
}
