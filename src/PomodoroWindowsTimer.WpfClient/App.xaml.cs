using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using Microsoft.Extensions.Configuration;

namespace PomodoroWindowsTimer.WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IErrorMessageQueue _errorMessageQueue = default!;
        private Bootstrap _bootstrap = default!;

        public App()
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            Thread.CurrentThread.CurrentCulture = ci;

            AppDomain.CurrentDomain.UnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _bootstrap = Bootstrap.Build<Bootstrap>(e.Args);
            _bootstrap.StartHost();

            _errorMessageQueue = _bootstrap.GetMainWindowErrorMessageQueue();
            var themeSwitcher = _bootstrap.GetThemeSwitcher();
            themeSwitcher.SwitchTheme(ElmishApp.TimePointKind.Work);

            var wnd = new MainWindow();
            _bootstrap.ShowMainWindow(wnd);
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

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _errorMessageQueue.EnqueueError(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
            e.Handled = false;
        }

        private void OnDispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMessage = e.ExceptionObject.ToString() + Environment.NewLine;
            _errorMessageQueue.EnqueueError(errorMessage);
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            string errorMessage = e.Exception.InnerExceptions.First().Message + Environment.NewLine + e.Exception.GetType();
            _errorMessageQueue.EnqueueError(errorMessage);
        }
    }
}
