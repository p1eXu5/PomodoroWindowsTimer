﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Templates;

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

    /// <summary>
    /// Disposes <see cref="Host"/>.
    /// </summary>
    /// <param name="disposing"></param>
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
    /// <list type="number">
    /// <item> Instantiates <typeparamref name="TBootstrap"/> </item>
    /// <item> Instantiates default <see cref="IHostBuilder"/> </item>
    /// <item> Suppresses status messages (<see cref="ConsoleLifetimeOptions.SuppressStatusMessages"/>) </item>
    /// <item> Invokes <see cref="ConfigureLogging(HostBuilderContext, ILoggingBuilder)"/> </item>
    /// <item> Invokes <see cref="ConfigureServices(HostBuilderContext, IServiceCollection)"/> </item>
    /// <item> Invokes <see cref="PostConfigureHost(IHostBuilder)"/> </item>
    /// <item> Built <see cref="IHost"/> </item>
    /// <item> Sets <see cref="BootstrapBase.Host"/> </item>
    /// </list> 
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
                    []
                )
                ?? throw new ArgumentException("TBootstrap type has no parameterless constructor.");

        TBootstrap bootstrap = (TBootstrap)parameterlessCtor.Invoke(null);

        IHostBuilder hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

        hostBuilder.UseConsoleLifetime(opts =>
        {
            opts.SuppressStatusMessages = true;
        });

        hostBuilder.ConfigureLogging(bootstrap.ConfigureLogging);

        hostBuilder.ConfigureServices((ctx, services) =>
        {
            bootstrap.PreConfigureServices(ctx, services);
            bootstrap.ConfigureServices(ctx, services);
        });

        bootstrap.PostConfigureHost(hostBuilder);

        IHost host = hostBuilder.Build();
        bootstrap.Host = host;

        return bootstrap;
    }

    /// <summary>
    /// Calls <see cref="Host"/> <see cref="HostingAbstractionsHostExtensions.Start(IHost)"/>.
    /// </summary>
    public virtual void StartHost()
    {
        Host.Start();
    }

    /// <summary>
    /// Invokes <see cref="IHost.StopAsync(CancellationToken)"/>.
    /// </summary>
    /// <returns></returns>
    public Task StopHostAsync() => Host.StopAsync();

    #region service accessors: GetTimerProvider

    /// <summary>
    /// Obtains <see cref="System.TimeProvider"/> from <see cref="IHost.Services"/>.
    /// </summary>
    /// <returns></returns>
    public TimeProvider GetTimerProvider()
       => Host.Services.GetRequiredService<System.TimeProvider>();

    #endregion

    #region protected methods: PreConfigureServices, ConfigureServices, ConfigureLogging, PostConfigureHost

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
    /// Configures <see cref="Serilog.ILogger"/>.
    /// </summary>
    protected virtual void ConfigureLogging(HostBuilderContext hostBuilderContext, ILoggingBuilder loggingBuilder)
    {
        // Configure Serilog
        var cfg = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Destructure.ToMaximumDepth(4)
            .Destructure.ToMaximumStringLength(100)
            .Destructure.ToMaximumCollectionCount(10)
            ;

        if (!hostBuilderContext.HostingEnvironment.IsDevelopment())
        {
            cfg = cfg
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Elmish.WPF.Update", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Elmish.WPF.Bindings", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Elmish.WPF.Performance", Serilog.Events.LogEventLevel.Warning);
        }
        else
        {
            cfg = cfg
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("Elmish.WPF.Update", Serilog.Events.LogEventLevel.Verbose)
                .MinimumLevel.Override("Elmish.WPF.Bindings", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Elmish.WPF.Performance", Serilog.Events.LogEventLevel.Warning);
        }

        if (!hostBuilderContext.HostingEnvironment.IsDevelopment())
        {
            cfg = cfg
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    "_logs/log.txt",
                    rollingInterval: RollingInterval.Day);
        }
        else
        {
            string template = "{@t:yyyy-MM-dd HH:mm:ss} [{@l:u3}] {Coalesce(SourceContext, '<none>')}: {Method}\n    {@m}\n{@x}\n";

            cfg = cfg
                .WriteTo.Console(
                    formatter: new ExpressionTemplate(
                        template,
                        theme: Serilog.Templates.Themes.TemplateTheme.Code
                    )
                )
                .WriteTo.Debug(
                    formatter: new ExpressionTemplate(
                        template
                    )
                );
        }

        Log.Logger = cfg.CreateLogger();

        // Add Serilog to the logging builder
        loggingBuilder
            .ClearProviders()
            .AddSerilog(Log.Logger, dispose: true);
    }

    /// <summary>
    /// Does nothing. Invoked in <see cref="Build{TBootstrap}(string[])"/> right before
    /// <see cref="IHostBuilder.Build"/> is called.
    /// <para>
    /// Using in tests.
    /// </para>
    /// </summary>
    protected virtual void PostConfigureHost(IHostBuilder hostBuilder)
    {
    }

    #endregion
}
