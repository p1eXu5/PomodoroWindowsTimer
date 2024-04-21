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
}
