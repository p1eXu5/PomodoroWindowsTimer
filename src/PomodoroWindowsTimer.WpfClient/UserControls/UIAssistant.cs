using System.Windows;

namespace PomodoroWindowsTimer.WpfClient.UserControls;

public static class UIAssistant
{
    public static readonly DependencyProperty IsLeftLcdDrawerOpenProperty =
        DependencyProperty.RegisterAttached(
            "IsLeftLcdDrawerOpen",
            typeof(bool),
            typeof(UIAssistant),
            new FrameworkPropertyMetadata(defaultValue: false)
        );

    public static bool GetIsLeftLcdDrawerOpen(UIElement target) =>
        (bool)target.GetValue(IsLeftLcdDrawerOpenProperty);

    public static void SetIsLeftLcdDrawerOpen(UIElement target, bool value) =>
        target.SetValue(IsLeftLcdDrawerOpenProperty, value);



    // Using a DependencyProperty as the backing store for IsRightLcdDrawerOpen.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsRightLcdDrawerOpenProperty =
        DependencyProperty.RegisterAttached(
            "IsRightLcdDrawerOpen",
            typeof(bool),
            typeof(UIAssistant),
            new PropertyMetadata(false)
        );

    public static bool GetIsRightLcdDrawerOpen(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsRightLcdDrawerOpenProperty);
    }

    public static void SetIsRightLcdDrawerOpen(DependencyObject obj, bool value)
    {
        obj.SetValue(IsRightLcdDrawerOpenProperty, value);
    }
}
