using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using p1eXu5.CliBootstrap;
using p1eXu5.CliBootstrap.CommandLineParser;

namespace PomodoroWindowsTimer.Migrator;

internal sealed class PwtMigratorBootstrap : Bootstrap
{
    protected override ParsingResult ParseCommandLineArguments(string[] args)
    {
        return ArgsParser.Parse<PwtMigratorOptions>(args);
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ParsingResult parsingResult)
    {
        base.ConfigureServices(services, configuration, parsingResult);

        services.TryAddSingleton<IOptionsHandler, OptionsHandler>();
    }
}
