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

                    bool isPlaying = ((dynamic)_mainWindow.DataContext).IsPlaying;
                    object currentWork = ((dynamic)_mainWindow.DataContext).CurrentWork;

                    if (isPlaying && currentWork is not null)
                    {
                        UInt64 workId = ((dynamic)currentWork).Id;
                        var timeProvider = _bootstrap.GetTimerProvider();
                        var workEvent = WorkEvent.NewStopped(timeProvider.GetUtcNow());

                        var workEventRepository = _bootstrap.GetWorkEventRepository();
                        await workEventRepository.CreateAsync(workId, workEvent, default);
                    }

                    var userSettings = _bootstrap.GetUserSettings();
                    userSettings.LastStatisticPeriod = null;

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
