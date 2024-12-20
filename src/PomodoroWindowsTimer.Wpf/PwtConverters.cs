using System.Windows;

namespace PomodoroWindowsTimer.Wpf;


// TODO: remove useless converters

public static class PwtConverters
{
    public static ComponentResourceKey ActualSizeToCenterPointKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_ActualSizeToCenterPoint");

    public static ComponentResourceKey ActualSizeToOuterTopRightPointKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_ActualSizeToOuterTopRightPoint");

    public static ComponentResourceKey ActualWidthToCenterPointKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_ActualWidthToCenterPoint");

    public static ComponentResourceKey ActualWidthToEndPointKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_ActualWidthToEndPoint");

    public static ComponentResourceKey ActualWidthToOriginPointKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_ActualWidthToOriginPoint");

    public static ComponentResourceKey DividerKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_Divider");

    public static ComponentResourceKey MaxDividerKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_MaxDivider");

    public static ComponentResourceKey ScaleKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_Scale");

    public static ComponentResourceKey MultiWidthToGradientStopsKey => new ComponentResourceKey(
            typeof(PwtConverters), "conv_MultiWidthToGradientStops");
}
