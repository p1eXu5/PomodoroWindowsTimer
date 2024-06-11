using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

    private void m_Root_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (m_TitleTextBlock.TextWrapping == TextWrapping.Wrap)
        {
            m_TitleTextBlock.TextWrapping = TextWrapping.NoWrap;
        }
        else
        {
            m_TitleTextBlock.TextWrapping = TextWrapping.Wrap;
        }
    }
}
