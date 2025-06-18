using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

/// <summary>
/// Interaction logic for WorkTable.xaml
/// </summary>
public partial class WorkTable : UserControl
{
    private Predicate<object>? _filter;

    private string SearchText { get; set; } = "";

    private TimeSpan DayCount { get; set; } = TimeSpan.Zero;

    private DateTimeOffset NowDate { get; set; }


    private Predicate<object>? Filter => _filter ??= new Predicate<object>(o =>
    {
        string searchText = SearchText;
        TimeSpan dayCount = DayCount;
        DateTimeOffset now = NowDate;

        DateTimeOffset? lastEventCreatedAt;
        DateTimeOffset updatedAt;

        if (searchText != "")
        {
            string number = ((dynamic)o).Number;
            string title = ((dynamic)o).Title;

            return
                (
                    number.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    || title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                );
        }

        lastEventCreatedAt = ((dynamic)o).LastEventCreatedAt;
        updatedAt = ((dynamic)o).UpdatedAt;

        return
            (
                (lastEventCreatedAt.HasValue && (now - lastEventCreatedAt.Value <= dayCount))
                ||
                (now - updatedAt <= dayCount)
            );
    });


    public WorkTable()
    {
        InitializeComponent();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        e.Handled = true;

        string searchText = m_SearchText.Text;

        if (String.IsNullOrEmpty(searchText) && DayCount == TimeSpan.Zero)
        {
            SearchText = "";
            ResetFilter();
            return;
        }

        if (String.Equals(SearchText, searchText, StringComparison.Ordinal))
        {
            return;
        }

        SearchText = searchText;
        SetFilter();

    }

    private void ResetFilter()
    {
        CollectionView? collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorksDataGrid.ItemsSource);

        if (collView is null || !collView.CanFilter)
        {
            return;
        }

        collView.Filter = null;
    }

    private void SetFilter()
    {
        CollectionView? collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorksDataGrid.ItemsSource);

        if (collView is null || !collView.CanFilter)
        {
            return;
        }

        collView.Filter = Filter;
    }
}
