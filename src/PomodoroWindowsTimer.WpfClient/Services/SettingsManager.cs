using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.WpfClient.Properties;

namespace PomodoroWindowsTimer.WpfClient
{
    public class SettingsManager : ISettingsManager
    {
        static private SettingsManager? _settingsManager;
        static public ISettingsManager Instance => _settingsManager ??= new SettingsManager();

        private SettingsManager()
        {
        }

        public object Load(string key)
        {
            return Settings.Default[key];
        }

        public void Save(string key, object value)
        {
            Settings.Default[key] = value;
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
