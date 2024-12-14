using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace PomodoroWindowsTimer.Wpf;

[TemplatePart(Name = "PART_BlackPane", Type = typeof(Border))]
[TemplatePart(Name = "PART_InnerTopRightBorder", Type = typeof(Border))]
[TemplatePart(Name = "PART_InnerLeftBottomBorder", Type = typeof(Border))]
[TemplatePart(Name = "PART_OuterLeftBottomBorder", Type = typeof(Border))]
[TemplatePart(Name = "PART_Beam", Type = typeof(Beam))]
[TemplatePart(Name = "PART_OuterTopRightBorder", Type = typeof(Border))]
public class Lcd : ContentControl
{
    private static Color _lightEdge = Color.FromRgb(0x15, 0x15, 0x15);
    private static double _lightEdgeBrightness = CalculateBrightness(_lightEdge);

    private static Color _middleEdge = Color.FromRgb(0x6, 0x6, 0x6);
    private static double _middleEdgeBrightness = CalculateBrightness(_middleEdge);

    private static Color _darkEdge = Color.FromRgb(0x03, 0x03, 0x03);
    private static double _darkEdgeBrightness = CalculateBrightness(_darkEdge);

    /// <summary>
    /// Light edge brush.
    /// </summary>
    private static readonly SolidColorBrush _defaultInnerTopRightBorderBrush;
    private Border? _innerTopRightBorder;

    /// <summary>
    /// Dark edge brush.
    /// </summary>
    private static readonly SolidColorBrush _defaultInnerLeftBottomBorderBrush;
    private Border? _innerLeftBottomBorder;

    /// <summary>
    /// Light edge brush.
    /// </summary>
    private static readonly SolidColorBrush _defaultOuterLeftBottomBorderBrush;
    private Border? _outerLeftBottomBorder;

    /// <summary>
    /// Middle edge brush.
    /// </summary>
    private static readonly SolidColorBrush _defaultOuterTopRightBorderBrush;
    private Border? _outerTopRightBorder;

