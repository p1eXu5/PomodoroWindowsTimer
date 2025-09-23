﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using PomodoroWindowsTimer.ElmishApp.Models;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

/// <summary>
/// Interaction logic for WorkList.xaml
/// </summary>
public partial class WorkSelector : UserControl
{
    private Predicate<object>? _filter;

    public WorkSelector()
    {
        InitializeComponent();
    }

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

    private TimeSpan DayCount { get; set; } = TimeSpan.Zero;

    private string SearchText { get; set; } = "";

    private DateTimeOffset NowDate { get; set; }

    private void WorksSearchFilterPanel_TextChanged(object sender, SearchTextChangedEventArgs e)
    {
        e.Handled = true;

        string searchText = e.SearchText;

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

    private void WorksSearchFilterPanel_CountChanged(object sender, CountChangedEventArgs e)
    {
        e.Handled = true;

        int? count = e.Count;

        if (!count.HasValue)
        {
            if (DayCount != TimeSpan.Zero && String.IsNullOrEmpty(SearchText))
            {
                DayCount = TimeSpan.Zero;
                ResetFilter();
            }

            return;
        }

        NowDate = DateTimeOffset.UtcNow;
        DayCount = TimeSpan.FromDays(count.Value);
        SetFilter();
    }

    private void ResetFilter()
    {
        var collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorkList.m_WorkList.ItemsSource);

        if (collView is null || !collView.CanFilter)
        {
            return;
        }

        collView.Filter = null;
    }

    private void SetFilter()
    {
        var collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorkList.m_WorkList.ItemsSource);

        if (collView is null || !collView.CanFilter)
        {
            return;
        }

        collView.Filter = Filter;
    }

    //private void SetWorksFilter(WorksSearchFilterPanel tb)
    //{
    //    var collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorkList.m_WorkList.ItemsSource);

    //    if (!collView.CanFilter)
    //    {
    //        return;
    //    }

    //    string searchText = tb.m_Search.Text;
    //    Int32.TryParse(tb.m_Count.Text, out int countText);

    //    if (String.IsNullOrEmpty(searchText) && countText < 1)
    //    {
    //        collView.Filter = null;
    //    }
    //    else
    //    {
    //        collView.Filter = new Predicate<object>(o =>
    //        {
    //            string number = ((dynamic)o).Number;
    //            string title = ((dynamic)o).Title;
    //            return
    //                number.Contains(searchText, StringComparison.OrdinalIgnoreCase)
    //                || title.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    //        });
    //    }
    //}
}
