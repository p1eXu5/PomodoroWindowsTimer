using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using DrugRoom.WpfClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using Serilog;

namespace PomodoroWindowsTimer.WpfClient;

internal class Bootstrap : IDisposable
{
    private bool _isDisposed;

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

    public void ShowMainWindow(Window window)
    {
        var elmishProgramFactory = GetElmishProgramFactory();
        elmishProgramFactory.RunElmishProgram(window);
        window.Show();
    }

    public virtual void StartElmishApp(Window mainWindow)
    {
        GetElmishProgramFactory().RunElmishProgram(mainWindow);
    }

    protected virtual void ConfigureServices(HostBuilderContext hostBuilderCtx, IServiceCollection services)
    {
        services.AddTimeProvider();
        services.AddTelegramBot();
        services.AddWindowsMinimizer();
        services.AddThemeSwitcher();
        services.AddUserSettings(hostBuilderCtx.Configuration);
        services.AddDb(hostBuilderCtx.Configuration);

        if (!hostBuilderCtx.Configuration.GetValue<bool>("InTest"))
        {
            services.AddErrorMessageQueue();
        }

        services.AddElmishProgramFactory();
    }


    internal IErrorMessageQueue GetMainWindowErrorMessageQueue()
        => Host.Services.GetRequiredKeyedService<IErrorMessageQueue>("main");

    protected ElmishProgramFactory GetElmishProgramFactory()
        => Host.Services.GetRequiredService<ElmishProgramFactory>();

    internal IThemeSwitcher GetThemeSwitcher()
        => Host.Services.GetRequiredService<IThemeSwitcher>();

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
