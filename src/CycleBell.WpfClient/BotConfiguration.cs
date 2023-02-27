using CycleBell.ElmishApp.Abstractions;
using Microsoft.Extensions.Configuration;

namespace CycleBell.WpfClient;

public class BotConfiguration : IBotConfiguration
{
    public const string BotConfigurationSectionName = "BotConfiguration";

    private IConfigurationSection _configurationSection;

    public BotConfiguration()
    {
        _configurationSection = App.Configuration.GetSection(BotConfigurationSectionName);
    }

    public string BotToken => _configurationSection.GetSection("BotToken").Value!;

    public string MyChatId => _configurationSection.GetSection("MyChatId").Value!;
}
