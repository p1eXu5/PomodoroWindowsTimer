using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.WpfClient.Converters;

internal class KindAliasToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Alias alias)
        {
            return AliasModule.value(alias);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
