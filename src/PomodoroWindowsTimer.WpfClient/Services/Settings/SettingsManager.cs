using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient.Services.Settings
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
            return Properties.Settings.Default[key];
        }

        public void Save(string key, object value)
        {
            Properties.Settings.Default[key] = value;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }
    }
}
