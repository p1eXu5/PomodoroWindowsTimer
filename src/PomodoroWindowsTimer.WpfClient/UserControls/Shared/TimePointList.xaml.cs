﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Shared;

/// <summary>
/// Interaction logic for TimePointList.xaml
/// </summary>
public partial class TimePointList : UserControl
{
    public TimePointList()
    {
        InitializeComponent();
    }

    [Bindable(true)]
    public DataTemplate? TimePointListBoxItemTemplate
    {
        get => m_TimePointListBox.ItemTemplate;
        set => m_TimePointListBox.ItemTemplate = value;
    }

    [Bindable(true)]
    public ScrollBarVisibility TimePointListBoxScrollBarVisibility
    {
        get => m_ScrollViewer.VerticalScrollBarVisibility;
        set => m_ScrollViewer.VerticalScrollBarVisibility = value;
    }

    public Visibility SearchToolbarVisibility
    {
        get => m_SearchToolbar.Visibility;
        set => m_SearchToolbar.Visibility = value;
    }

    public string? Header
    {
        get { return (string?)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(TimePointList), new PropertyMetadata(defaultValue: "TIME POINTS"));
}
