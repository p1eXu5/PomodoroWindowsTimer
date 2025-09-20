using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using PomodoroWindowsTimer.WpfClient.Converters;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Shared;
/// <summary>
/// Interaction logic for TimePointPrototypeV2.xaml
/// </summary>
public partial class TimePointPrototypeV2 : UserControl
{
    private static KindToTextConverter _kindToTextConverter;
    private static KindToBrushConverter _kindToBrushConverter;

    static TimePointPrototypeV2()
    {
        _kindToTextConverter = new KindToTextConverter();
        _kindToBrushConverter = new KindToBrushConverter();
    }

    public TimePointPrototypeV2()
    {
        InitializeComponent();

        SetBinding(BreakWorkLabelProperty, new Binding()
        {
            Source = this,
            Path = new PropertyPath(TimePointAssistant.TimePointKindProperty),
            Mode = BindingMode.OneWay,
            Converter = _kindToTextConverter,
        });

        SetBinding(BreakWorkBrushProperty, new Binding()
        {
            Source = this,
            Path = new PropertyPath(TimePointAssistant.TimePointKindProperty),
            Mode = BindingMode.OneWay,
            Converter = _kindToBrushConverter,
        });
    }

    public string BreakWorkLabel
    {
        get { return (string)GetValue(BreakWorkLabelProperty); }
        set { SetValue(BreakWorkLabelProperty, value); }
    }

    // Using a DependencyProperty as the backing store for BreakeWorkLabel.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BreakWorkLabelProperty =
        DependencyProperty.Register(
            "BreakWorkLabel",
            typeof(string),
            typeof(TimePointPrototypeV2),
            new PropertyMetadata(default));


    public Brush BreakWorkBrush
    {
        get { return (Brush)GetValue(BreakWorkBrushProperty); }
        set { SetValue(BreakWorkBrushProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BreakWorkBrushProperty =
        DependencyProperty.Register(
            "BreakWorkBrush", 
            typeof(Brush), 
            typeof(TimePointPrototypeV2), 
            new FrameworkPropertyMetadata(
                default, FrameworkPropertyMetadataOptions.AffectsRender));
}
