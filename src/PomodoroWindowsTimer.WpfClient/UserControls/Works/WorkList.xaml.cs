using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;

/// <summary>
/// Interaction logic for WorkList.xaml
/// </summary>
public partial class WorkList : UserControl
{
    public WorkList()
    {
        InitializeComponent();

        ((INotifyCollectionChanged)m_WorkList.Items).CollectionChanged += WorkList_CollectionChanged;
    }

    private void WorkList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Clear any existing sorting first
        m_WorkList.Items.SortDescriptions.Clear();

        // Sort by the Content property
        m_WorkList.Items.SortDescriptions.Add(
            new SortDescription(nameof(ElmishApp.WorkModel.IBindings.LastEventCreatedAt), ListSortDirection.Descending));
    }
}
