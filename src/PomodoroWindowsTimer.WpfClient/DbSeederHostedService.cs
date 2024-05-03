using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PomodoroWindowsTimer.Storage;
using PomodoroWindowsTimer.WpfClient.Configuration;

namespace PomodoroWindowsTimer.WpfClient;

internal sealed class DbSeederHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public DbSeederHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(0, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<WorkDbOptions>>().Value;
        await Initializer.initdb(options.ConnectionString);

        SemaphoreSlim.Release();
    }

    public override void Dispose()
    {
        SemaphoreSlim.Dispose();
        base.Dispose();
    }
}
