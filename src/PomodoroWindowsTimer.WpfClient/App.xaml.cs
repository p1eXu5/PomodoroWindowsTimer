using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using Microsoft.Extensions.Configuration;
using PomodoroWindowsTimer.ElmishApp.Models;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.Types;
using PomodoroWindowsTimer.WpfClient.Properties;
using Microsoft.Extensions.Logging;

namespace PomodoroWindowsTimer.WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IErrorMessageQueue _errorMessageQueue = default!;
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

            _errorMessageQueue = _bootstrap.GetMainWindowErrorMessageQueue();
            var themeSwitcher = _bootstrap.GetThemeSwitcher();
            themeSwitcher.SwitchTheme(ElmishApp.TimePointKind.Work);

            _mainWindow = new MainWindow();
            _bootstrap.ShowMainWindow(_mainWindow, () => new WorkStatisticWindow());
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_bootstrap is not null)
            {
                using (_bootstrap)
                {
                    var looper = _bootstrap.GetLooper();
                    looper.Stop();

                    object player = ((dynamic)_mainWindow.DataContext).Player;
                    bool isPlaying = ((dynamic)player).IsPlaying;
                    object currentWork = ((dynamic)_mainWindow.DataContext).CurrentWork;

                    if (isPlaying && currentWork is not null)
                    {
                        UInt64 workId = ((dynamic)currentWork).Id;
                        var timeProvider = _bootstrap.GetTimerProvider();
                        var workEvent = WorkEvent.NewStopped(timeProvider.GetUtcNow());

                        var workEventRepository = _bootstrap.GetWorkEventRepository();
                        await workEventRepository.InsertAsync(workId, workEvent, default);
                    }

                    var userSettings = _bootstrap.GetUserSettings();
                    userSettings.LastStatisticPeriod = null;

                    await _bootstrap.StopHostAsync();
                }
            }
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Dispatcher unhandled exception.");
            _errorMessageQueue.EnqueueError(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
            e.Handled = false;
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger?.LogError("Current domain unhandled exception. {ExceptionObject}", e.ExceptionObject);
            string errorMessage = e.ExceptionObject.ToString() + Environment.NewLine;
            _errorMessageQueue.EnqueueError(errorMessage);
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Unobserved task exception.");
            string errorMessage = e.Exception.InnerExceptions.First().Message + Environment.NewLine + e.Exception.GetType();
            _errorMessageQueue.EnqueueError(errorMessage);
        }
    }
}
