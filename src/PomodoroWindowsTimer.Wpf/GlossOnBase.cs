using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.Wpf;

public class GlossOnBase : Control
{
    static GlossOnBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GlossOnBase), new FrameworkPropertyMetadata(typeof(GlossOnBase)));
    }
}
