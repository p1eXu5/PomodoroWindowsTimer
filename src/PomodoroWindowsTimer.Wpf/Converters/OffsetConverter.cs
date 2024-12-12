using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace PomodoroWindowsTimer.Wpf.Converters;

public sealed class OffsetConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 2 && values[0] is double baseMeasurement && values[1] is double actualMeasurement && parameter is double offsetRel)
        {
            double offset = baseMeasurement * offsetRel;
            return ((baseMeasurement - actualMeasurement) / 2.0) - offset;
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
