using System.Windows;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

public sealed class SearchTextChangedEventArgs : RoutedEventArgs
{
    public SearchTextChangedEventArgs(RoutedEvent routedEvent, string searchText)
        : base(routedEvent)
    {
        SearchText = searchText;
    }

    public string SearchText { get; }
}

public delegate void SearchTextChangedEventHandler(object sender, SearchTextChangedEventArgs e);
