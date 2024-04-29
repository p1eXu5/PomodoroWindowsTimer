using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<WorkDbOptions>>().Value;
        return Initializer.initdb(options.ConnectionString);
    }
}
