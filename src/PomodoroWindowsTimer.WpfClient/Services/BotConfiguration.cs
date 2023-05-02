﻿using PomodoroWindowsTimer.ElmishApp.Abstractions;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace PomodoroWindowsTimer.WpfClient;

public class BotConfiguration : IBotConfiguration
{
    public const string BOT_CONFIGURATION_SECTION_NAME = "BotConfiguration";

    private readonly ISettingsManager _settingsManager;
    private readonly IConfigurationSection _configurationSection;

    public BotConfiguration(ISettingsManager settingsManager)
    {
        _configurationSection = App.Configuration.GetSection(BOT_CONFIGURATION_SECTION_NAME);
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
