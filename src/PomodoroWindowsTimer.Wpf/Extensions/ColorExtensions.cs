using Color = System.Windows.Media.Color;

namespace PomodoroWindowsTimer.Wpf.Extensions;

public static class ColorExtensions
{
    public static double CalculateBrightness(this Color color)
        => 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;

    public static Color AdjustToBrightness(this Color color, double targetBrightness, double multiplier = 1.0)
    {
        double currentBrightness = CalculateBrightness(color);
        double brightnessRatio = targetBrightness / currentBrightness;

        var r = Clamp((int)(color.R * brightnessRatio * multiplier));
        var g = Clamp((int)(color.G * brightnessRatio * multiplier));
        var b = Clamp((int)(color.B * brightnessRatio * multiplier));

        return Color.FromRgb(r, g, b);

        // -------------------- local methods
        static byte Clamp(int value)
            => (byte)Math.Clamp(value, 0, 0xFF);
    }
}
