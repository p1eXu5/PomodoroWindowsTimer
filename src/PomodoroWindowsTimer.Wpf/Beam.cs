using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.Wpf;

public class Beam : Control
{
    static Beam()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Beam), new FrameworkPropertyMetadata(typeof(Beam)));
    }
}
