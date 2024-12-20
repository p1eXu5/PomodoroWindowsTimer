using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PomodoroWindowsTimer.Wpf;

[TemplatePart(Name = "PART_FigureN", Type = typeof(Path))]
[TemplatePart(Name = "PART_FigureNE", Type = typeof(Path))]
[TemplatePart(Name = "PART_FigureE", Type = typeof(Path))]
[TemplatePart(Name = "PART_FigureSE", Type = typeof(Path))]
[TemplatePart(Name = "PART_FigureS", Type = typeof(Path))]
[TemplatePart(Name = "PART_FigureSW", Type = typeof(Path))]
[TemplatePart(Name = "PART_FigureW", Type = typeof(Path))]
[TemplatePart(Name = "PART_FigureNW", Type = typeof(Path))]
public class BlackMirrorSpinner : Control
{
    private const int N = 8;
    private const float ANGLE = 360 / 8f; // 45.0f
    private const float HALF_ANGLE = ANGLE / 2.0f; // 22.5f

    private const float ANGLE_OFFSET1 = 2.5f;
    private const float ANGLE_OFFSET2 = ANGLE_OFFSET1 + 2.5f;
    private const float ANGLE_OFFSET3 = ANGLE_OFFSET2 + 0.5f;
    
    private const float K = (float)(Math.PI / 180);
    private const float FIGURE_HEIGHT_RATIO = 0.3f;
    private const float FIGURE_TOP_CHAMFER_RATIO = 4f;
    private const float FIGURE_BOTTOM_CHAMFER_RATIO = 5.7f;

    private PathCollection? _pathCollection;

