using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using p1eXu5.CliBootstrap;
using p1eXu5.CliBootstrap.CommandLineParser;
using PomodoroWindowsTimer.Installer.Configuration;

namespace PomodoroWindowsTimer.AppAgent;

internal class Bootstrap : p1eXu5.CliBootstrap.Bootstrap
{
    protected override ParsingResult ParseCommandLineArguments(string[] args)
    {
        var result = ArgsParser.Parse<GetLastVersionVerb>(args);
        return result;
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ParsingResult parsingResult)
    {
        base.ConfigureServices(services, configuration, parsingResult);

        services.AddSingleton<IOptionsHandler, CliOptionsHandler>();
        services.AddInstallerServices();
    }
}
