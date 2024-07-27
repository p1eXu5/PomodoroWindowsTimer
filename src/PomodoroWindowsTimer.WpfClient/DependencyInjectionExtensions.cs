using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer;
using PomodoroWindowsTimer.Types;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.ElmishApp.Infrastructure;
using PomodoroWindowsTimer.WpfClient;
using PomodoroWindowsTimer.WpfClient.Services;
using PomodoroWindowsTimer.TimePointQueue;
using System.Windows.Interop;
using PomodoroWindowsTimer.Exporter.Excel;
using PomodoroWindowsTimer.ElmishApp;

namespace DrugRoom.WpfClient;

internal static class DependencyInjectionExtensions
{
    public static void AddTimeProvider(this IServiceCollection services)
        => services.TryAddSingleton(TimeProvider.System);

    public static void AddTimePointQueue(this IServiceCollection services)
        => services.TryAddSingleton<ITimePointQueue>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TimePointQueue>>();
            return new TimePointQueue(logger);
        });

    public static void AddLooper(this IServiceCollection services)
        => services.TryAddSingleton<ILooper>(sp =>
        {
            var timePointQueue = sp.GetRequiredService<ITimePointQueue>();
            var timeProvider = sp.GetRequiredService<System.TimeProvider>();
            var logger = sp.GetRequiredService<ILogger<Looper.Looper>>();
            return
                new Looper.Looper(timePointQueue, timeProvider, PomodoroWindowsTimer.ElmishApp.Program.tickMilliseconds, logger, default);
        });

    public static void AddTelegramBot(this IServiceCollection services)
        => services.TryAddSingleton(sp =>
        {
            var userSettings = sp.GetRequiredService<IUserSettings>();
            return TelegramBot.init(userSettings);
        });

    public static void AddWindowsMinimizer(this IServiceCollection services)
        => services.TryAddSingleton<IWindowsMinimizer>(sp =>
        {
            var timeProvider = sp.GetRequiredService<System.TimeProvider>();
#if DEBUG
            return new StabWindowsMinimizer();
#else
            return new WindowsMinimizer(timeProvider);
#endif
        });

    public static void AddThemeSwitcher(this IServiceCollection services)
        => services.TryAddSingleton<IThemeSwitcher>(new ThemeSwitcher());

    /// <summary>
    /// Registers <see cref="ISettingsManager"/>.
    /// </summary>
    /// <param name="services"></param>
    public static void AddUserSettings(this IServiceCollection services, IConfiguration configuration)
        => services.TryAddSingleton<IUserSettings>(new UserSettings(configuration.GetSection("BotConfiguration")));

    public static void AddErrorMessageQueue(this IServiceCollection services)
    {
        Func<IServiceProvider, object?, IErrorMessageQueue> errorMessageQueueFactory = (sp, key) =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
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
