using CycleBell.ElmishApp.Abstractions;
using CycleBell.WpfClient.Properties;

namespace CycleBell.WpfClient
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
