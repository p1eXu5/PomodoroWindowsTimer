using System.Windows;

namespace PomodoroWindowsTimer.Wpf;

public static class PwtBrushes
{
    public static ComponentResourceKey BaseTextureKey => new ComponentResourceKey(
            typeof(PwtBrushes), "br_BaseTexture");

    public static ComponentResourceKey GlossTextureKey => new ComponentResourceKey(
        typeof(PwtBrushes), "br_GlossTexture");

    public static ComponentResourceKey RadialGlossKey => new ComponentResourceKey(
        typeof(PwtBrushes), "br_RadialGloss");

    public static ComponentResourceKey SpinnerFilledKey => new ComponentResourceKey(
        typeof(PwtBrushes), "br_SpinnerFilled");

    public static ComponentResourceKey SpinnerUnfilledKey => new ComponentResourceKey(
        typeof(PwtBrushes), "br_SpinnerUnfilled");
}
