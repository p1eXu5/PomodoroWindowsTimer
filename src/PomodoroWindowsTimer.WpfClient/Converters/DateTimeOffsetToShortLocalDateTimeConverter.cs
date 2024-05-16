﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.Converters;
internal class DateTimeOffsetToShortLocalDateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dtOffset)
        {
            DateTime localDateTime = dtOffset.LocalDateTime;
            var d = localDateTime.ToShortDateString();
            var t = localDateTime.ToShortTimeString();
            return $"{d} {t}";
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
