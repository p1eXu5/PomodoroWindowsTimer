using PomodoroWindowsTimer.ElmishApp.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.WpfClient.Extensions;

#nullable disable

namespace PomodoroWindowsTimer.WpfClient.Services.Settings;

public class BotSettings : SettingsBase, IBotSettings
{
    public const string BOT_TOKEN_KEY = "BotToken";
    public const string MY_CHAT_ID_KEY = "MyChatId";
    public const string BOT_CONFIGURATION_SECTION_NAME = "BotConfiguration";

    private readonly IConfigurationSection _configurationSection;

    public BotSettings(ISettingsManager settingsManager)
        : base(settingsManager)
    {
        _configurationSection = App.Configuration.GetSection(BOT_CONFIGURATION_SECTION_NAME);
    }

    public FSharpOption<string> BotToken
    {
        get
        {
            var botToken = SettingsManager.Load(BOT_TOKEN_KEY) as string;
            if (string.IsNullOrWhiteSpace(botToken))
            {
                botToken = _configurationSection?.GetSection(BOT_TOKEN_KEY)?.Value;
                SettingsManager.Save(BOT_TOKEN_KEY, botToken);

                return botToken.ToFSharpOption();
            }

            return FSharpOption<string>.Some(botToken);
        }

        set => SaveValue(BOT_TOKEN_KEY, value);
    }

    public FSharpOption<string> MyChatId
    {
        get
        {
            var myChatId = SettingsManager.Load(MY_CHAT_ID_KEY) as string;
            if (string.IsNullOrWhiteSpace(myChatId))
            {
                myChatId = _configurationSection?.GetSection(MY_CHAT_ID_KEY)?.Value;
                SettingsManager.Save(MY_CHAT_ID_KEY, myChatId);

                return myChatId.ToFSharpOption();
            }

            return FSharpOption<string>.Some(myChatId);
        }

        set => SaveValue(MY_CHAT_ID_KEY, value);
    }
}
