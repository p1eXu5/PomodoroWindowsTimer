using System.Windows;

namespace PomodoroWindowsTimer.WpfClient.UserControls;

public static class UIAssistant
{
    public static readonly DependencyProperty AreRunningTimePointsShownProperty =
            DependencyProperty.RegisterAttached(
          "AreRunningTimePointsShown",
          typeof(bool),
          typeof(UIAssistant),
          new FrameworkPropertyMetadata(defaultValue: false)
        );

    public static bool GetAreRunningTimePointsShown(UIElement target) =>
        (bool)target.GetValue(AreRunningTimePointsShownProperty);

    public static void SetAreRunningTimePointsShown(UIElement target, bool value) =>
        target.SetValue(AreRunningTimePointsShownProperty, value);



    public static bool GetIsTimePointsGeneratorShown(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsTimePointsGeneratorShownProperty);
    }

    public static void SetIsTimePointsGeneratorShown(DependencyObject obj, bool value)
    {
        obj.SetValue(IsTimePointsGeneratorShownProperty, value);
    }

    // Using a DependencyProperty as the backing store for IsTimePointsGeneratorShown.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsTimePointsGeneratorShownProperty =
        DependencyProperty.RegisterAttached("IsTimePointsGeneratorShown", typeof(bool), typeof(UIAssistant), new PropertyMetadata(false));


}
