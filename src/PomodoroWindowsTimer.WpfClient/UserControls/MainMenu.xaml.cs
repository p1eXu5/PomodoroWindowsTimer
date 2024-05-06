using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PomodoroWindowsTimer.WpfClient.UserControls;
/// <summary>
/// Interaction logic for MainMenu.xaml
/// </summary>
public partial class MainMenu : UserControl
{
    public MainMenu()
    {
        InitializeComponent();
    }

    private void OpenBotSettings(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
    {
        if (DataContext is null)
        {
            return;
        }

        object appDialog = ((dynamic)DataContext).AppDialog;

        if (appDialog is null)
        {
            return;
        }

        ICommand command = ((dynamic)appDialog).OpenBotSettingsDialogCommand;
        if (command != null && command.CanExecute(null))
        {
            command.Execute(null);
        }
    }

    private void CloseBotSettings(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
    {
        if (DataContext is null)
        {
            return;
        }

        object appDialog = ((dynamic)DataContext).AppDialog;

        if (appDialog is null)
        {
            return;
        }

        ICommand command = ((dynamic)appDialog).OpenBotSettingsDialogCommand;
        if (command != null && command.CanExecute(null))
        {
            command.Execute(null);
        }
    }
}
