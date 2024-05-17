using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.Converters;

internal sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v)
        {
            return
                v ? Visibility.Collapsed : Visibility.Visible;
        }

        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v)
        {
            return
                v != Visibility.Visible;
        }

        return null;
    }
}
