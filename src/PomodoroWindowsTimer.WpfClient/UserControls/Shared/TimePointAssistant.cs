using System.Windows;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Shared
{
    /// <summary>
    /// TimePoint attached properties
    /// </summary>
    public static class TimePointAssistant
    {
        public static readonly DependencyProperty TimePointNameProperty =
            DependencyProperty.RegisterAttached(
          "TimePointName",
          typeof(string),
          typeof(TimePointAssistant),
          new FrameworkPropertyMetadata(defaultValue: "TimePointName")
        );

        public static string? GetTimePointName(UIElement target) =>
            (string?)target.GetValue(TimePointNameProperty);

        public static void SetTimePointName(UIElement target, string? value) =>
            target.SetValue(TimePointNameProperty, value);



        public static readonly DependencyProperty TimePointTimeSpanProperty =
            DependencyProperty.RegisterAttached(
          "TimePointTimeSpan",
          typeof(string),
          typeof(TimePointAssistant),
          new FrameworkPropertyMetadata(defaultValue: "0:00")
        );

        public static string? GetTimePointTimeSpan(UIElement target) =>
            (string?)target.GetValue(TimePointTimeSpanProperty);

        public static void SetTimePointTimeSpan(UIElement target, string? value) =>
            target.SetValue(TimePointTimeSpanProperty, value);



        public static readonly DependencyProperty TimePointKindProperty =
            DependencyProperty.RegisterAttached(
          "TimePointKind",
          typeof(Kind),
          typeof(TimePointAssistant),
          new FrameworkPropertyMetadata(defaultValue: null)
        );

        public static Kind? GetTimePointKind(UIElement target) =>
            (Kind?)target.GetValue(TimePointKindProperty);

        public static void SetTimePointKind(UIElement target, Kind? value) =>
            target.SetValue(TimePointKindProperty, value);



        public static readonly DependencyProperty TimePointKindAliasProperty =
            DependencyProperty.RegisterAttached(
          "TimePointKindAlias",
          typeof(string),
          typeof(TimePointAssistant),
          new FrameworkPropertyMetadata(defaultValue: null)
        );

        public static string? GetTimePointKindAlias(UIElement target) =>
            (string?)target.GetValue(TimePointKindAliasProperty);

        public static void SetTimePointKindAlias(UIElement target, string? value) =>
            target.SetValue(TimePointKindAliasProperty, value);
    }
}
