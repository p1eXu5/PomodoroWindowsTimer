﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.Wpf.Converters;
public sealed class ActualSizeToCenterPointConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 2 && values[0] is double actualWidth && values[1] is double actualHeight)
        {
            return new Point(actualWidth / 2, actualHeight / 2);
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
