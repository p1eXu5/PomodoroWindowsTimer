using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using PomodoroWindowsTimer.ElmishApp.Models;

namespace PomodoroWindowsTimer.WpfClient.Converters
{
    public sealed class SubModelIdToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WorkSelectorModelModule.SubModelId subModelId && parameter is string p)
            {
                if (subModelId.IsWorkListId && String.Equals(p, "WorkListId", StringComparison.Ordinal))
                {
                    return Visibility.Visible;
                }

                if (subModelId.IsCreatingWorkId && String.Equals(p, "CreatingWorkId", StringComparison.Ordinal))
                {
                    return Visibility.Visible;
                }

                if (subModelId.IsUpdatingWorkId && String.Equals(p, "UpdatingWorkId", StringComparison.Ordinal))
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
