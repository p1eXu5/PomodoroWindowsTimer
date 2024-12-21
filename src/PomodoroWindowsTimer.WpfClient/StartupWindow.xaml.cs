using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace PomodoroWindowsTimer.WpfClient;
/// <summary>
/// Interaction logic for StartupWindow.xaml
/// </summary>
public partial class StartupWindow : Window
{
    private readonly IMainEntryPoint _mainEntryPoint;

    public StartupWindow(IMainEntryPoint mainEntryPoint)
    {
        InitializeComponent();
        _mainEntryPoint = mainEntryPoint;
        this.Loaded += OnLoaded;
        m_DatabaseSelector.DatabaseFilesChanged += StoreDatabaseFiles;
        m_DatabaseSelector.SelectedDatabaseFileChanged += StoreSelectedDatabase;
        m_DatabaseSelector.ApplyRequested += SeedAndMigrateDatabase;
        m_DatabaseSelector.CancelRequested += Exit;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        m_Spinner.Visibility = Visibility.Collapsed;
        m_DatabaseSelector.Visibility = Visibility.Visible;
    }

    private void Exit(object? sender, EventArgs e)
    {
        Exit();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Exit();
    }

    private void SeedAndMigrateDatabase(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void StoreSelectedDatabase(object? sender, string? e)
    {
        throw new NotImplementedException();
    }

    private void StoreDatabaseFiles(object? sender, IEnumerable<string>? e)
    {
        throw new NotImplementedException();
    }

    private void Exit()
    {
        m_DatabaseSelector.IsEnabled = false;
        _mainEntryPoint.Exit();
    }
}
