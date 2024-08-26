using System;
using System.Globalization;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

internal sealed class IsWorkSelectedMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not null && values.Length >= 2 && values[0] is UInt64 workId && values[1] is UInt64 selectedWorkId)
        {
            return workId == selectedWorkId;
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
