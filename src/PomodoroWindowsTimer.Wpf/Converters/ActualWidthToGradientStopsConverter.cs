using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PomodoroWindowsTimer.Wpf.Converters;

public sealed class ActualWidthToGradientStopsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double actualWidth && parameter is GradientStop[] stops && stops.Length == 4)
        {
            GradientStopCollection gradientStopCollection = new GradientStopCollection();
            foreach (GradientStop stop in stops)
            {
                double stopOffset = actualWidth * stop.Offset;
                
                gradientStopCollection.Add(
                    new GradientStop
                    {
                        Color = stop.Color,
                        Offset = (stopOffset - 0) / actualWidth,
                    }
                );
            }

            return gradientStopCollection;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
