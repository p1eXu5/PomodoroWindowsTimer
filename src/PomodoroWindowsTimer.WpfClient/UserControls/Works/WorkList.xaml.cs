using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

/// <summary>
/// Interaction logic for WorkList.xaml
/// </summary>
public partial class WorkList : UserControl
{
    private Predicate<object>? _filter;

    public WorkList()
    {
        InitializeComponent();

        ((INotifyCollectionChanged)m_WorkList.Items).CollectionChanged += mListBox_CollectionChanged;
    }

    private Predicate<object>? Filter => _filter ??= new Predicate<object>(o =>
    {
        string searchText = SearchText!;

        string number = ((dynamic)o).Number;
        string title = ((dynamic)o).Title;
        return
            number.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || title.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    });

    private int Count { get; set; }
    private string? SearchText { get; set; }

    private void WorksSearchFilterPanel_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.Source is WorksSearchFilterPanel tb)
        {
            e.Handled = true;

            string searchText = SearchText = tb.m_Search.Text;

            if (String.IsNullOrEmpty(searchText) && Count < 1)
            {
                ResetFilter();
                return;
            }

            SetFilter();
        }
    }

    private void ResetFilter()
    {
        var collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorkList.ItemsSource);

        if (!collView.CanFilter)
        {
            return;
        }

        collView.Filter = null;
    }

    private void SetFilter()
    {
        var collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorkList.ItemsSource);

        if (!collView.CanFilter)
        {
            return;
        }

        collView.Filter = Filter;
    }

    private void SetWorksFilter(WorksSearchFilterPanel tb)
    {
        var collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorkList.ItemsSource);

        if (!collView.CanFilter)
        {
            return;
        }

        string searchText = tb.m_Search.Text;
        Int32.TryParse(tb.m_Count.Text, out int countText);

        if (String.IsNullOrEmpty(searchText) && countText < 1)
        {
            collView.Filter = null;
        }
        else
        {
            collView.Filter = new Predicate<object>(o =>
            {
                string number = ((dynamic)o).Number;
                string title = ((dynamic)o).Title;
                return
                    number.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    || title.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    private void WorksSearchFilterPanel_CountChanged(object sender, TextChangedEventArgs e)
    {

    }

    private void mListBox_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Clear any existing sorting first
        m_WorkList.Items.SortDescriptions.Clear();

        // Sort by the Content property
        m_WorkList.Items.SortDescriptions.Add(
            new SortDescription("UpdatedAt", ListSortDirection.Descending));
    }

}
