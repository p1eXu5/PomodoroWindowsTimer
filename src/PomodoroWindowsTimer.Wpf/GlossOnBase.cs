using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroWindowsTimer.Wpf.Converters;

namespace PomodoroWindowsTimer.Wpf;

public class GlossOnBase : ContentControl
{
    static GlossOnBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GlossOnBase), new FrameworkPropertyMetadata(typeof(GlossOnBase)));
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        SetGlossOpacityMask();
        SetBeamFill();
    }

    private void SetGlossOpacityMask()
    {
        RadialGradientBrush radialGradient = new RadialGradientBrush();
        radialGradient.MappingMode = BrushMappingMode.Absolute;
        radialGradient.SpreadMethod = GradientSpreadMethod.Pad;
        var gradientStops = new GradientStopCollection();
        radialGradient.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0, 0, 0), Offset = 0, });
        radialGradient.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0, 0, 0, 0), Offset = 0.7, });

        var radiusBinding = new MultiBinding()
        {
            Converter = new MaxDividerConverter(),
            ConverterParameter = 1.5,
            Mode = BindingMode.OneWay,
        };
        radiusBinding.Bindings.Add(new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(this.ActualWidth))
        });
        radiusBinding.Bindings.Add(new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(this.ActualHeight))
        });
        BindingOperations.SetBinding(radialGradient, RadialGradientBrush.RadiusXProperty, radiusBinding);
        BindingOperations.SetBinding(radialGradient, RadialGradientBrush.RadiusYProperty, radiusBinding);

        var centerBinding = new MultiBinding()
        {
            Converter = new ActualSizeToCenterPointConverter(),
            Mode = BindingMode.OneWay,
        };
        centerBinding.Bindings.Add(new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(this.ActualWidth))
        });
        centerBinding.Bindings.Add(new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(this.ActualHeight))
        });
        BindingOperations.SetBinding(radialGradient, RadialGradientBrush.CenterProperty, centerBinding);

        var gradientOriginBinding = new MultiBinding()
        {
            Converter = new ActualSizeToOuterTopRightPointConverter(),
            Mode = BindingMode.OneWay,
        };
        gradientOriginBinding.Bindings.Add(new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(this.ActualWidth))
        });
        gradientOriginBinding.Bindings.Add(new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(this.ActualHeight))
        });
        BindingOperations.SetBinding(radialGradient, RadialGradientBrush.GradientOriginProperty, gradientOriginBinding);

        this.GlossOpacityMask = radialGradient;
    }

    private void SetBeamFill()
    {
        LinearGradientBrush linearGradient = new LinearGradientBrush();
        linearGradient.MappingMode = BrushMappingMode.Absolute;
        linearGradient.SpreadMethod = GradientSpreadMethod.Pad;
        linearGradient.StartPoint = new Point(0, 0);
        linearGradient.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF), Offset = 0.32, });
        linearGradient.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Offset = 0.34, });
        linearGradient.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Offset = 0.80, });
        linearGradient.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF), Offset = 0.82, });

        var endPointBinding = new Binding
        {
            Source = this,
            Converter = new ActualWidthToEndPointConverter(),
            Mode = BindingMode.OneWay,
            Path = new PropertyPath(nameof(this.ActualWidth)),
        };
        BindingOperations.SetBinding(linearGradient, LinearGradientBrush.EndPointProperty, endPointBinding);

        RotateTransform rotateTransform = new RotateTransform
        {
            Angle = 40,
        };
        var rotateTransformCenterXBinding = new Binding
        {
            Source = this,
            Converter = new DividerConverter(),
            ConverterParameter = 2.0,
            Mode = BindingMode.OneWay,
            Path = new PropertyPath(nameof(this.ActualWidth)),
        };
        BindingOperations.SetBinding(rotateTransform, RotateTransform.CenterXProperty, rotateTransformCenterXBinding);

        var rotateTransformCenterYBinding = new Binding
        {
            Source = this,
            Converter = new DividerConverter(),
            ConverterParameter = 2.0,
            Mode = BindingMode.OneWay,
            Path = new PropertyPath(nameof(this.ActualHeight)),
        };
        BindingOperations.SetBinding(rotateTransform, RotateTransform.CenterYProperty, rotateTransformCenterYBinding);

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(rotateTransform);

        linearGradient.Transform = transformGroup;

        this.BeamFill = linearGradient;
    }

    public double GlossRadiusScale
    {
        get { return (double)GetValue(GlossRadiusScaleProperty); }
        set { SetValue(GlossRadiusScaleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for GlossRadiusScale.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GlossRadiusScaleProperty =
        DependencyProperty.Register(
            "GlossRadiusScale",
            typeof(double),
            typeof(GlossOnBase),
            new FrameworkPropertyMetadata(
                1.0, 
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

    public Brush? BaseFill
    {
        get { return (Brush?)GetValue(BaseFillProperty); }
        set { SetValue(BaseFillProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BaseFillProperty =
        DependencyProperty.Register(
            "BaseFill",
            typeof(Brush),
            typeof(GlossOnBase),
            new FrameworkPropertyMetadata(
                (Brush?)null,
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

    public Brush? GlossFill
    {
        get { return (Brush?)GetValue(GlossFillProperty); }
        set { SetValue(GlossFillProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GlossFillProperty =
        DependencyProperty.Register(
            "GlossFill",
            typeof(Brush),
            typeof(GlossOnBase),
            new FrameworkPropertyMetadata(
                (Brush?)null,
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

    public Visibility BeamVisibility
    {
        get { return (Visibility)GetValue(BeamVisibilityProperty); }
        set { SetValue(BeamVisibilityProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ShowBeam.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BeamVisibilityProperty =
        DependencyProperty.Register(
            "BeamVisibility",
            typeof(Visibility),
            typeof(GlossOnBase),
            new FrameworkPropertyMetadata(
                Visibility.Hidden,
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

    public Brush? GlossOpacityMask
    {
        get { return (Brush?)GetValue(GlossOpacityMaskProperty); }
        set { SetValue(GlossOpacityMaskProperty, value); }
    }

    // Using a DependencyProperty as the backing store for GlossOpacityMask.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GlossOpacityMaskProperty =
        DependencyProperty.Register(
            "GlossOpacityMask",
            typeof(Brush),
            typeof(GlossOnBase),
            new FrameworkPropertyMetadata(
                (Brush?)null,
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );



    public Brush BeamFill
    {
        get { return (Brush)GetValue(BeamFillProperty); }
        set { SetValue(BeamFillProperty, value); }
    }

    // Using a DependencyProperty as the backing store for BeamFill.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BeamFillProperty =
        DependencyProperty.Register(
            "BeamFill",
            typeof(Brush),
            typeof(GlossOnBase),
            new FrameworkPropertyMetadata(
                (Brush?)null,
                FrameworkPropertyMetadataOptions.AffectsRender
            ));
}
