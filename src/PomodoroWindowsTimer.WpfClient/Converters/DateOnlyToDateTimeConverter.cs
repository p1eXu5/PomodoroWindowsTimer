using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.Converters;

internal sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateOnly d)
        {
            return d.ToDateTime(default);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not null && value is DateTime dt)
        {
            return DateOnly.FromDateTime(dt);
        }

        return null!;
    }
}
