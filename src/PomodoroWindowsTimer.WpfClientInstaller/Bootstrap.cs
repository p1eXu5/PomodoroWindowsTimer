using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace PomodoroWindowsTimer.WpfClientInstaller;

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

        hostBuilder.ConfigureServices((ctx, services) =>
        {
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
                    BindingFlags.Instance
                    | BindingFlags.NonPublic
                    | BindingFlags.Public,
                    []
                )
                ?? throw new ArgumentException("TBootstrap type has no parameterless constructor.");

        return (TBootstrap)parameterlessCtor.Invoke(null);
    }

    public IHost Host { get; private set; } = default!;

    public void StartHost()
    {
        Host.Start();
    }

    public Task StopHostAsync() => Host.StopAsync();

    public void ShowMainWindow()
    {
        var window = Host.Services.GetRequiredService<Window>();
        window.Show();
    }

    protected virtual void ConfigureServices(HostBuilderContext hostBuilderCtx, IServiceCollection services)
    {
    }

    internal TimeProvider GetTimerProvider()
        => Host.Services.GetRequiredService<TimeProvider>();

    //internal IUserSettings GetUserSettings()
    //    => Host.Services.GetRequiredService<IUserSettings>();

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
