﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

namespace PomodoroWindowsTimer.Bootstrap;

/// <summary>
/// Base bootstrap class for WPF clients.
/// </summary>
public abstract class BootstrapBase : IDisposable
{
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BootstrapBase"/> class.
    /// </summary>
    protected BootstrapBase()
    {
    }

    #region properties

    /// <inheritdoc cref="IHost"/>
    protected IHost Host { get; private set; } = default!;

    /// <summary>
    /// Indicates that the program running in test.
    /// </summary>
    protected bool IsInTest { get; private set; }

    #endregion

    #region IDisposable_implementation

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

    #region public_methods

    /// <summary>
    /// Adds configuration, registers services (<see cref="RegisterServices(IServiceCollection, IConfiguration)"/>)
    /// if <see cref="Bootstrap"/> instance has not been created. 
    /// </summary>
    public static TBootstrap Build<TBootstrap>(params string[] args)
        where TBootstrap : BootstrapBase
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

        TBootstrap bootstrap = (TBootstrap)parameterlessCtor.Invoke(null);

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

    public void StartHost()
    {
        Host.Start();
    }

    public Task StopHostAsync() => Host.StopAsync();

    #endregion

    #region ServiceProvider_accessors

    public TimeProvider GetTimerProvider()
       => Host.Services.GetRequiredService<System.TimeProvider>();

    #endregion

    #region protected_methods

    /// <summary>
    /// Does nothing. Introduced for test setup.
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="services"></param>
    protected virtual void PreConfigureServices(HostBuilderContext hostBuilder, IServiceCollection services)
    { }

    /// <summary>
    /// Configures services:
    /// <list type="number">
    /// <item>Adds <see cref="TimeProvider.System"/></item>
    /// <item>Sets <see cref="IsInTest"/> property.</item>
    /// </list>
    /// </summary>
    /// <param name="hostBuilderCtx"></param>
    /// <param name="services"></param>
    protected virtual void ConfigureServices(HostBuilderContext hostBuilderCtx, IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        if (hostBuilderCtx.Configuration.GetValue<bool>("InTest"))
        {
            IsInTest = true;
        }
    }

    /// <summary>
    /// Does nothind.
    /// </summary>
    /// <param name="loggingBuilder"></param>
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

    /// <summary>
    /// Sets Serilog using from configurations.
    /// </summary>
    /// <param name="hostBuilder"></param>
    protected virtual void PostConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder
            .UseSerilog((context, conf) =>
                conf.ReadFrom.Configuration(context.Configuration)
            );
    }

    #endregion
}