    static Lcd()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Lcd), new FrameworkPropertyMetadata(typeof(Lcd)));

        var lightEdgeBrush = GetSolidColorBrush(_lightEdge);

        _defaultInnerTopRightBorderBrush = lightEdgeBrush;
        _defaultInnerLeftBottomBorderBrush = GetSolidColorBrush(_darkEdge);
        _defaultOuterLeftBottomBorderBrush = lightEdgeBrush;
        _defaultOuterTopRightBorderBrush = GetSolidColorBrush(_middleEdge);
    }

    private static SolidColorBrush GetSolidColorBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();

        return brush;
    }

    private static double CalculateBrightness(Color color)
        => 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;

    private static Color AdjustToBrightness(Color color, double targetBrightness, double multiplier = 1.0)
    {
        double currentBrightness = CalculateBrightness(color);
        double brightnessRatio = targetBrightness / currentBrightness;

        var r = Clamp((int)(color.R * brightnessRatio * multiplier));
        var g = Clamp((int)(color.G * brightnessRatio * multiplier));
        var b = Clamp((int)(color.B * brightnessRatio * multiplier));

        return Color.FromRgb(r, g, b);
    }

    private static byte Clamp(int value)
        => (byte)Math.Clamp(value, 0, 0xFF);


    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _innerTopRightBorder = (Border)this.GetTemplateChild("PART_InnerTopRightBorder");
        ArgumentNullException.ThrowIfNull(_innerTopRightBorder);

        _innerLeftBottomBorder = (Border)this.GetTemplateChild("PART_InnerLeftBottomBorder");
        ArgumentNullException.ThrowIfNull(_innerLeftBottomBorder);

        _outerLeftBottomBorder = (Border)this.GetTemplateChild("PART_OuterLeftBottomBorder");
        ArgumentNullException.ThrowIfNull(_outerLeftBottomBorder);

        _outerTopRightBorder = (Border)this.GetTemplateChild("PART_OuterTopRightBorder");
        ArgumentNullException.ThrowIfNull(_outerTopRightBorder);

        SetTintLight(this, this.TintLightColor);
    }

    #region CornerRadius

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

    #endregion

    #region BeamOpacity

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

    #endregion

    #region Brightness

    public double Brightness
    {
        get { return (double)GetValue(BrightnessProperty); }
        set { SetValue(BrightnessProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Brightness.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BrightnessProperty =
        DependencyProperty.Register(
            "Brightness",
            typeof(double),
            typeof(Lcd),
            new FrameworkPropertyMetadata(
                1.0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnMultiplierChanged
            )
        );

    private static void OnMultiplierChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Lcd lcd || lcd._innerTopRightBorder is null)
        {
            return;
        }

        SetTintLight(lcd, lcd.TintLightColor);
    }

    #endregion

    #region TintLightColor

    public Color? TintLightColor
    {
        get { return (Color?)GetValue(TintLightColorProperty); }
        set { SetValue(TintLightColorProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TintLightColorProperty =
        DependencyProperty.Register(
            "TintLightColor",
            typeof(Color?),
            typeof(Lcd),
            new FrameworkPropertyMetadata(
                (Color?)null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnTintLightColorChanged
            )
        );

    private static void OnTintLightColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Lcd lcd || lcd._innerTopRightBorder is null)
        {
            return;
        }

        SetTintLight(lcd, e.NewValue as Color?);
    }

    private static void SetTintLight(Lcd lcd, Color? tintColor)
    {
        if (tintColor.HasValue)
        {
            SetColoredTintLight(lcd, tintColor.Value);
        }
        else
        {
            SetDefaultTintLight(lcd);
        }
    }

    private static void SetColoredTintLight(Lcd lcd, Color tintColor)
    {
        var multiplier = lcd.Brightness;

        var lightEdge = AdjustToBrightness(tintColor, _lightEdgeBrightness, multiplier);
        var middleEdge = AdjustToBrightness(tintColor, _middleEdgeBrightness, multiplier);
        var darkEdge = AdjustToBrightness(tintColor, _darkEdgeBrightness, multiplier);

        var lightEdgeBrush = GetSolidColorBrush(lightEdge);

        lcd._innerTopRightBorder!.BorderBrush = lightEdgeBrush;
        lcd._innerLeftBottomBorder!.BorderBrush = GetSolidColorBrush(darkEdge);
        lcd._outerLeftBottomBorder!.BorderBrush = lightEdgeBrush;
        lcd._outerTopRightBorder!.BorderBrush = GetSolidColorBrush(middleEdge);

        lcd.HighLightBrush = lightEdgeBrush;
    }

    private static void SetDefaultTintLight(Lcd lcd)
    {
        var multiplier = lcd.Brightness;

        if (multiplier == 1.0)
        {
            lcd._innerTopRightBorder!.BorderBrush = _defaultInnerTopRightBorderBrush;
            lcd._innerLeftBottomBorder!.BorderBrush = _defaultInnerLeftBottomBorderBrush;
            lcd._outerLeftBottomBorder!.BorderBrush = _defaultOuterLeftBottomBorderBrush;
            lcd._outerTopRightBorder!.BorderBrush = _defaultOuterTopRightBorderBrush;

            lcd.HighLightBrush = _defaultInnerTopRightBorderBrush;
        }
        else
        {
            var lightEdge = AdjustToBrightness(_lightEdge, _lightEdgeBrightness, multiplier);
            var middleEdge = AdjustToBrightness(_middleEdge, _middleEdgeBrightness, multiplier);
            var darkEdge = AdjustToBrightness(_darkEdge, _darkEdgeBrightness, multiplier);

            var lightEdgeBrush = GetSolidColorBrush(lightEdge);

            lcd._innerTopRightBorder!.BorderBrush = lightEdgeBrush;
            lcd._innerLeftBottomBorder!.BorderBrush = GetSolidColorBrush(darkEdge);
            lcd._outerLeftBottomBorder!.BorderBrush = lightEdgeBrush;
            lcd._outerTopRightBorder!.BorderBrush = GetSolidColorBrush(middleEdge);

            lcd.HighLightBrush = lightEdgeBrush;
        }
    }

    #endregion



    public Brush? TintLightBrush
    {
        get { return (Brush?)GetValue(TintLightBrushProperty); }
        set { SetValue(TintLightBrushProperty, value); }
    }

    // Using a DependencyProperty as the backing store for TintLightBrush.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TintLightBrushProperty =
        DependencyProperty.Register(
            "TintLightBrush",
            typeof(Brush),
            typeof(Lcd),
            new FrameworkPropertyMetadata((Brush?)null, FrameworkPropertyMetadataOptions.AffectsRender, OnTintLightBrushChanged));

    private static void OnTintLightBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Lcd lcd)
        {
            lcd.TintLightColor =
                e.NewValue is SolidColorBrush brush ? brush.Color : null;
        }
    }



    public Brush HighLightBrush
    {
        get { return (Brush)GetValue(HighLightBrushProperty); }
        set { SetValue(HighLightBrushProperty, value); }
    }

    // Using a DependencyProperty as the backing store for HighLightBrush.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HighLightBrushProperty =
        DependencyProperty.Register(
            "HighLightBrush", 
            typeof(Brush), 
            typeof(Lcd), 
            new PropertyMetadata(_defaultInnerTopRightBorderBrush)
        );
}
