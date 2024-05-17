using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.Converters;

public sealed class HeightReminder2Converter : IMultiValueConverter
{
    private readonly List<double> _test = new();

    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 3 && values[0] is double hParent && values[1] is double hChild && values[2] is System.Windows.Thickness childMargin)
        {
            if (hParent > 0 && hChild > 0)
            {
                var height = hParent - hChild - (childMargin.Top + childMargin.Bottom);
                _test.Add(height);
                return height;
            }
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
