using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.Converters;

internal sealed class TimeSpanToDateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            return new DateTime(default, TimeOnly.FromTimeSpan(ts));
        }

        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
        {
            return dt.TimeOfDay;
        }

        return null;
    }
}
