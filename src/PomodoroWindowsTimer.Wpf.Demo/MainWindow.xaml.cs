using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PomodoroWindowsTimer.Wpf.Models;

namespace PomodoroWindowsTimer.Wpf.Demo;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        m_Diagram.ItemsSource = new Dictionary<LinearDiagramItemKey, IReadOnlyCollection<LinearDiagramItemValue>>
        {
            [new LinearDiagramItemKey(1, "Work1")] = new[]
            {
                new LinearDiagramItemValue(
                    StartTime: new TimeOnly(8, 0),
                    Duration: TimeSpan.FromMinutes(25),
                    true
                ),
                new LinearDiagramItemValue(
                    StartTime: new TimeOnly(8, 25),
                    Duration: TimeSpan.FromMinutes(5),
                    false
                ),
            },
            [new LinearDiagramItemKey(2, "Work2")] = new[]
            {
                new LinearDiagramItemValue(
                    StartTime: new TimeOnly(8, 30),
                    Duration: TimeSpan.FromMinutes(25),
                    true
                ),
                new LinearDiagramItemValue(
                    StartTime: new TimeOnly(8, 55),
                    Duration: TimeSpan.FromMinutes(5),
                    false
                ),
            },
            [new LinearDiagramItemKey(0, "Idle")] = new[]
            {
                new LinearDiagramItemValue(
                    StartTime: new TimeOnly(9, 00),
                    Duration: TimeSpan.FromHours(7),
                    null
                ),
            },
        };
    }
}