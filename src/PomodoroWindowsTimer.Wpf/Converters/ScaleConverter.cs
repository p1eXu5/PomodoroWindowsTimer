using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.Wpf.Converters;

public sealed class ScaleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 2 && values[0] is double baseMeasurement && values[1] is double actualMeasurement)
        {
            return baseMeasurement / actualMeasurement;
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
