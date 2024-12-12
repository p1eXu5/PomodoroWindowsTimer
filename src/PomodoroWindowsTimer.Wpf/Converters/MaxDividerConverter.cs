using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.Wpf.Converters;

public sealed class MaxDividerConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 2 && values[0] is double actualWidth && values[1] is double actualHeight && parameter is double multiplier)
        {
            return
                actualWidth > actualHeight
                    ? actualWidth * multiplier
                    : actualHeight * multiplier;
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
