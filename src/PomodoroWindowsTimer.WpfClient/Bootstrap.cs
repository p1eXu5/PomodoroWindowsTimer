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
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.Storage;
using PomodoroWindowsTimer.Storage.Configuration;
using Serilog;

namespace PomodoroWindowsTimer.WpfClient;

internal class Bootstrap : IDisposable
{
    private bool _isDisposed;
    private bool _isInTest;

    protected Bootstrap()
    {
    }

    # region =========== IDisposable implementation
    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Host.Dispose();
            }

            _isDisposed = true;
        }
    }
    #endregion

    /// <summary>
    /// Adds configuration, registers services (<see cref="RegisterServices(IServiceCollection, IConfiguration)"/>)
    /// if <see cref="Bootstrap"/> instance has not been created. 
    /// </summary>
    public static TBootstrap Build<TBootstrap>(params string[] args)
        where TBootstrap : Bootstrap
    {
        TBootstrap bootstrap = CreateBootstrapInstance<TBootstrap>();

        IHostBuilder hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

        hostBuilder.UseConsoleLifetime(opts =>
        {
            opts.SuppressStatusMessages = true;
        });
        
        hostBuilder.ConfigureServices((ctx, services) => {
            bootstrap.PreConfigureServices(ctx, services);
            bootstrap.ConfigureServices(ctx, services);
        });

        hostBuilder.ConfigureLogging(bootstrap.ConfigureLogging);

        bootstrap.PostConfigureHost(hostBuilder);

        IHost host = hostBuilder.Build();
        bootstrap.Host = host;

        return bootstrap;
    }

    private static TBootstrap CreateBootstrapInstance<TBootstrap>()
    {
        var parameterlessCtor =
            typeof(TBootstrap)
                .GetConstructor(
                    System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Public,
                    Array.Empty<Type>()
                );

        if (parameterlessCtor is null)
        {
            throw new ArgumentException("TBootstrap type has no parameterless constructor.");
        }

        return (TBootstrap)parameterlessCtor.Invoke(null);
    }

    public IHost Host { get; private set; } = default!;

    public void StartHost()
    {
        Host.Start();
    }

    public Task StopHostAsync() => Host.StopAsync();

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

    public virtual void StartElmishApp(Window mainWindow, Func<WorkStatisticWindow> workStatisticWindowFactory)
    {
        WaitDbSeeding();
        GetElmishProgramFactory().RunElmishProgram(mainWindow, workStatisticWindowFactory);
    }

    protected void WaitDbSeeding()
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

    protected virtual void ConfigureServices(HostBuilderContext hostBuilderCtx, IServiceCollection services)
    {
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
        else
        {
            _isInTest = true;
        }

        services.AddElmishProgramFactory();
    }


    internal IErrorMessageQueue GetMainWindowErrorMessageQueue()
        => Host.Services.GetRequiredKeyedService<IErrorMessageQueue>("main");

    protected ElmishProgramFactory GetElmishProgramFactory()
        => Host.Services.GetRequiredService<ElmishProgramFactory>();

    internal IThemeSwitcher GetThemeSwitcher()
        => Host.Services.GetRequiredService<IThemeSwitcher>();

    internal ILooper GetLooper()
        => Host.Services.GetRequiredService<ILooper>();

    internal IWorkEventRepository GetWorkEventRepository()
        => Host.Services.GetRequiredService<IWorkEventRepository>();

    internal System.TimeProvider GetTimerProvider()
        => Host.Services.GetRequiredService<System.TimeProvider>();

    internal IUserSettings GetUserSettings()
        => Host.Services.GetRequiredService<IUserSettings>();

    internal ILogger<T> GetLogger<T>()
        => Host.Services.GetRequiredService<ILogger<T>>();

    protected virtual void PreConfigureServices(HostBuilderContext hostBuilder, IServiceCollection services)
    { }

    protected virtual void ConfigureLogging(ILoggingBuilder loggingBuilder)
    { }

    protected virtual void PostConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder
            .UseSerilog((context, conf) =>
                conf.ReadFrom.Configuration(context.Configuration)
            );
    }
}
