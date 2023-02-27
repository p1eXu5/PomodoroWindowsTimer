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
        private readonly IErrorMessageQueue _errorMessageQueue;

        static App()
        {
            Configuration =
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true)
                    .Build();
        }

        public App()
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            Thread.CurrentThread.CurrentCulture = ci;

            _errorMessageQueue = new ErrorMessageQueue();



            AppDomain.CurrentDomain.UnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            this.Activated += StartElmish;
        }

        public static IConfiguration Configuration { get; }

        private void StartElmish(object? sender, EventArgs e)
        {
            this.Activated -= StartElmish;
            PomodoroWindowsTimer.ElmishApp.Program.main(MainWindow, _errorMessageQueue, SettingsManager.Instance, new BotConfiguration(SettingsManager.Instance));
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _errorMessageQueue.EnqueuError(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
            e.Handled = false;
        }

        private void OnDispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMessage = e.ExceptionObject.ToString() + Environment.NewLine;
            _errorMessageQueue.EnqueuError(errorMessage);
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            string errorMessage = e.Exception.InnerExceptions.First().Message + Environment.NewLine + e.Exception.GetType();
            _errorMessageQueue.EnqueuError(errorMessage);
        }
    }
}
