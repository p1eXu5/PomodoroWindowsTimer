using System;
using System.Threading;
using System.Windows;
#if DEBUG
#else
using System.Windows.Interop;
#endif
using DrugRoom.WpfClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.Bootstrap;
using PomodoroWindowsTimer.ElmishApp;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.Storage;

namespace PomodoroWindowsTimer.WpfClient;

internal class Bootstrap : BootstrapBase
{
    #region public_methods

    internal FSharpResult<Unit, string> TryUpdateDatabaseFile()
    {
        var dbFileRevisor = GetDbFileRevisor();
        var userSettings = GetUserSettings();

        return dbFileRevisor.TryUpdateDatabaseFileAsync(userSettings, CancellationToken.None)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
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

    #endregion

    #region service_accessors

    internal IDbFileRevisor GetDbFileRevisor()
        => Host.Services.GetRequiredService<IDbFileRevisor>();

    internal IErrorMessageQueue GetMainWindowErrorMessageQueue()
        => Host.Services.GetRequiredKeyedService<IErrorMessageQueue>("main");

    internal IThemeSwitcher GetThemeSwitcher()
        => Host.Services.GetRequiredService<IThemeSwitcher>();

    internal ILooper GetLooper()
        => Host.Services.GetRequiredService<ILooper>();

    internal IWorkEventRepository GetWorkEventRepository()
        => Host.Services.GetRequiredService<IRepositoryFactory>().GetWorkEventRepository();

    internal IRepositoryFactory GetRepositoryFactory()
        => Host.Services.GetRequiredService<IRepositoryFactory>();

    internal IUserSettings GetUserSettings()
        => Host.Services.GetRequiredService<IUserSettings>();

    internal ILoggerFactory GetLoggerFactory()
        => Host.Services.GetRequiredService<ILoggerFactory>();

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
        services.AddTimePointStore();

        if (!hostBuilderCtx.Configuration.GetValue<bool>("InTest"))
        {
            services.AddErrorMessageQueue();
        }

        services.AddElmishProgramFactory();
    }

    #endregion
}
