using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using PomodoroWindowsTimer.Wpf.Converters;
using PomodoroWindowsTimer.Wpf.Extensions;

namespace PomodoroWindowsTimer.Wpf;

[TemplatePart(Name = "PART_Rectangle", Type = typeof(Rectangle))]
public class Beam : Control
{
    private Rectangle? _rectangle;

    static Beam()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Beam), new FrameworkPropertyMetadata(typeof(Beam)));
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        LayoutUpdated += Beam_LayoutUpdated;
    }

    private void Beam_LayoutUpdated(object? sender, EventArgs e)
    {
        LayoutUpdated -= Beam_LayoutUpdated;
        GlossOnBase? glossOnBase = this.FindParent<GlossOnBase>();
        if (glossOnBase == null)
        {
            return;
        }

        SetBeamFillBrush(glossOnBase);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _rectangle = (Rectangle)this.GetTemplateChild("PART_Rectangle");
        ArgumentNullException.ThrowIfNull(_rectangle);

        SetRectangleRadiusXY(this, this.CornerRadius);
    }

    private void SetBeamFillBrush(GlossOnBase glossOnBase)
    {
        var visualBrushRectangle = new Rectangle();

        var actualWidthBinding = new Binding
        {
            Source = glossOnBase,
            Path = new PropertyPath(nameof(glossOnBase.ActualWidth)),
            Mode = BindingMode.OneWay,
        };
        BindingOperations.SetBinding(visualBrushRectangle, Rectangle.WidthProperty, actualWidthBinding);

        var actualHeightBinding = new Binding
        {
            Source = glossOnBase,
            Path = new PropertyPath(nameof(glossOnBase.ActualHeight)),
            Mode = BindingMode.OneWay,
        };
        BindingOperations.SetBinding(visualBrushRectangle, Rectangle.HeightProperty, actualHeightBinding);

        var visualBrushRectangleFillBinding = new Binding
        {
            Source = glossOnBase,
            Path = new PropertyPath(nameof(glossOnBase.BeamFill)),
            Mode = BindingMode.OneWay,
        };
        BindingOperations.SetBinding(visualBrushRectangle, Rectangle.FillProperty, visualBrushRectangleFillBinding);

        var visualBrush = new VisualBrush(visualBrushRectangle);

        var visualBrushScaleTransform = new ScaleTransform();
        var scaleXBinding = new MultiBinding
        {
            Converter = new ScaleConverter(),
            Mode = BindingMode.OneWay,
        };
        scaleXBinding.Bindings.Add(new Binding { Source = glossOnBase, Path = new PropertyPath(nameof(glossOnBase.ActualWidth)) });
        scaleXBinding.Bindings.Add(new Binding { Source = this, Path = new PropertyPath(nameof(this.ActualWidth)) });
        BindingOperations.SetBinding(visualBrushScaleTransform, ScaleTransform.ScaleXProperty, scaleXBinding);

        var scaleYBinding = new MultiBinding
        {
            Converter = new ScaleConverter(),
            Mode = BindingMode.OneWay,
        };
        scaleYBinding.Bindings.Add(new Binding { Source = glossOnBase, Path = new PropertyPath(nameof(glossOnBase.ActualHeight)) });
        scaleYBinding.Bindings.Add(new Binding { Source = this, Path = new PropertyPath(nameof(this.ActualHeight)) });
        BindingOperations.SetBinding(visualBrushScaleTransform, ScaleTransform.ScaleYProperty, scaleYBinding);

        var centerXBinding = new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(this.ActualWidth)),
            Mode = BindingMode.OneWay,
            Converter = new DividerConverter(),
            ConverterParameter = 2.0,
        };
        BindingOperations.SetBinding(visualBrushScaleTransform, ScaleTransform.CenterXProperty, centerXBinding);

        var centerYBinding = new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(this.ActualHeight)),
            Mode = BindingMode.OneWay,
            Converter = new DividerConverter(),
            ConverterParameter = 2.0,
        };
        BindingOperations.SetBinding(visualBrushScaleTransform, ScaleTransform.CenterYProperty, centerYBinding);


        Point beamPosition = this.TransformToAncestor(glossOnBase).Transform(new Point(0, 0));

        var visualBrushTranslateTransform = new TranslateTransform();
        var xBinding = new MultiBinding
        {
            Converter = new OffsetConverter(),
            Mode = BindingMode.OneWay,
            ConverterParameter = beamPosition.X / glossOnBase.ActualWidth
        };
        xBinding.Bindings.Add(new Binding { Source = glossOnBase, Path = new PropertyPath(nameof(glossOnBase.ActualWidth)) });
        xBinding.Bindings.Add(new Binding { Source = this, Path = new PropertyPath(nameof(glossOnBase.ActualWidth)) });
        BindingOperations.SetBinding(visualBrushTranslateTransform, TranslateTransform.XProperty, xBinding);

        var yBinding = new MultiBinding
        {
            Converter = new OffsetConverter(),
            Mode = BindingMode.OneWay,
            ConverterParameter = beamPosition.Y / glossOnBase.ActualHeight
        };
        yBinding.Bindings.Add(new Binding { Source = glossOnBase, Path = new PropertyPath(nameof(glossOnBase.ActualHeight)) });
        yBinding.Bindings.Add(new Binding { Source = this, Path = new PropertyPath(nameof(glossOnBase.ActualHeight)) });
        BindingOperations.SetBinding(visualBrushTranslateTransform, TranslateTransform.YProperty, yBinding);

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(visualBrushScaleTransform);
        transformGroup.Children.Add(visualBrushTranslateTransform);

        visualBrush.Transform = transformGroup;

        _rectangle.Fill = visualBrush;
    }

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(Beam),
            new FrameworkPropertyMetadata(
                new CornerRadius(),
                FrameworkPropertyMetadataOptions.AffectsRender,
                SetRectangleRadiuses)
        );

    private static void SetRectangleRadiuses(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Beam beam || beam._rectangle is null)
        {
            return;
        }

        SetRectangleRadiusXY(beam, (CornerRadius)e.NewValue);
    }

    private static void SetRectangleRadiusXY(Beam beam, CornerRadius cornerRadius)
    {
        beam._rectangle!.RadiusX = cornerRadius.TopLeft;
        beam._rectangle.RadiusY = cornerRadius.TopLeft;
    }
}
