using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works.WorkEvents;
/// <summary>
/// Interaction logic for WorkEventList.xaml
/// </summary>
public partial class WorkEventList : UserControl
{
    public WorkEventList()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        var parentFrameworkElement = FindParentFrameworkElement(this);
        if (parentFrameworkElement != null)
        {
            this.DataContext = ((dynamic)parentFrameworkElement.DataContext).WorkEventListDialog;
        }
    }

    private static FrameworkElement? FindParentFrameworkElement(DependencyObject child)
    {
        // Get the parent item
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);

        // We've reached the end of the tree
        if (parentObject == null) return null;


        if (parentObject is FrameworkElement elem)
        {
            IEnumerable<string> members = ((dynamic)elem.DataContext).GetDynamicMemberNames();
            if (members.Any(s => s.Equals("WorkEventListDialog", StringComparison.Ordinal)))
            {
                return elem;
            }
            return FindParentFrameworkElement(parentObject);
        }
        else
        {
            // Use recursion to proceed with the next level
            return FindParentFrameworkElement(parentObject);
        }
    }
}
