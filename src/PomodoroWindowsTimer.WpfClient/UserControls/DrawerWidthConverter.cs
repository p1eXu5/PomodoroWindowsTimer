using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.UserControls;

internal sealed class DrawerWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double lcdWidth)
        {
            return Math.Floor(lcdWidth * 0.50);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
