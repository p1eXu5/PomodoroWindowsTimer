using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.WpfClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IErrorMessageQueue? _errorMessageQueue = default!;
    private Bootstrap? _bootstrap = default!;
    private MainWindow? _mainWindow = default!;
    private ILogger<App>? _logger = default!;

    public App()
    {
        CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
        ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
        Thread.CurrentThread.CurrentCulture = ci;

        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        this.DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    public static new App Current => (App)Application.Current;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var mainEntryPoint = new MainEntryPoint();
        StartupWindow startupWindow = new(mainEntryPoint);

        Task.Run(() =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                startupWindow.Show();
            });
        });

        Task.Run(async () =>
        {
            BootstrapApplication(e);
            mainEntryPoint.BootstrapMres.Set();
            mainEntryPoint.MainEntryMres.Wait();

            if (mainEntryPoint.ExitRequested)
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    startupWindow.Close();
                    _logger.LogInformation("Shutting down...");
                    Shutdown();
                });
            }
            else
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    ShowMainWindow(startupWindow);
                });
            }
        });
    }

    [MemberNotNull(nameof(_bootstrap), nameof(_logger))]
    private void BootstrapApplication(StartupEventArgs e)
    {
        _bootstrap = Bootstrap.Build<Bootstrap>(e.Args);
        _bootstrap.StartHost();

        _logger = _bootstrap.GetLogger<App>();
        _logger.LogInformation("Boostrapped.");
    }

    private void ShowMainWindow(StartupWindow startupWindow)
    {
        if (_bootstrap is null)
        {
            _logger?.LogCritical("Application has not been bootstrapped! Shutting down...");
            Shutdown();
        }

        _mainWindow = new MainWindow();
        _errorMessageQueue = _bootstrap!.GetMainWindowErrorMessageQueue();
        _bootstrap.GetThemeSwitcher().SwitchTheme(ElmishApp.TimePointKind.Work);

        startupWindow.Close();
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

                if (_mainWindow?.DataContext is not null)
                {
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
        _errorMessageQueue?.EnqueueError(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
        e.Handled = false;
    }

    private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger?.LogError("Current domain unhandled exception. {ExceptionObject}", e.ExceptionObject);
        string errorMessage = e.ExceptionObject.ToString() + Environment.NewLine;
        _errorMessageQueue?.EnqueueError(errorMessage);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger?.LogError(e.Exception, "Unobserved task exception.");
        string errorMessage = e.Exception.InnerExceptions.First().Message + Environment.NewLine + e.Exception.GetType();
        _errorMessageQueue?.EnqueueError(errorMessage);
    }

    private sealed class MainEntryPoint : IMainEntryPoint, IDisposable
    {
        private bool _disposedValue;

        internal ManualResetEventSlim BootstrapMres { get; private set; } = new ManualResetEventSlim();

        internal ManualResetEventSlim MainEntryMres { get; private set; } = new ManualResetEventSlim();

        internal CancellationTokenSource Cts { get; private set; } = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        internal bool ExitRequested { get; private set; }

        public void WaitBootstrap()
        {
            BootstrapMres.Wait();
        }

        public void Exit()
        {
            ExitRequested = true;
            MainEntryMres.Set();
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                BootstrapMres.Dispose();
                MainEntryMres.Dispose();
                Cts.Dispose();
            }

            BootstrapMres = null!;
            MainEntryMres = null!;
            Cts = null!;

            _disposedValue = true;
        }
    }
}
