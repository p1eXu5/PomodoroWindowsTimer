﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.Types;
using PomodoroWindowsTimer.WpfClient.Services;

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
                if (startupWindow.ShowDialog() == true)
                {
                    Shutdown(666);
                }
            });
        });

        Task.Run(async () =>    
        {
            try
            {
                BootstrapApplication(e);
            }
            catch (Exception ex)
            {
                await Dispatcher.BeginInvoke(() =>
                {
#if DEBUG
                    startupWindow.ShowError(ex.Message, true);
#else
                    startupWindow.ShowError("Critical error!", true);
#endif
                });

                return;
            }

            await Dispatcher.BeginInvoke(() =>
            {
                startupWindow.LoadDatabaseList(_bootstrap.GetUserSettings());
            });

            mainEntryPoint.BootstrapMres.Set();

            while (true)
            {
                mainEntryPoint.MainEntryMres.Wait();

                if (mainEntryPoint.AppShutdownRequested)
                {
                    await Dispatcher.BeginInvoke(() =>
                    {
                        startupWindow.Close();
                        _logger.LogInformation("Shutting down...");
                        Shutdown();
                    });

                    break;
                }
                else
                {
                    var res = _bootstrap.TryUpdateDatabaseFile();
                    if (res.IsError)
                    {
                        mainEntryPoint.MainEntryMres.Reset();

                        await Dispatcher.BeginInvoke(() =>
                        {
                            startupWindow.ShowError(res.ErrorValue, false);
                        });

                        continue;
                    }

                    break;
                }
            }

            await Dispatcher.BeginInvoke(() =>
            {
                ShowMainWindow(startupWindow);
            });
        });
    }

    /// <summary>
    /// Builds and instantiates <see cref="_bootstrap"/>.
    /// </summary>
    /// <param name="e"></param>
    [MemberNotNull(nameof(_bootstrap), nameof(_logger))]
    private void BootstrapApplication(StartupEventArgs e)
    {
        _bootstrap = Bootstrap.Build<Bootstrap>(e.Args);
        _bootstrap.StartHost();

        _logger = _bootstrap.GetLogger<App>();
        _logger.LogInformation("Boostrapped.");
    }

    /// <summary>
    /// If <see cref="_bootstrap"/> is <see langword="null"/> then shutdowns application.
    /// Instantiates <see cref="MainWindow"/>, closes <paramref name="startupWindow"/> and
    /// shown <see cref="MainWindow"/> with Elmish bootstrapping.
    /// </summary>
    /// <param name="startupWindow"></param>
    private void ShowMainWindow(StartupWindow startupWindow)
    {
        if (_bootstrap is null)
        {
            _logger?.LogCritical("Application has not been bootstrapped! Shutting down...");
            Shutdown();

            return;
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
                    bool isCurrentWorkSet = ((dynamic)_mainWindow.DataContext).IsCurrentWorkSet;

                    try
                    {
                        if (isCurrentWorkSet)
                        {
                            object currentWork = ((dynamic)_mainWindow.DataContext).CurrentWork;
                            UInt64 workId = ((dynamic)currentWork).Id;

                            var repoFactory = _bootstrap.GetRepositoryFactory();
                            var work = await repoFactory.GetWorkRepository().FindByIdAsync(workId, default);

                            if (work.IsOk)
                            {
                                UserSettings.StoreCurrentWork(work.ResultValue!);
                            }

                            if (isPlaying)
                            {
                                // store work event
                                var timeProvider = _bootstrap.GetTimerProvider();
                                var workEvent = WorkEvent.NewStopped(timeProvider.GetUtcNow());
                                var workEventRepository = repoFactory.GetWorkEventRepository();
                                await workEventRepository.InsertAsync(workId, workEvent, default);
                            }
                        }
                    }
                    catch (RuntimeBinderException ex)
                    {
#if DEBUG
                        _logger?.LogError(ex, "Failed to get current work from data context.");
#else
                        _logger?.LogError("Failed to get current work from data context.");
#endif
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

        public SnackbarMessageQueue? MessageQueue { private get; set; }

        internal ManualResetEventSlim BootstrapMres { get; private set; } = new ManualResetEventSlim();

        internal ManualResetEventSlim MainEntryMres { get; private set; } = new ManualResetEventSlim();

        internal CancellationTokenSource Cts { get; private set; } = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        internal bool AppShutdownRequested { get; private set; }

        public void WaitBootstrap()
        {
            BootstrapMres.Wait();
        }

        public void Shutdown()
        {
            AppShutdownRequested = true;
            MainEntryMres.Set();
        }

        public void LoadMainWindow()
        {
            MainEntryMres.Set();
        }

        public void EnqueueError(string error)
        {
            MessageQueue?.Enqueue(
                error,
                "Clear",
                _ => MessageQueue?.Clear(),
                null,
                false,
                true,
                TimeSpan.FromSeconds(15)
            );
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
