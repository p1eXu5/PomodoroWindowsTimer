using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Shared;

/// <summary>
/// Interaction logic for TimePointV2.xaml
/// </summary>
public partial class TimePointV2 : UserControl
{
    public TimePointV2()
    {
        InitializeComponent();
    }

    public Brush? ButtonForeground
    {
        get { return (Brush?)GetValue(ButtonForegroundProperty); }
        set { SetValue(ButtonForegroundProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ButtonForeground.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ButtonForegroundProperty =
        DependencyProperty.Register("ButtonForeground", typeof(Brush), typeof(TimePointV2), new PropertyMetadata(defaultValue: null));



    public Brush? NameForeground
    {
        get { return (Brush?)GetValue(NameForegroundProperty); }
        set { SetValue(NameForegroundProperty, value); }
    }

    // Using a DependencyProperty as the backing store for NameForeground.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty NameForegroundProperty =
        DependencyProperty.Register("NameForeground", typeof(Brush), typeof(TimePointV2), new PropertyMetadata(defaultValue: null));



    public ControlTemplate ButtonIcon
    {
        get { return (ControlTemplate)GetValue(ButtonIconProperty); }
        set { SetValue(ButtonIconProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ButtonIcon.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ButtonIconProperty =
        DependencyProperty.Register("ButtonIcon", typeof(ControlTemplate), typeof(TimePointV2), new PropertyMetadata(defaultValue: null));



    public ICommand PlayStopCommand
    {
        get { return (ICommand)GetValue(PlayStopCommandProperty); }
        set { SetValue(PlayStopCommandProperty, value); }
    }

    // Using a DependencyProperty as the backing store for PlayStopCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PlayStopCommandProperty =
        DependencyProperty.Register("PlayStopCommand", typeof(ICommand), typeof(TimePointV2), new PropertyMetadata(defaultValue: null));



    public object PlayStopCommandParameter
    {
        get { return (object)GetValue(PlayStopCommandParameterProperty); }
        set { SetValue(PlayStopCommandParameterProperty, value); }
    }

    // Using a DependencyProperty as the backing store for PlayStopCommanParameter.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PlayStopCommandParameterProperty =
        DependencyProperty.Register("PlayStopCommandParameter", typeof(object), typeof(TimePointV2), new PropertyMetadata(defaultValue: null));
}
