using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroWindowsTimer.Types;


namespace PomodoroWindowsTimer.WpfClient.Converters;

internal sealed class DiffTimeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            if (ts < TimeSpan.Zero) {
                return (Brush)Application.Current.FindResource("WorkDigitForeground");
            }
            return (Brush)Application.Current.FindResource("BreakDigitForeground");
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
