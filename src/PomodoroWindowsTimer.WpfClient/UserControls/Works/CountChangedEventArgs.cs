using System.Windows;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

public sealed class CountChangedEventArgs : RoutedEventArgs
{
    public CountChangedEventArgs(RoutedEvent routedEvent, string countText)
        : base(routedEvent)
    {
        Count = 
            string.IsNullOrWhiteSpace(countText)
            || !int.TryParse(countText, out var count)
            || count < 0
            ? null
            : count;
    }

    public int? Count { get; }
}

public delegate void CountChangedEventHandler(object sender, CountChangedEventArgs e);
