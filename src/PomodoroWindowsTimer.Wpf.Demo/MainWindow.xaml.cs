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

        m_Diagram.ItemsSource = new LinearDiagramItem[]
        {
            new LinearDiagramItem(
                Id: 1,
                Name: "Work1",
                StartTime: new TimeOnly(8, 0),
                Duration: TimeSpan.FromMinutes(25),
                true
            ),
            new LinearDiagramItem(
                Id: 1,
                Name: "Work1",
                StartTime: new TimeOnly(8, 25),
                Duration: TimeSpan.FromMinutes(5),
                false
            ),
            
            new LinearDiagramItem(
                Id: 2,
                Name: "Work2",
                StartTime: new TimeOnly(8, 30),
                Duration: TimeSpan.FromMinutes(25),
                true
            ),
            new LinearDiagramItem(
                Id: 2,
                Name: "Work2",
                StartTime: new TimeOnly(8, 55),
                Duration: TimeSpan.FromMinutes(5),
                false
            ),

            new LinearDiagramItem(
                Id: 0,
                Name: "Idle",
                StartTime: new TimeOnly(9, 00),
                Duration: TimeSpan.FromHours(7),
                null
            ),
        };
    }
}