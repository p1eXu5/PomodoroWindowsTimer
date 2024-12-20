using System.Windows;

namespace PomodoroWindowsTimer.Wpf;

public static class PwtColors
{
    public static ComponentResourceKey SpinnerFilledKey => new ComponentResourceKey(
        typeof(PwtColors), "col_SpinnerFilled");

    public static ComponentResourceKey SpinnerUnfilledKey => new ComponentResourceKey(
        typeof(PwtColors), "col_SpinnerUnfilled");
}
