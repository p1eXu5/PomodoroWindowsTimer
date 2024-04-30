using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.ElmishApp.Infrastructure;
using PomodoroWindowsTimer.WpfClient;
using PomodoroWindowsTimer.WpfClient.Services;
using PomodoroWindowsTimer.Storage;
using PomodoroWindowsTimer.WpfClient.Configuration;
using Microsoft.Extensions.Options;

namespace DrugRoom.WpfClient;

internal static class DependencyInjectionExtensions
{
    public static void AddDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WorkDbOptions>().Bind(configuration.GetSection("WorkDb")).ValidateDataAnnotations().ValidateOnStart();

        services.TryAddSingleton<IWorkRepository>(sp =>
        {
            // var logger = sp.GetRequiredService<ILogger<IWorkRepository>>
            var timeProvider = sp.GetRequiredService<System.TimeProvider>();
            
            using var scope = sp.CreateScope();
            var workDbOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<WorkDbOptions>>().Value;

            return
                Initializer.initWorkRepository(workDbOptions.ConnectionString, timeProvider);
        });

        services.TryAddSingleton<IWorkEventRepository>(sp =>
        {
            // var logger = sp.GetRequiredService<ILogger<IWorkRepository>>
            var timeProvider = sp.GetRequiredService<System.TimeProvider>();

            using var scope = sp.CreateScope();
            var workDbOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<WorkDbOptions>>().Value;

            return
                Initializer.initWorkEventRepository(workDbOptions.ConnectionString, timeProvider);
        });

        services.AddHostedService<DbSeederHostedService>();
    }

    public static void AddTimeProvider(this IServiceCollection services)
        => services.TryAddSingleton(TimeProvider.System);

    public static void AddTelegramBot(this IServiceCollection services)
        => services.TryAddSingleton(sp =>
        {
            var userSettings = sp.GetRequiredService<IUserSettings>();
            return TelegramBot.init(userSettings);
        });

    public static void AddWindowsMinimizer(this IServiceCollection services)
        => services.TryAddSingleton(sp =>
        {
#if DEBUG
            return WindowsMinimizer.initStub(PomodoroWindowsTimer.ElmishApp.Program.title);
#else
            return WindowsMinimizer.init(PomodoroWindowsTimer.ElmishApp.Program.title);
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

    public static void AddElmishProgramFactory(this IServiceCollection services)
        => services.TryAddTransient<ElmishProgramFactory>();
}
