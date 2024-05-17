﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.WpfClient.Converters;

public sealed class KindToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Kind kind)
        {
            return KindModule.displayString(kind);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
