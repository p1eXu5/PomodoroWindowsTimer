using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.WpfClient;
using PomodoroWindowsTimer.WpfClient.Services;

namespace DrugRoom.WpfClient;

internal static class DependencyInjectionExtensions
{
    public static void AddTimeProvider(this IServiceCollection services)
        => services.TryAddSingleton(TimeProvider.System);

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

    public static void AddElmishProgramFactory(this IServiceCollection services)
        => services.TryAddTransient<ElmishProgramFactory>();
}
