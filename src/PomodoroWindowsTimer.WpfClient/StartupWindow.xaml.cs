using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PomodoroWindowsTimer.Abstractions;

namespace PomodoroWindowsTimer.WpfClient;

/// <summary>
/// Interaction logic for StartupWindow.xaml
/// </summary>
public partial class StartupWindow : Window
{
    private readonly IMainEntryPoint _mainEntryPoint;
    private IUserSettings? _userSettings;
    private string? _databaseFilePath;
    private bool _isCritical;

    public StartupWindow(IMainEntryPoint mainEntryPoint)
    {
        InitializeComponent();
        _mainEntryPoint = mainEntryPoint;

        // this.Loaded += OnLoaded;
        m_DatabaseSelector.SelectedDatabaseFileChanged += StoreSelectedDatabaseFile;
        m_DatabaseSelector.ApplyRequested += OnApplyDatabaseFileRequested;
        m_DatabaseSelector.CancelRequested += OnCancelRequested;
    }

    internal void LoadDatabaseList(IUserSettings userSettings)
    {
        _userSettings = userSettings;

        var currentDbFile = userSettings.DatabaseFilePath;
        var recentDbFiles = userSettings.RecentDbFileList;
        
        if (
            recentDbFiles.Count == 0
            || (
                recentDbFiles.Count == 1
                && recentDbFiles.First().Equals(currentDbFile, StringComparison.Ordinal)
            )
        ) {
            LoadMainWindow();
            return;
        }

        m_DatabaseSelector.DatabaseFiles = new List<string> { currentDbFile }.Concat(recentDbFiles).Distinct();
        m_DatabaseSelector.SelectDatabaseFile(currentDbFile);
        m_Spinner.Visibility = Visibility.Collapsed;
        m_DatabaseSelector.Visibility = Visibility.Visible;
    }

    internal void ShowError(string error, bool isCritical)
    {
        _isCritical = isCritical;
        m_Spinner.Visibility = Visibility.Collapsed;
        m_DatabaseSelector.Visibility = Visibility.Collapsed;
        m_ErrorGrid.Visibility = Visibility.Visible;
        m_ErrorText.Text = error;
    }

    private void ErrorGridOkButtonClick(object sender, RoutedEventArgs e)
    {
        if (_isCritical)
        {
            DialogResult = true;
            return;
        }

        m_ErrorGrid.Visibility = Visibility.Collapsed;
        m_DatabaseSelector.Visibility = Visibility.Visible;
        m_DatabaseSelector.IsEnabled = true;
    }

    private void OnCancelRequested(object? sender, EventArgs e)
    {
        Shutdown();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Shutdown();
    }

    private void OnApplyDatabaseFileRequested(object? sender, EventArgs e)
    {
        _mainEntryPoint.WaitBootstrap();
        if (_userSettings is not null && !string.IsNullOrWhiteSpace(_databaseFilePath))
        {
            _userSettings.DatabaseFilePath = _databaseFilePath;

            LoadMainWindow();

            return;
        }

        // force log error:
        throw new InvalidOperationException("Database applying has been requested, " +
            "but user settings service has not been set or database file has not been choosen");
    }

    private void StoreSelectedDatabaseFile(object? sender, string? e)
    {
        _databaseFilePath = e;
    }

    private void Shutdown()
    {
        m_DatabaseSelector.IsEnabled = false;
        m_Spinner.Visibility = Visibility.Visible;
        _mainEntryPoint.Shutdown();
    }

    private void LoadMainWindow()
    {
        m_DatabaseSelector.IsEnabled = false;
        m_Spinner.Visibility = Visibility.Visible;
        _mainEntryPoint.LoadMainWindow();
    }
}
