using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.Wpf;

public class GlossBaseRect : Control
{
    static GlossBaseRect()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GlossBaseRect), new FrameworkPropertyMetadata(typeof(GlossBaseRect)));
    }
}
