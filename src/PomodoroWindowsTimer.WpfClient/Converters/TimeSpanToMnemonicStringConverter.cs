using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.Converters;

internal sealed class TimeSpanToMnemonicStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            double totalMinutes = Math.Abs(ts.TotalMinutes);
            int hours = (int)(totalMinutes / 60.0);
            int minutes = (int)(Math.Ceiling(totalMinutes - hours * 60));
            char sign = ts < TimeSpan.Zero ? '-' : ' ';

            return $"{sign}{hours,2:#0}h {minutes:00}m";
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
