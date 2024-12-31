using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Shared;

/// <summary>
/// Interaction logic for DatabaseSelector.xaml
/// </summary>
public partial class DatabaseSelector : UserControl
{
    public DatabaseSelector()
    {
        InitializeComponent();
    }

    public event EventHandler<string?>? SelectedDatabaseFileChanged;

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedDatabaseFileChanged?.Invoke(this, (string?)m_ComboBox.SelectedItem);
    }


    #region DatabaseFilesProperty

    public event EventHandler<IEnumerable<string>?>? DatabaseFilesChanged;

    public IEnumerable<string>? DatabaseFiles
    {
        get { return (IEnumerable<string>?)GetValue(DatabaseFilesProperty); }
        set { SetValue(DatabaseFilesProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DatabaseFilesProperty =
        DependencyProperty.Register(
            "DatabaseFiles",
            typeof(IEnumerable<string>),
            typeof(DatabaseSelector),
            new PropertyMetadata((IEnumerable<string>?)null, OnDatabaseFilesPropertyChanged));

    private static void OnDatabaseFilesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseSelector databaseSelector)
        {
            databaseSelector.OnDatabaseFilesChanged(e);
        }
    }

    private void OnDatabaseFilesChanged(DependencyPropertyChangedEventArgs e)
    {
        DatabaseFilesChanged?.Invoke(this, (IEnumerable<string>?)e.NewValue);
    }

    #endregion

    #region SelectDatabaseFileCommandProperty

    public ICommand? SelectDatabaseFileCommand
    {
        get { return (ICommand?)GetValue(SelectDatabaseFileCommandProperty); }
        set { SetValue(SelectDatabaseFileCommandProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SelectDatabaseFileCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SelectDatabaseFileCommandProperty =
        DependencyProperty.Register(
            "SelectDatabaseFileCommand",
            typeof(ICommand),
            typeof(DatabaseSelector),
            new PropertyMetadata((ICommand?)null));


    private void OpenSelectDatabaseFileDialogButton_Click(object sender, RoutedEventArgs e)
    {
        var cmd = SelectDatabaseFileCommand;
        if (cmd is not null)
        {
            if (cmd.CanExecute(null))
            {
                cmd.Execute(null);
            }

            e.Handled = true;
            return;
        }

        var openFileDialog = new VistaOpenFileDialog
        {
            Filter = "Database Files (*.db)|*.db",
            Title = "Select a Database File"
        };
        var result = openFileDialog.ShowDialog();

        if (result == true
            && !String.IsNullOrWhiteSpace(openFileDialog.FileName)
            && openFileDialog.FileName.Length >= 16
        )
        {
            var databaseFiles = DatabaseFiles;

            if (databaseFiles is null || !databaseFiles.Contains(openFileDialog.FileName) == true)
            {
                DatabaseFiles = (databaseFiles ?? Enumerable.Empty<string>()).Append(openFileDialog.FileName).ToList();
                m_ComboBox.SelectedItem = openFileDialog.FileName;
            }
        }

        e.Handled = true;
    }

    #endregion

    internal void SelectDatabaseFile(string dbFilePath)
    {
        if (DatabaseFiles?.Any() == true && DatabaseFiles.Contains(dbFilePath))
        {
            m_ComboBox.SelectedItem = dbFilePath;
        }
    }

    public ICommand? ApplyCommand
    {
        get { return (ICommand?)GetValue(ApplyCommandProperty); }
        set { SetValue(ApplyCommandProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SelectDatabaseFileCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ApplyCommandProperty =
        DependencyProperty.Register(
            "ApplyCommand",
            typeof(ICommand),
            typeof(DatabaseSelector),
            new PropertyMetadata((ICommand?)null));



    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        var cmd = ApplyCommand;
        if (cmd is not null)
        {
            if (cmd.CanExecute(null))
            {
                cmd.Execute(null);
            }

            e.Handled = true;
            return;
        }

        OnApplyRequested();
        e.Handled = true;
    }

    public event EventHandler? ApplyRequested;

    private void OnApplyRequested()
    {
        ApplyRequested?.Invoke(this, new());
    }


    public ICommand? CancelCommand
    {
        get { return (ICommand?)GetValue(CancelCommandProperty); }
        set { SetValue(CancelCommandProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SelectDatabaseFileCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.Register(
            "CancelCommand",
            typeof(ICommand),
            typeof(DatabaseSelector),
            new PropertyMetadata((ICommand?)null));

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        var cmd = CancelCommand;
        if (cmd is not null)
        {
            if (cmd.CanExecute(null))
            {
                cmd.Execute(null);
            }

            e.Handled = true;
            return;
        }

        OnCancelRequested();
        e.Handled = true;
    }

    public event EventHandler? CancelRequested;

    private void OnCancelRequested()
    {
        CancelRequested?.Invoke(this, new());
    }



    public string ApplyButtonCaption
    {
        get { return (string)GetValue(ApplyButtonCaptionProperty); }
        set { SetValue(ApplyButtonCaptionProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ApplyButtonCaption.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ApplyButtonCaptionProperty =
        DependencyProperty.Register("ApplyButtonCaption", typeof(string), typeof(DatabaseSelector), new PropertyMetadata("APPLY"));



    public string CancelButtonCaption
    {
        get { return (string)GetValue(CancelButtonCaptionProperty); }
        set { SetValue(CancelButtonCaptionProperty, value); }
    }

    // Using a DependencyProperty as the backing store for CancelButtonCaption.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CancelButtonCaptionProperty =
        DependencyProperty.Register("CancelButtonCaption", typeof(string), typeof(DatabaseSelector), new PropertyMetadata("CANCEL"));



    public bool ShowCloseButton
    {
        get { return (bool)GetValue(ShowCloseButtonProperty); }
        set { SetValue(ShowCloseButtonProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ShowCloseButton.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ShowCloseButtonProperty =
        DependencyProperty.Register(
            "ShowCloseButton",
            typeof(bool),
            typeof(DatabaseSelector),
            new PropertyMetadata(true, OnShowCloseButtonPropertyChanged));

    private static void OnShowCloseButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseSelector databaseSelector)
        {
            if (databaseSelector.ShowCloseButton)
            {
                databaseSelector.m_CloseButton.Visibility = Visibility.Visible;
            }
            else
            {
                databaseSelector.m_CloseButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
