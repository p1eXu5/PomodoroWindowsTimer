using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.Converters;

internal sealed class AsyncDeferredToVisicbilityConverter : IValueConverter
{
    private static Type _asyncDeferredType = typeof(Elmish.Extensions.AsyncDeferred<>);
    private static PropertyInfo _isNotRequestedProperty = _asyncDeferredType.GetProperty(nameof(Elmish.Extensions.AsyncDeferred<object>.IsNotRequested))!;
    private static PropertyInfo _isInProgressProperty = _asyncDeferredType.GetProperty(nameof(Elmish.Extensions.AsyncDeferred<object>.IsInProgress))!;
    private static PropertyInfo _isRetrievedProperty = _asyncDeferredType.GetProperty(nameof(Elmish.Extensions.AsyncDeferred<object>.IsRetrieved))!;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value.GetType() == _asyncDeferredType)
        {
            if (_isInProgressProperty.GetValue(value) is bool v && v)
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
