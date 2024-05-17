using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.WpfClient.Converters;

internal class ToUpperInvariantTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string alias)
        {
            return alias.ToUpperInvariant();
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
