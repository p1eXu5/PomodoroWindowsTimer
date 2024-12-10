using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PomodoroWindowsTimer.Wpf.Converters;

public sealed class MultiWidthToGradientStopsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 3 && values[0] is double baseWidth && values[1] is double actualWidth && parameter is GradientStop[] stops && stops.Length == 4)
        {
            double actualLeft = values[2] is double v ? v : (baseWidth - actualWidth) / 2.0;
            double actualRight = actualLeft + actualWidth;

            GradientStopCollection gradientStopCollection = new GradientStopCollection();
            foreach (GradientStop stop in stops)
            {
                double stopOffset = baseWidth * stop.Offset;
                if (actualLeft <= stopOffset && stopOffset < actualRight)
                {
                    gradientStopCollection.Add(
                        new GradientStop
                        {
                            Color = stop.Color,
                            Offset = (stopOffset - actualLeft) / actualWidth,
                        }
                    );
                }
            }

            return gradientStopCollection;
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
