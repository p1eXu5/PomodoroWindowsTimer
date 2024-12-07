using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.Wpf;
public class GlossRect : Control
{
    static GlossRect()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GlossRect), new FrameworkPropertyMetadata(typeof(GlossRect)));
    }

    public double? BaseRectWidth
    {
        get { return (double?)GetValue(BaseRectWidthProperty); }
        set { SetValue(BaseRectWidthProperty, value); }
    }

    // Using a DependencyProperty as the backing store for BaseRectWidth.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BaseRectWidthProperty =
        DependencyProperty.Register(
            "BaseRectWidth",
            typeof(double?),
            typeof(GlossRect),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );
}
