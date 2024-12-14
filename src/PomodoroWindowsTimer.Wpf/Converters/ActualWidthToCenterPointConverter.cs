using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PomodoroWindowsTimer.Wpf.Converters;

/// <summary>
/// Shifts Center of <see cref="RadialGradientBrush"/> from top-right corner to center
/// by percent of actual width and height.
/// </summary>
public sealed class ActualWidthToCenterPointConverter : IMultiValueConverter
{
    private const double _widthOffsetPercent = 0.3125;
    private const double _heightOffsetPercent = 0.4807;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 2)
        {
            return CalculateCenterForGlossOnBase(values);
        }

        if (values?.Length == 4)
        {
            CalculateCenterForBeam(values, null, null);
        }

        if (values?.Length == 5)
        {
            CalculateCenterForBeam(values, values[4] as double?, null);
        }

        if (values?.Length == 6)
        {
            CalculateCenterForBeam(values, values[4] as double?, values[5] as double?);
        }

        return DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetTypes"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    private object CalculateCenterForGlossOnBase(object[] values)
    {
        if (values[0] is double actualWidth
            && values[1] is double actualHeight)
        {
            return new Point(actualWidth - actualWidth * _widthOffsetPercent, actualHeight * _heightOffsetPercent);
        }

        return DependencyProperty.UnsetValue;
    }

    private object CalculateCenterForBeam(object[] values, double? offsetWidth, double? offsetHeight)
    {
        if (values[0] is double baseWidth
            && values[1] is double baseHeight
            && values[2] is double actualWidth
            && values[3] is double actualHeight
        )
        {
            var baseRightPointOffset = (baseWidth - actualWidth) / 2.0;
            var baseTopPointOffset = (baseHeight - actualHeight) / 2.0;

            Point point;
            var x = actualWidth - actualWidth * _widthOffsetPercent + baseRightPointOffset;
            var y = actualHeight * _heightOffsetPercent - baseTopPointOffset;

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
