using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.Wpf;

[TemplatePart(Name = "PART_Beam", Type = typeof(Beam))]
public class Lcd : ContentControl
{
    static Lcd()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Lcd), new FrameworkPropertyMetadata(typeof(Lcd)));
    }

    public double? BaseWidth
    {
        get { return (double?)GetValue(BaseWidthProperty); }
        set { SetValue(BaseWidthProperty, value); }
    }

    // Using a DependencyProperty as the backing store for BaseRectWidth.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BaseWidthProperty =
        DependencyProperty.Register(
            "BaseWidth",
            typeof(double?),
            typeof(Lcd),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }

    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(Lcd),
            new FrameworkPropertyMetadata(
                new CornerRadius(),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );



    public double BeamOpacity
    {
        get { return (double)GetValue(BeamOpacityProperty); }
        set { SetValue(BeamOpacityProperty, value); }
    }

    // Using a DependencyProperty as the backing store for BeamOpacity.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BeamOpacityProperty =
        DependencyProperty.Register(
            "BeamOpacity",
            typeof(double),
            typeof(Lcd),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));


}
