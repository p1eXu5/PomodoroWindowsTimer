using CycleBell.ElmishApp.Abstractions;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace CycleBell.WpfClient;

public class BotConfiguration : IBotConfiguration
{
    public const string BotConfigurationSectionName = "BotConfiguration";
    private readonly ISettingsManager _settingsManager;
    private IConfigurationSection? _configurationSection;

    public BotConfiguration(ISettingsManager settingsManager)
    {
        _configurationSection = App.Configuration.GetSection(BotConfigurationSectionName);
        _settingsManager = settingsManager;
    }

    public string BotToken
    {
        get
        {
            var t = _settingsManager.Load("BotToken") as string;
            if (string.IsNullOrWhiteSpace(t))
            {
                t = _configurationSection?.GetSection("BotToken")?.Value;
                _settingsManager.Save("BotToken", t ?? "");
            }

            return t;
        }

        set =>
            _settingsManager.Save("BotToken", value);
    }

    public string MyChatId
    {
        get
        {
            var ch = _settingsManager.Load("MyChatId") as string;
            if (string.IsNullOrWhiteSpace(ch))
            {
                ch = _configurationSection?.GetSection("MyChatId")?.Value;
                _settingsManager.Save("MyChatId", ch ?? "");
            }

            return ch;
        }

        set =>
            _settingsManager.Save("MyChatId", value);
    }
}
