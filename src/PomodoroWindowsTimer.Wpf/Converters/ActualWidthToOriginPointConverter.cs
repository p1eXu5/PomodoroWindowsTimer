using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.Wpf.Converters;

public sealed class ActualWidthToOriginPointConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 1)
        {
            return CalculateOriginForGlossOnBase(values);
        }

        if (values?.Length == 4)
        {
            CalculateOriginForBeam(values, null, null);
        }

        if (values?.Length == 5)
        {
            CalculateOriginForBeam(values, values[4] as double?, null);
        }

        if (values?.Length == 6)
        {
            CalculateOriginForBeam(values, values[4] as double?, values[5] as double?);
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private object CalculateOriginForGlossOnBase(object[] values)
    {
        if (values[0] is double actualWidth)
        {
            return new Point(actualWidth, 0);
        }

        return DependencyProperty.UnsetValue;
    }

    private object CalculateOriginForBeam(object[] values, double? offsetWidth, double? offsetHeight)
    {
        if (values[0] is double baseWidth
            && values[1] is double baseHeight
            && values[2] is double actualWidth
            && values[3] is double actualHeight)
        {
            double baseRightPointOffset = (baseWidth - actualWidth) / 2.0;
            double baseTopPointOffset = (baseHeight - actualHeight) / 2.0;

            Point point;
            double x = actualWidth + baseRightPointOffset;
            double y = -baseTopPointOffset;

            if (offsetWidth.HasValue && offsetHeight.HasValue)
            {
                point = new Point(x + offsetWidth.Value, y + offsetHeight.Value
                );
            }
            else if (offsetWidth.HasValue)
            {
                point = new Point(x + offsetWidth.Value, y);
            }
            else
            {
                point = new Point(x, y);
            }

            return point;
        }

        return DependencyProperty.UnsetValue;
    }
}
