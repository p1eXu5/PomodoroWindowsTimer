using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using CliWrap;
using CliWrap.Buffered;
using DrugRoom.WpfClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.Bootstrap;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.Storage;
using PomodoroWindowsTimer.ElmishApp;

namespace PomodoroWindowsTimer.WpfClient;

internal class Bootstrap : BootstrapBase
{
    #region public_methods

    //internal void WaitDbSeeding()
    //{
    //    if (IsInTest)
    //    {
    //        var seederService = Host.Services.GetRequiredService<IHostedService>() as TestDbSeederHostedService;
    //        seederService!.Semaphore.Wait();
    //    }
    //    else
    //    {
    //        var seederService = Host.Services.GetRequiredService<IHostedService>() as DbSeederHostedService;
    //        seederService!.Semaphore.Wait();
    //    }
    //}

    //internal async Task ApplyMigrationsAsync()
    //{
    //    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    //    var connectionString = Host.Services.GetRequiredService<IUserSettings>().DatabaseFilePath;
    //    var eqInd = connectionString.IndexOf('=');
    //    var dbFilePath = connectionString.Substring(eqInd + 1, connectionString.Length - eqInd - 2);
    //    if (!Path.IsPathFullyQualified(dbFilePath))
    //    {
    //        connectionString = "Data Source=" + Path.Combine(path, dbFilePath) + ";";
    //    }

    //    var migratorPath =
    //        Path.Combine(path, "migrator", "PomodoroWindowsTimer.Migrator.exe");

    //    if (File.Exists(migratorPath))
    //    {
    //        var res = await Cli.Wrap(migratorPath)
    //            .WithArguments(["--connection", connectionString])
    //            .WithWorkingDirectory(Path.Combine(path, "migrator"))
    //            .WithValidation(CommandResultValidation.None)
    //            .ExecuteBufferedAsync();
    //        ;

    //        var logger = GetLogger<Bootstrap>();
    //        if (!String.IsNullOrWhiteSpace(res.StandardError))
    //        {
    //            logger.LogError(res.StandardError);
    //        }
    //        else
    //        {
    //            logger.LogInformation(res.StandardOutput);
    //        }
    //    }
    //}

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

    #endregion

    #region service_accessors

    internal IErrorMessageQueue GetMainWindowErrorMessageQueue()
        => Host.Services.GetRequiredKeyedService<IErrorMessageQueue>("main");

    internal IThemeSwitcher GetThemeSwitcher()
        => Host.Services.GetRequiredService<IThemeSwitcher>();

    internal ILooper GetLooper()
        => Host.Services.GetRequiredService<ILooper>();

    internal IWorkEventRepository GetWorkEventRepository()
        => Host.Services.GetRequiredService<IWorkEventRepository>();

    internal IUserSettings GetUserSettings()
        => Host.Services.GetRequiredService<IUserSettings>();

    internal ILogger<T> GetLogger<T>()
        => Host.Services.GetRequiredService<ILogger<T>>();

    protected ElmishProgramFactory GetElmishProgramFactory()
        => Host.Services.GetRequiredService<ElmishProgramFactory>();

    #endregion

    #region overrides

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
        services.AddElmishAppServices();

        if (!hostBuilderCtx.Configuration.GetValue<bool>("InTest"))
        {
            services.AddErrorMessageQueue();
        }

        services.AddElmishProgramFactory();
    }

    #endregion
}
