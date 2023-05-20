using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.WpfClient.Converters;
public sealed class KindToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Kind kind)
        {
            switch (kind.Tag)
            {
                case Kind.Tags.Break:
                case Kind.Tags.LongBreak:
                    return (Brush)Application.Current.FindResource("BreakDigitForeground");

                default:
                    return (Brush)Application.Current.FindResource("WorkDigitForeground");
            }
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
