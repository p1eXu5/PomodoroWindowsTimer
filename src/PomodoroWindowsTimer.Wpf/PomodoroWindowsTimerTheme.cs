using System.Windows;

namespace PomodoroWindowsTimer.Wpf;

public sealed class PomodoroWindowsTimerTheme : ResourceDictionary
{
    public PomodoroWindowsTimerTheme()
    {
        
    }
}

public static class PwtImages
{
    public static ComponentResourceKey ImgGlossKey => new ComponentResourceKey(
            typeof(PwtImages), "ImgGloss");

    public static ComponentResourceKey ImgBaseKey => new ComponentResourceKey(
            typeof(PwtImages), "ImgBase");
}

public static class PwtBrushes
{
    public static ComponentResourceKey BrBaseKey => new ComponentResourceKey(
            typeof(PwtBrushes), "BrBase");
}
