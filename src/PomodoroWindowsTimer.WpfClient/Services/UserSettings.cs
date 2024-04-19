﻿using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.WpfClient.Extensions;


namespace PomodoroWindowsTimer.WpfClient.Services;

internal class UserSettings : IUserSettings
{
    public const string BOT_TOKEN_KEY = "BotToken";
    public const string MY_CHAT_ID_KEY = "MyChatId";

    private readonly IConfigurationSection _botSection;

    public UserSettings(IConfigurationSection botSection)
    {
        _botSection = botSection;
    }

    public bool DisableSkipBreak
    {
        get => Properties.Settings.Default.DisableSkipBreak;
        set => Properties.Settings.Default.DisableSkipBreak = value;
    }

    public FSharpOption<string> TimePointSettings
    {
        get => Properties.Settings.Default.TimePoints.ToFSharpOption();
        set => Properties.Settings.Default.TimePoints = value.FromFSharpOption();
    }

    public FSharpOption<string> TimePointPrototypesSettings
    {
        get => Properties.Settings.Default.TimePointPrototypes.ToFSharpOption();
        set => Properties.Settings.Default.TimePointPrototypes = value.FromFSharpOption();
    }
    public FSharpList<string> Patterns
    {
        get {
            var patterns = Properties.Settings.Default.Patterns;
            if (patterns is null)
            {
                return FSharpList<string>.Empty;
            }

            return SeqModule.ToList<string>(patterns.Cast<string>());
        }

        set {
            StringCollection coll = new StringCollection();

            foreach (string item in value)
            {
                coll.Add(item);
            }

            Properties.Settings.Default.Patterns = coll;
        }
    }

    public FSharpOption<string> BotToken
    {
        get
        {
            var botToken = Properties.Settings.Default.BotToken;
            if (string.IsNullOrWhiteSpace(botToken))
            {
                botToken = _botSection?.GetSection(BOT_TOKEN_KEY)?.Value;
                Properties.Settings.Default.BotToken = botToken;
            }

            return botToken.ToFSharpOption();
        }
        set => Properties.Settings.Default.BotToken = value.FromFSharpOption();
    }
    public FSharpOption<string> MyChatId
    {
        get {
            var myChatId = Properties.Settings.Default.MyChatId;
            if (string.IsNullOrWhiteSpace(myChatId))
            {
                myChatId = _botSection?.GetSection(MY_CHAT_ID_KEY)?.Value;
                Properties.Settings.Default.MyChatId = myChatId;
            }

            return myChatId.ToFSharpOption();
        }

        set => Properties.Settings.Default.MyChatId = value.FromFSharpOption();
    }
}
