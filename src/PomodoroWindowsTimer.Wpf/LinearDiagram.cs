using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PomodoroWindowsTimer.Wpf.Models;

namespace PomodoroWindowsTimer.Wpf;

[TemplatePart(Name = "PART_ItemsControl", Type = typeof(ItemsControl))]
public class LinearDiagram : ItemsControl
{
    private ItemsControl _innerItemsControl = default!;

    static LinearDiagram()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LinearDiagram), new FrameworkPropertyMetadata(typeof(LinearDiagram)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _innerItemsControl = (ItemsControl)GetTemplateChild("PART_ItemsControl");

        if (ItemsSource is not null)
        {
            OnItemsSourceChanged(ItemsSource, ItemsSource);
        }
    }

    public double DiagramHeight
    {
        get { return (double)GetValue(DiagramHeightProperty); }
        set { SetValue(DiagramHeightProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DiagramHeight.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DiagramHeightProperty =
        DependencyProperty.Register("DiagramHeight", typeof(double), typeof(LinearDiagram), new PropertyMetadata(26.0));



    public string StartTime
    {
        get { return (string)GetValue(StartTimeProperty); }
        set { SetValue(StartTimeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty StartTimeProperty =
        DependencyProperty.Register("StartTime", typeof(string), typeof(LinearDiagram), new PropertyMetadata(default));



    public string EndTime
    {
        get { return (string)GetValue(EndTimeProperty); }
        set { SetValue(EndTimeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty EndTimeProperty =
        DependencyProperty.Register("EndTime", typeof(string), typeof(LinearDiagram), new PropertyMetadata(default));



    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register("CornerRadius", typeof(CornerRadius ), typeof(LinearDiagram), new PropertyMetadata(default));



    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (_innerItemsControl is null)
        {
            return;
        }

        base.OnItemsSourceChanged(oldValue, newValue);

        if (newValue is IReadOnlyCollection<LinearDiagramItem> data)
        {
            var startTime = data.First().StartTime;
            StartTime = $"{startTime.Hour}:{startTime.Minute.ToString("00")}";
            var last = data.Last();
            var endTime = last.StartTime.AddMinutes(last.Duration.TotalMinutes);
            EndTime = $"{endTime.Hour}:{endTime.Minute.ToString("00")}";

            List<RectData> rectangles = [];

            foreach (var value in data)
            {
                if (!value.IsWork.HasValue)
                {
                    rectangles.Add(new(
                        Fill: Brushes.Transparent,
                        Width: value.Duration.TotalMinutes,
                        Hint: GetHint(value.Name, value.StartTime, value.Duration, value.IsWork)
                    ));

                    continue;
                }

                rectangles.Add(new(
                    Fill: GetBrush(value.Id, value.IsWork.Value),
                    Width: value.Duration.TotalMinutes,
                    Hint: GetHint(value.Name, value.StartTime, value.Duration, value.IsWork)
                ));
            }

            _innerItemsControl.ItemsSource = rectangles;
        }
    }

    private static string GetHint(string name, TimeOnly time, TimeSpan duration, bool? isWork)
    {
        var sb = new StringBuilder();
        if (isWork.HasValue)
        {
            sb.Append(name).Append(" [").Append(isWork.Value ? "WORK" : "BREAK").AppendLine("]");
        }
        else
        {
            sb.AppendLine("Idle");
        }
        sb.Append(time.Hour).Append(':').Append(time.Minute.ToString("00")).Append(" - ");
        var end = time.AddMinutes(duration.TotalMinutes);
        sb.Append(end.Hour).Append(':').Append(end.Minute.ToString("00")).AppendLine();
        sb.Append('(').Append(duration.Hours).Append(':').Append(duration.Minutes.ToString("00")).Append(')');

        return sb.ToString();
    }

    private Brush GetBrush(int id, bool isActive)
    {
        switch (id % 10)
        {
            case 1:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush1Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush1Key);

            case 2:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush2Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush2Key);

            case 3:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush3Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush3Key);

            case 4:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush4Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush4Key);

            case 5:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush5Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush5Key);

            case 6:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush6Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush6Key);

            case 7:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush7Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush7Key);

            case 8:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush8Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush8Key);

            case 9:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush9Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush9Key);

            case 0:
                return isActive
                    ? (SolidColorBrush)TryFindResource(PwtBrushes.DiagramAccentBrush10Key)
                    : (SolidColorBrush)TryFindResource(PwtBrushes.DiagramBrush10Key);

            default:
                return null!;
        }
    }

    private readonly record struct RectData(
        System.Windows.Media.Brush Fill,
        double Width,
        string Hint
    );
}
