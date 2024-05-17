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

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works;
/// <summary>
/// Interaction logic for WorkList.xaml
/// </summary>
public partial class WorkList : UserControl
{
    public WorkList()
    {
        InitializeComponent();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.Source is TextBox tb)
        {
            e.Handled = true;
            var collView = (CollectionView)CollectionViewSource.GetDefaultView(m_WorkList.ItemsSource);

            if (!collView.CanFilter)
            {
                return;
            }

            string text = tb.Text;
            if (String.IsNullOrEmpty(text))
            {
                collView.Filter = null;
            }
            else
            {
                collView.Filter = new Predicate<object>(o => {
                    string number = ((dynamic)o).Number;
                    string title = ((dynamic)o).Title;
                    return number.Contains(text, StringComparison.OrdinalIgnoreCase) || title.Contains(text, StringComparison.OrdinalIgnoreCase);
                });
            }
        }
    }
}
