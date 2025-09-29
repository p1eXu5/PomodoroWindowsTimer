using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.ElmishApp;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.ElmishApp.Infrastructure;
using PomodoroWindowsTimer.Exporter.Excel;
using PomodoroWindowsTimer.Looper;
using PomodoroWindowsTimer.Storage.Configuration;
using PomodoroWindowsTimer.TimePointQueue;
using PomodoroWindowsTimer.WpfClient;
using PomodoroWindowsTimer.WpfClient.Services;

namespace DrugRoom.WpfClient;

internal static class DependencyInjectionExtensions
{
    public static void AddTimeProvider(this IServiceCollection services)
        => services.TryAddSingleton(TimeProvider.System);

    public static void AddTimePointStore(this IServiceCollection services)
        => services.TryAddSingleton(sp =>
        {
            var tpSettings = sp.GetRequiredService<IUserSettings>();
            return TimePointStoreModule.Initialize(tpSettings);
        });

    public static void AddTimePointQueue(this IServiceCollection services)
        => services.TryAddSingleton<ITimePointQueue>(sp =>
        {
            var timePointStore = sp.GetRequiredService<TimePointStore>();
            var logger = sp.GetRequiredService<ILogger<TimePointQueue>>();
            return new TimePointQueue(timePointStore, logger, FSharpOption<int>.Some(2000), default);
        });

    public static void AddLooper(this IServiceCollection services)
        => services.TryAddSingleton<ILooper>(sp =>
        {
            ITimePointQueue timePointQueue = sp.GetRequiredService<ITimePointQueue>();
            TimeProvider timeProvider = sp.GetRequiredService<System.TimeProvider>();
            ILogger<Looper> logger = sp.GetRequiredService<ILogger<Looper>>();
            return
                new Looper(timePointQueue, timeProvider, Program.tickMilliseconds, logger, default);
        });

    public static void AddTelegramBot(this IServiceCollection services)
        => services.TryAddSingleton(sp =>
        {
            IUserSettings userSettings = sp.GetRequiredService<IUserSettings>();
            return TelegramBot.init(userSettings);
        });

    public static void AddWindowsMinimizer(this IServiceCollection services)
        => services.TryAddSingleton<IWindowsMinimizer>(sp =>
        {
            TimeProvider timeProvider = sp.GetRequiredService<System.TimeProvider>();
#if DEBUG
            return new StabWindowsMinimizer();
#else
            return new WindowsMinimizer(timeProvider);
#endif
        });

    public static void AddThemeSwitcher(this IServiceCollection services)
        => services.TryAddSingleton<IThemeSwitcher, ThemeSwitcher>();

    /// <summary>
    /// Registers <see cref="ISettingsManager"/>.
    /// </summary>
    /// <param name="services"></param>
    public static void AddUserSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IUserSettings>(sp =>
            new UserSettings(
                configuration.GetSection("BotConfiguration"),
                sp.GetRequiredService<IOptions<WorkDbOptions>>()
            )
        );

        services.TryAddSingleton<IDatabaseSettings>(sp =>
            sp.GetRequiredService<IUserSettings>()
        );

        services.TryAddSingleton<ITimePointSettings>(sp =>
            sp.GetRequiredService<IUserSettings>()
        );
    }

    public static void AddErrorMessageQueue(this IServiceCollection services)
    {
        Func<IServiceProvider, object?, IErrorMessageQueue> errorMessageQueueFactory = (sp, key) =>
        {
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            Microsoft.Extensions.Logging.ILogger logger;

            if (key is string skey)
            {
                logger = loggerFactory.CreateLogger(ErrorMessageQueue.TypeFullName + "." + skey);
            }
            else
            {
                logger = loggerFactory.CreateLogger<ErrorMessageQueue>();
            }

            return new ErrorMessageQueue(logger);
        };


        services.TryAddKeyedSingleton<IErrorMessageQueue>("main", errorMessageQueueFactory);
        services.TryAddKeyedSingleton<IErrorMessageQueue>("dialog", errorMessageQueueFactory);
    }

    public static void AddExcelBook(this IServiceCollection services)
        => services.TryAddSingleton<IExcelBook, ExcelBook>();

    public static void AddElmishProgramFactory(this IServiceCollection services)
        => services.TryAddTransient<ElmishProgramFactory>();
}
