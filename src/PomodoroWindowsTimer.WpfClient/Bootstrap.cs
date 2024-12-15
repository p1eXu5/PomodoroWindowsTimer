using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using CliWrap;
using CliWrap.Buffered;
using DrugRoom.WpfClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.Bootstrap;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.Storage;
using PomodoroWindowsTimer.Storage.Configuration;
using Serilog;
using Serilog.Formatting.Compact;

namespace PomodoroWindowsTimer.WpfClient;

internal class Bootstrap : BootstrapBase
{
    private bool _isDisposed;
    private bool _isInTest;

    protected Bootstrap()
    {
    }


    internal void WaitDbSeeding()
    {
        if (_isInTest)
        {
            var seederService = Host.Services.GetRequiredService<IHostedService>() as TestDbSeederHostedService;
            seederService!.Semaphore.Wait();
        }
        else
        {
            var seederService = Host.Services.GetRequiredService<IHostedService>() as DbSeederHostedService;
            seederService!.Semaphore.Wait();
        }
    }

    internal async Task ApplyMigrationsAsync()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        var connectionString = Host.Services.GetRequiredService<IUserSettings>().DatabaseFilePath;
        var eqInd = connectionString.IndexOf('=');
        var dbFilePath = connectionString.Substring(eqInd + 1, connectionString.Length - eqInd - 2);
        if (!Path.IsPathFullyQualified(dbFilePath))
        {
            connectionString = "Data Source=" + Path.Combine(path, dbFilePath) + ";";
        }

        var migratorPath =
            Path.Combine(path, "migrator", "PomodoroWindowsTimer.Migrator.exe");

        if (File.Exists(migratorPath))
        {
            var res = await Cli.Wrap(migratorPath)
                .WithArguments(["--connection", connectionString])
                .WithWorkingDirectory(Path.Combine(path, "migrator"))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
            ;

            var logger = GetLogger<Bootstrap>();
            if (!String.IsNullOrWhiteSpace(res.StandardError))
            {
                logger.LogError(res.StandardError);
            }
            else
            {
                logger.LogInformation(res.StandardOutput);
            }
        }
    }

    public void ShowMainWindow(Window window, Func<WorkStatisticWindow> workStatisticWindowFactory)
    {
        var elmishProgramFactory = GetElmishProgramFactory();
        elmishProgramFactory.RunElmishProgram(window, workStatisticWindowFactory);
        window.Show();

#if DEBUG
#else
        var mainWindowPtr = new WindowInteropHelper(window).Handle;
        elmishProgramFactory.WindowsMinimizer.AppWindowPtr = mainWindowPtr;
#endif
    }


    #region ServiceProvider_accessors

    internal IErrorMessageQueue GetMainWindowErrorMessageQueue()
        => Host.Services.GetRequiredKeyedService<IErrorMessageQueue>("main");

    internal IThemeSwitcher GetThemeSwitcher()
        => Host.Services.GetRequiredService<IThemeSwitcher>();

    internal ILooper GetLooper()
        => Host.Services.GetRequiredService<ILooper>();

    internal IWorkEventRepository GetWorkEventRepository()
        => Host.Services.GetRequiredService<IWorkEventRepository>();

    internal TimeProvider GetTimerProvider()
        => Host.Services.GetRequiredService<System.TimeProvider>();

    internal IUserSettings GetUserSettings()
        => Host.Services.GetRequiredService<IUserSettings>();

    internal ILogger<T> GetLogger<T>()
        => Host.Services.GetRequiredService<ILogger<T>>();

    protected ElmishProgramFactory GetElmishProgramFactory()
        => Host.Services.GetRequiredService<ElmishProgramFactory>();

    #endregion

    protected override void ConfigureServices(HostBuilderContext hostBuilderCtx, IServiceCollection services)
    {
        base.ConfigureServices(hostBuilderCtx, services);

        services.AddTimeProvider();
        services.AddTimePointQueue();
        services.AddLooper();
        services.AddTelegramBot();
        services.AddWindowsMinimizer();
        services.AddThemeSwitcher();
        services.AddUserSettings(hostBuilderCtx.Configuration);
        services.AddWorkEventStorage(hostBuilderCtx.Configuration);
        services.AddExcelBook();

        if (!hostBuilderCtx.Configuration.GetValue<bool>("InTest"))
        {
            services.AddErrorMessageQueue();
        }

        services.AddElmishProgramFactory();
    }

    protected virtual void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        // Clear default logging providers
        loggingBuilder.ClearProviders();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("Elmish.WPF.Update", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("Elmish.WPF.Performance", Serilog.Events.LogEventLevel.Error)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Destructure.ToMaximumDepth(4)
            .Destructure.ToMaximumStringLength(100)
            .Destructure.ToMaximumCollectionCount(10)
            .WriteTo.File(
                new CompactJsonFormatter(),
                "_logs/log.txt",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Add Serilog to the logging builder
        loggingBuilder.AddSerilog();
    }

    protected virtual void PostConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder
            .UseSerilog((context, conf) =>
                conf.ReadFrom.Configuration(context.Configuration)
            );
    }
}
