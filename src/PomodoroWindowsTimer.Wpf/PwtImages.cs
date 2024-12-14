using System.Windows;

namespace PomodoroWindowsTimer.Wpf;

public static class PwtImages
{
    public static ComponentResourceKey BaseTextureKey => new ComponentResourceKey(
            typeof(PwtImages), "im_BaseTexture");

    public static ComponentResourceKey GlossTextureKey => new ComponentResourceKey(
            typeof(PwtImages), "im_GlossTexture");
}
