using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

/// <summary>
/// Interaction logic for SelectableWork.xaml
/// </summary>
public partial class SelectableWork : UserControl
{
    public SelectableWork()
    {
        InitializeComponent();
    }

    public bool IsSelected
    {
        get { return (bool)GetValue(IsSelectedProperty); }
        set { SetValue(IsSelectedProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(SelectableWork),
            new PropertyMetadata(false));
}
