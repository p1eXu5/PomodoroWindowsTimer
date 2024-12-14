using System.Globalization;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace PomodoroWindowsTimer.WpfClientInstaller;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private Bootstrap _bootstrap = default!;
    private MainWindow _mainWindow = default!;
    private ILogger<App> _logger = default!;

    public App()
    {
        CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
        ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
        Thread.CurrentThread.CurrentCulture = ci;

        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        this.DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _bootstrap = Bootstrap.Build<Bootstrap>(e.Args);
        _bootstrap.StartHost();

        _logger = _bootstrap.GetLogger<App>();

        _bootstrap.ShowMainWindow();
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        if (_bootstrap is not null)
        {
            using (_bootstrap)
            {
                await _bootstrap.StopHostAsync();
            }
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        _logger?.LogError(e.Exception, "Dispatcher unhandled exception.");
        e.Handled = false;
    }

    private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger?.LogError("Current domain unhandled exception. {ExceptionObject}", e.ExceptionObject);
        string errorMessage = e.ExceptionObject.ToString() + Environment.NewLine;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger?.LogError(e.Exception, "Unobserved task exception.");
        string errorMessage = e.Exception.InnerExceptions.First().Message + Environment.NewLine + e.Exception.GetType();
    }
}

