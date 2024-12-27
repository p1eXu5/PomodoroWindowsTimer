using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.Storage.Configuration;
using PomodoroWindowsTimer.Types;
using PomodoroWindowsTimer.WpfClient.Extensions;


namespace PomodoroWindowsTimer.WpfClient.Services;

internal class UserSettings : IUserSettings
{
    public const string BOT_TOKEN_KEY = "BotToken";
    public const string MY_CHAT_ID_KEY = "MyChatId";

    private readonly IConfigurationSection _botSection;
    private readonly WorkDbOptions _workDbOptions;

    public UserSettings(IConfigurationSection botSection, IOptions<WorkDbOptions> workDbOptions)
    {
        _botSection = botSection;
        _workDbOptions = workDbOptions.Value;
    }

    public bool DisableSkipBreak
    {
        get => Properties.Settings.Default.DisableSkipBreak;
        set
        {
            Properties.Settings.Default.DisableSkipBreak = value;
            Properties.Settings.Default.Save();
        }
    }

    public FSharpOption<string> TimePointSettings
    {
        get => Properties.Settings.Default.TimePoints.ToFSharpOption();
        set
        {
            Properties.Settings.Default.TimePoints = value.FromFSharpOption();
            Properties.Settings.Default.Save();
        }
    }

    public FSharpOption<string> TimePointPrototypesSettings
    {
        get => Properties.Settings.Default.TimePointPrototypes.ToFSharpOption();
        set
        {
            Properties.Settings.Default.TimePointPrototypes = value.FromFSharpOption();
            Properties.Settings.Default.Save();
        }
    }
    public FSharpList<string> Patterns
    {
        get
        {
            StringCollection? patterns = Properties.Settings.Default.Patterns;
            if (patterns is null)
            {
                return FSharpList<string>.Empty;
            }

            return SeqModule.ToList<string>(patterns.Cast<string>());
        }

        set
        {
            StringCollection coll = new StringCollection();

            foreach (string item in value)
            {
                coll.Add(item);
            }

            Properties.Settings.Default.Patterns = coll;
            Properties.Settings.Default.Save();
        }
    }

    public FSharpOption<string> BotToken
    {
        get
        {
            string? botToken = Properties.Settings.Default.BotToken;
            if (string.IsNullOrWhiteSpace(botToken))
            {
                botToken = _botSection?.GetSection(BOT_TOKEN_KEY)?.Value;
                Properties.Settings.Default.BotToken = botToken;
            }

            return botToken.ToFSharpOption();
        }
        set
        {
            Properties.Settings.Default.BotToken = value.FromFSharpOption();
            Properties.Settings.Default.Save();
        }
    }

    public FSharpOption<string> MyChatId
    {
        get
        {
            string? myChatId = Properties.Settings.Default.MyChatId;
            if (string.IsNullOrWhiteSpace(myChatId))
            {
                myChatId = _botSection?.GetSection(MY_CHAT_ID_KEY)?.Value;
                Properties.Settings.Default.MyChatId = myChatId;
            }

            return myChatId.ToFSharpOption();
        }

        set
        {
            Properties.Settings.Default.MyChatId = value.FromFSharpOption();
            Properties.Settings.Default.Save();
        }
    }

    public FSharpOption<Work> CurrentWork
    {
        get
        {
            string currentWork = Properties.Settings.Default.CurrentWork;
            if (string.IsNullOrWhiteSpace(currentWork))
            {
                return FSharpOption<Work>.None;
            }

            try
            {
                Work work = JsonHelpers.Deserialize<Work>(currentWork);
                return FSharpOption<Work>.Some(work);
            }
            catch
            {
                Properties.Settings.Default.CurrentWork = null;
                return FSharpOption<Work>.None;
            }
        }
        set
        {
            string currentWork = JsonHelpers.Serialize(value);
            Properties.Settings.Default.CurrentWork = currentWork;
            Properties.Settings.Default.Save();
        }
    }

    public FSharpOption<DateOnlyPeriod> LastStatisticPeriod
    {
        get
        {
            string lastStatisticPeriod = Properties.Settings.Default.LastStatisticPeriod;
            if (string.IsNullOrWhiteSpace(lastStatisticPeriod))
            {
                return FSharpOption<DateOnlyPeriod>.None;
            }

            try
            {
                DateOnlyPeriod period = JsonHelpers.Deserialize<DateOnlyPeriod>(lastStatisticPeriod);
                return FSharpOption<DateOnlyPeriod>.Some(period);
            }
            catch
            {
                Properties.Settings.Default.LastStatisticPeriod = null;
                return FSharpOption<DateOnlyPeriod>.None;
            }
        }
        set
        {
            string lastStatisticPeriod = JsonHelpers.Serialize(value);
            Properties.Settings.Default.LastStatisticPeriod = lastStatisticPeriod;
            Properties.Settings.Default.Save();
        }
    }

    public string DatabaseFilePath
    {
        get
        {
            string databaseFilePath = Properties.Settings.Default.DatabaseFilePath;
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                databaseFilePath = _workDbOptions.DatabaseFilePath;
                Properties.Settings.Default.DatabaseFilePath = databaseFilePath;
            }

            return databaseFilePath;
        }
        set
        {
            Properties.Settings.Default.DatabaseFilePath = value;
            Properties.Settings.Default.Save();
        }
    }

    public bool Pooling
    {
        get
        {
            return Properties.Settings.Default.Pooling;
        }
    }

    [MaybeNull]
    public string CurrentVersion
    {
        get => Properties.Settings.Default.CurrentVersion;
        set
        {
            Properties.Settings.Default.CurrentVersion = value;
            Properties.Settings.Default.Save();
        }
    }

    /* TODO:
    public RollbackWorkStrategy RollbackWorkStrategy
    {
        get
        {
            var rollbackWorkStrategy = Properties.Settings.Default.RollbackWorkStrategy;
            if (string.IsNullOrWhiteSpace(rollbackWorkStrategy))
            {
                return RollbackWorkStrategy.UserChoiceIsRequired;
            }

            try
            {
                var strategy = JsonHelpers.Deserialize<RollbackWorkStrategy>(rollbackWorkStrategy);
                return strategy;
            }
            catch
            {
                Properties.Settings.Default.RollbackWorkStrategy = null;
                return RollbackWorkStrategy.UserChoiceIsRequired;
            }
        }
        set
        {
            var rollbackWorkStrategy = JsonHelpers.Serialize(value);
            Properties.Settings.Default.RollbackWorkStrategy = rollbackWorkStrategy;
            Properties.Settings.Default.Save();
        }
    }
    */

    public int LastDayCount
    {
        get => Properties.Settings.Default.LastDayCount;
        set
        {
            Properties.Settings.Default.LastDayCount = value;
            Properties.Settings.Default.Save();
        }
    }
}
