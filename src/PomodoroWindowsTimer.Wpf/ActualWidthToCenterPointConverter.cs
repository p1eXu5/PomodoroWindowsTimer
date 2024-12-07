using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.Wpf;

public sealed class ActualWidthToCenterPointConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double actualWidth)
        {
            return new Point(actualWidth - 250.0, 250);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