    static BlackMirrorSpinner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BlackMirrorSpinner), new FrameworkPropertyMetadata(typeof(BlackMirrorSpinner)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var figureN = GetTemplateChild("PART_FigureN") as Path;
        var figureNE = GetTemplateChild("PART_FigureNE") as Path;
        var figureE = GetTemplateChild("PART_FigureE") as Path;
        var figureSE = GetTemplateChild("PART_FigureSE") as Path;
        var figureS = GetTemplateChild("PART_FigureS") as Path;
        var figureSW = GetTemplateChild("PART_FigureSW") as Path;
        var figureW = GetTemplateChild("PART_FigureW") as Path;
        var figureNW = GetTemplateChild("PART_FigureNW") as Path;

        if (figureN is not null
            && figureNE is not null
            && figureE is not null
            && figureSE is not null
            && figureS is not null
            && figureSW is not null
            && figureW is not null
            && figureNW is not null)
        {
            _pathCollection = new PathCollection(
                figureN,
                figureNE,
                figureE,
                figureSE,
                figureS,
                figureSW,
                figureW,
                figureNW);

            UpdateFigures6(_pathCollection, (float)Radius);
        }
    }

    public Brush? FillInactive
    {
        get { return (Brush?)GetValue(FillInactiveProperty); }
        set { SetValue(FillInactiveProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FillInactiveProperty =
        DependencyProperty.Register(
            "FillInactive",
            typeof(Brush),
            typeof(BlackMirrorSpinner),
            new FrameworkPropertyMetadata((Brush?)null, FrameworkPropertyMetadataOptions.AffectsRender));


    public Brush? FillActive
    {
        get { return (Brush?)GetValue(FillActiveProperty); }
        set { SetValue(FillActiveProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FillActiveProperty =
        DependencyProperty.Register(
            "FillActive",
            typeof(Brush),
            typeof(BlackMirrorSpinner),
            new FrameworkPropertyMetadata((Brush?)null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush? StrokeInactive
    {
        get { return (Brush?)GetValue(StrokeInactiveProperty); }
        set { SetValue(StrokeInactiveProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty StrokeInactiveProperty =
        DependencyProperty.Register(
            "StrokeInactive",
            typeof(Brush),
            typeof(BlackMirrorSpinner),
            new FrameworkPropertyMetadata((Brush?)null, FrameworkPropertyMetadataOptions.AffectsRender));


    public Brush? StrokeActive
    {
        get { return (Brush?)GetValue(StrokeActiveProperty); }
        set { SetValue(StrokeActiveProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty StrokeActiveProperty =
        DependencyProperty.Register(
            "StrokeActive",
            typeof(Brush),
            typeof(BlackMirrorSpinner),
            new FrameworkPropertyMetadata((Brush?)null, FrameworkPropertyMetadataOptions.AffectsRender));


    public Visibility LabelVisibility
    {
        get { return (Visibility)GetValue(LabelVisibilityProperty); }
        set { SetValue(LabelVisibilityProperty, value); }
    }

    // Using a DependencyProperty as the backing store for LabelVisibility.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LabelVisibilityProperty =
        DependencyProperty.Register(
            "LabelVisibility",
            typeof(Visibility),
            typeof(BlackMirrorSpinner),
            new PropertyMetadata(Visibility.Collapsed));



    public string? Text
    {
        get { return (string?)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(BlackMirrorSpinner),
            new PropertyMetadata((string?)null));


    public double Radius
    {
        get { return (double)GetValue(RadiusProperty); }
        set { SetValue(RadiusProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RadiusProperty =
        DependencyProperty.Register(
            "Radius",
            typeof(double),
            typeof(BlackMirrorSpinner),
            new FrameworkPropertyMetadata(
                100.0,
                FrameworkPropertyMetadataOptions.AffectsRender
                | FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                OnRadiusChanged));

    private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BlackMirrorSpinner spinner && spinner._pathCollection is not null && e.NewValue is double radius)
        {
            UpdateFigures6(spinner._pathCollection, (float)radius);
        }
    }

    private static void UpdateFigures6(PathCollection pathCollection, float radius)
    {
        float HEIGHT = radius * FIGURE_HEIGHT_RATIO;
        float HEIGHT_OFFSET_TOP = HEIGHT / FIGURE_TOP_CHAMFER_RATIO;
        float HEIGHT_OFFSETT_BOTTOM = HEIGHT / FIGURE_BOTTOM_CHAMFER_RATIO;

        float middleA = 90f;
        int j = 0;
        for (; j < N; middleA = (middleA <= 0 ? 360 + middleA : middleA) - ANGLE, j++)
        {
            float lowA = Clamp(middleA + HALF_ANGLE - ANGLE_OFFSET1);
            float lowInnerA = Clamp(middleA + HALF_ANGLE - ANGLE_OFFSET2);
            float lowInnerA2 = Clamp(middleA + HALF_ANGLE - ANGLE_OFFSET3);
            float highInnerA2 = Clamp(middleA - HALF_ANGLE + ANGLE_OFFSET3);
            float highInnerA = Clamp(middleA - HALF_ANGLE + ANGLE_OFFSET2);
            float highA = Clamp(middleA - HALF_ANGLE + ANGLE_OFFSET1);

            float offset = radius + HEIGHT + 1; // 1 - stroke size

            // low
            var p1 = Point1(lowA, radius + HEIGHT - HEIGHT_OFFSET_TOP, offset);
            var p2 = Point1(lowInnerA, radius + HEIGHT, offset);
            var p3 = Point1(highInnerA, radius + HEIGHT, offset);
            var p4 = Point1(highA, radius + HEIGHT - HEIGHT_OFFSET_TOP, offset);
            var p5 = Point1(highInnerA2, radius, offset);
            var p6 = Point1(lowInnerA2, radius, offset);

            FigurePoints6 figurePoints = new(p1, p2, p3, p4, p5, p6);
            Geometry geometry = BuildGeometry(figurePoints);
            Path path = pathCollection[j];
            path.Data = geometry;
            var sideSize = (2 * radius) + (HEIGHT * 2) + 2;
            path.Width = path.Height = sideSize;
        }

        // ------------------------------------------- local methods

        static Geometry BuildGeometry(FigurePoints6 figurePoints)
        {
            PathGeometry geometry = new();

            PathFigure pathFigure = new();
            pathFigure.StartPoint = figurePoints.P1;
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P2));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P3));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P4));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P5));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P6));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P1));

            geometry.Figures.Add(pathFigure);

            return geometry;
        }
    }

    private static void UpdateFigures8(PathCollection pathCollection, float radius)
    {
        float HEIGHT = radius * FIGURE_HEIGHT_RATIO;
        float HEIGHT_OFFSET_TOP = HEIGHT / FIGURE_TOP_CHAMFER_RATIO;
        float HEIGHT_OFFSETT_BOTTOM = HEIGHT / FIGURE_BOTTOM_CHAMFER_RATIO;

        float middleA = 90f;
        int j = 0;
        for (; j < N; middleA = (middleA <= 0 ? 360 + middleA : middleA) - ANGLE, j++)
        {
            float lowA = Clamp(middleA + HALF_ANGLE - ANGLE_OFFSET1);
            float lowInnerA = Clamp(middleA + HALF_ANGLE - ANGLE_OFFSET2);
            float lowInnerA2 = Clamp(middleA + HALF_ANGLE - ANGLE_OFFSET3);
            float highInnerA2 = Clamp(middleA - HALF_ANGLE + ANGLE_OFFSET3);
            float highInnerA = Clamp(middleA - HALF_ANGLE + ANGLE_OFFSET2);
            float highA = Clamp(middleA - HALF_ANGLE + ANGLE_OFFSET1);

            float offset = radius + HEIGHT + 1; // 1 - stroke size

            // low
            var (p1, p2) = Points2(lowA, radius + HEIGHT_OFFSET_TOP, radius + HEIGHT - HEIGHT_OFFSET_TOP, offset);

            // low inner
            var (_, p3) = Points2(lowInnerA, radius, radius + HEIGHT, offset);
            var (p8, _) = Points2(lowInnerA2, radius, radius + HEIGHT, offset);

            // high inner
            var (p7, _) = Points2(highInnerA2, radius, radius + HEIGHT, offset);
            var (_, p4) = Points2(highInnerA, radius, radius + HEIGHT, offset);

            // high
            var (p6, p5) = Points2(highA, radius + HEIGHT_OFFSET_TOP, radius + HEIGHT - HEIGHT_OFFSET_TOP, offset);

            FigurePoints8 figurePoints = new(p1, p2, p3, p4, p5, p6, p7, p8);
            Geometry geometry = BuildGeometry(figurePoints);
            Path path = pathCollection[j];
            path.Data = geometry;
            var sideSize = (2 * radius) + (HEIGHT * 2) + 2;
            path.Width = path.Height = sideSize;
        }

        // ------------------------------------------- local methods

        static Geometry BuildGeometry(FigurePoints8 figurePoints)
        {
            PathGeometry geometry = new();
            
            PathFigure pathFigure = new();
            pathFigure.StartPoint = figurePoints.P1;
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P2));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P3));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P4));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P5));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P6));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P7));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P8));
            pathFigure.Segments.Add(GetLineSegment(figurePoints.P1));

            geometry.Figures.Add(pathFigure);

            return geometry;
        }
    }

    private static float Clamp(float angle)
        => angle < 0 ? 360 + angle : angle;

    private static Point Point1(float a, float r, float offset)
    {
        float rad;
        float cos;
        float sin;

        float xLow1;
        float yLow1;

        float xLow2;
        float yLow2;

        if (0 <= a && a <= 90)
        {
            rad = a * K;
            cos = (float)Math.Cos(rad);
            sin = (float)Math.Sin(rad);

            xLow1 = r * cos;
            yLow1 = -(r * sin);


            return new Point(xLow1 + offset, yLow1 + offset);
        }

        if (90 <= a && a <= 180)
        {
            rad = (a % 90) * K;
            cos = (float)Math.Cos(rad);
            sin = (float)Math.Sin(rad);

            xLow1 = -(r * sin);
            yLow1 = -(r * cos);

            return new Point(xLow1 + offset, yLow1 + offset);
        }

        if (180 <= a && a <= 270)
        {
            rad = (a % 90) * K;
            cos = (float)Math.Cos(rad);
            sin = (float)Math.Sin(rad);

            xLow1 = -(r * cos);
            yLow1 = r * sin;

            return new Point(xLow1 + offset, yLow1 + offset);
        }

        rad = (a % 90) * K;
        cos = (float)Math.Cos(rad);
        sin = (float)Math.Sin(rad);

        xLow1 = r * sin;
        yLow1 = r * cos;

        return new Point(xLow1 + offset, yLow1 + offset);
    }

    private static (Point, Point) Points2(float a, float r1, float r2, float offset)
    {
        float rad;
        float cos;
        float sin;

        float xLow1;
        float yLow1;

        float xLow2;
        float yLow2;

        if (0 <= a && a <= 90)
        {
            rad = a * K;
            cos = (float)Math.Cos(rad);
            sin = (float)Math.Sin(rad);

            xLow1 = r1 * cos;
            yLow1 = -(r1 * sin);

            xLow2 = r2 * cos;
            yLow2 = -(r2 * sin);

            return (new Point(xLow1 + offset, yLow1 + offset), new Point(xLow2 + offset, yLow2 + offset));
        }

        if (90 <= a && a <= 180)
        {
            rad = (a % 90) * K;
            cos = (float)Math.Cos(rad);
            sin = (float)Math.Sin(rad);

            xLow1 = -(r1 * sin);
            yLow1 = -(r1 * cos);

            xLow2 = -(r2 * sin);
            yLow2 = -(r2 * cos);

            return (new Point(xLow1 + offset, yLow1 + offset), new Point(xLow2 + offset, yLow2 + offset));
        }

        if (180 <= a && a <= 270)
        {
            rad = (a % 90) * K;
            cos = (float)Math.Cos(rad);
            sin = (float)Math.Sin(rad);

            xLow1 = -(r1 * cos);
            yLow1 = r1 * sin;

            xLow2 = -(r2 * cos);
            yLow2 = r2 * sin;

            return (new Point(xLow1 + offset, yLow1 + offset), new Point(xLow2 + offset, yLow2 + offset));
        }

        rad = (a % 90) * K;
        cos = (float)Math.Cos(rad);
        sin = (float)Math.Sin(rad);

        xLow1 = r1 * sin;
        yLow1 = r1 * cos;

        xLow2 = r2 * sin;
        yLow2 = r2 * cos;

        return (new Point(xLow1 + offset, yLow1 + offset), new Point(xLow2 + offset, yLow2 + offset));
    }


    private static LineSegment GetLineSegment(Point point)
        => new LineSegment(point, false);

    private static ArcSegment GetArcSegment(Point pStart, Point pEnd)
        => new ArcSegment(
                pEnd,
                GetArcSize(pEnd, pStart),
                0,
                false,
                SweepDirection.Clockwise,
                false);

    private static Size GetArcSize(Point pA, Point pB)
        => new Size(Math.Abs(pA.X - pB.X), Math.Abs(pA.Y - pB.Y));

    

    /// <summary>
    /// <code>
    ///   p3-------------p4
    /// p2                 p5
    /// p1                 p6
    ///   p8-------------p7
    /// </code>
    /// </summary>
    /// <param name="P1"></param>
    /// <param name="P2"></param>
    /// <param name="P3"></param>
    /// <param name="P4"></param>
    /// <param name="P5"></param>
    /// <param name="P6"></param>
    /// <param name="P7"></param>
    /// <param name="P8"></param>
    private record struct FigurePoints8(Point P1, Point P2, Point P3, Point P4, Point P5, Point P6, Point P7, Point P8);

    /// <summary>
    /// <code>
    ///   p3-------------p4
    /// p2                 p5
    ///     p8---------p7
    /// </code>
    /// </summary>
    /// <param name="P1"></param>
    /// <param name="P2"></param>
    /// <param name="P3"></param>
    /// <param name="P4"></param>
    /// <param name="P5"></param>
    /// <param name="P6"></param>
    /// <param name="P7"></param>
    /// <param name="P8"></param>
    private record struct FigurePoints6(Point P1, Point P2, Point P3, Point P4, Point P5, Point P6);

    private sealed record PathCollection(
        Path FigureN,
        Path FigureNE,
        Path FigureE,
        Path FigureSE,
        Path FigureS,
        Path FigureSW,
        Path FigureW,
        Path FigureNW
    )
    {
        public Path this[int index]
        {
            get => index switch
            {
                0 => FigureN,
                1 => FigureNE,
                2 => FigureE,
                3 => FigureSE,
                4 => FigureS,
                5 => FigureSW,
                6 => FigureW,
                7 => FigureNW,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
    }
}
