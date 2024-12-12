using System.Windows;
using System.Windows.Media;

namespace PomodoroWindowsTimer.Wpf.Extensions;

public static class VisualTreeHelperExtensions
{
    public static T? FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        // Get the parent of the child
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);

        // If there is no parent, return null
        if (parentObject == null)
        {
            return null;
        }

        // Check if the parent is of the desired type
        if (parentObject is T parent)
        {
            return parent;
        }

        // Recursively search the parent
        return FindParent<T>(parentObject);
    }
}
