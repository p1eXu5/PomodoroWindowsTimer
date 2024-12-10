using System.Windows;

namespace PomodoroWindowsTimer.Wpf;

public static class GlossAssistant
{
    /// <summary>
    /// Property in inherited by elements and used by <see cref="Beam"/>
    /// </summary>
    public static readonly DependencyProperty BaseWidthProperty =
        DependencyProperty.RegisterAttached(
            "BaseWidth",
            typeof(double),
            typeof(GlossAssistant),
            new FrameworkPropertyMetadata(
                defaultValue: 800.0,
                flags: FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits)
    );

    // Declare a get accessor method.
    public static double GetBaseWidth(UIElement target) =>
        (double)target.GetValue(BaseWidthProperty);

    // Declare a set accessor method.
    public static void SetBaseWidth(UIElement target, double value) =>
        target.SetValue(BaseWidthProperty, value);

    /// <summary>
    /// Property in inherited by elements and used by <see cref="Beam"/>
    /// </summary>
    public static readonly DependencyProperty BaseHeightProperty =
        DependencyProperty.RegisterAttached(
            "BaseHeight",
            typeof(double),
            typeof(GlossAssistant),
            new FrameworkPropertyMetadata(
                defaultValue: 500.0,
                flags: FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits)
    );

    // Declare a get accessor method.
    public static double GetBaseHeight(UIElement target) =>
        (double)target.GetValue(BaseHeightProperty);

    // Declare a set accessor method.
    public static void SetBaseHeight(UIElement target, double value) =>
        target.SetValue(BaseHeightProperty, value);



    public static readonly DependencyProperty OffsetXProperty =
        DependencyProperty.RegisterAttached(
            "OffsetX",
            typeof(double?),
            typeof(GlossAssistant),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

    public static double? GetOffsetX(UIElement target) =>
        (double)target.GetValue(OffsetXProperty);

    // Declare a set accessor method.
    public static void SetOffsetX(UIElement target, double? value) =>
        target.SetValue(OffsetXProperty, value);


    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.RegisterAttached(
            "OffsetY",
            typeof(double?),
            typeof(GlossAssistant),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

    public static double? GetOffsetY(UIElement target) =>
        (double)target.GetValue(OffsetYProperty);

    // Declare a set accessor method.
    public static void SetOffsetY(UIElement target, double? value) =>
        target.SetValue(OffsetYProperty, value);
}
