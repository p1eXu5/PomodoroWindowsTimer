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
using Elmish.WPF;
using MaterialDesignThemes.Wpf;
using PomodoroWindowsTimer.WpfClient.UserControls.Settings;

namespace PomodoroWindowsTimer.WpfClient.UserControls.MainScreen;

/// <summary>
/// Interaction logic for Timer.xaml
/// </summary>
public partial class MainScreen : UserControl
{
    public MainScreen()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Duplicate of Timer.DigitBackground. Could be moved to attached property of some assistant.
    /// </summary>
    public Brush DigitBackground
    {
        get { return (Brush)GetValue(DigitBackgroundProperty); }
        set { SetValue(DigitBackgroundProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DigitBackground.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DigitBackgroundProperty =
        DependencyProperty.Register("DigitBackground", typeof(Brush), typeof(MainScreen), new PropertyMetadata(null));

}
