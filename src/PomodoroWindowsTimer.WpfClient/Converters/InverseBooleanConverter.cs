using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.Converters;

internal sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v)
        {
            return !v;
        }

        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v)
        {
            return !v;
        }

        return null;
    }
}
