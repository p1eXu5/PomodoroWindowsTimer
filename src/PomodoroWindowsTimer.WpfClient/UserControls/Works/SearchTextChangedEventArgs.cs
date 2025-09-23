using System.Windows;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

internal sealed class SearchTextChangedEventArgs : RoutedEventArgs
{
    public SearchTextChangedEventArgs(RoutedEvent routedEvent, string searchText)
        : base(routedEvent)
    {
        SearchText = searchText;
    }

    public string SearchText { get; }
}

internal delegate void SearchTextChangedEventHandler(object sender, SearchTextChangedEventArgs e);
