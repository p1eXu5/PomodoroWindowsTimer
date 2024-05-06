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

namespace PomodoroWindowsTimer.WpfClient.UserControls.Shared;
/// <summary>
/// Interaction logic for PwtDatePicker.xaml
/// </summary>
public partial class PwtDatePicker : UserControl
{
    public PwtDatePicker()
    {
        InitializeComponent();
    }



    public ICommand OkCommand
    {
        get { return (ICommand)GetValue(OkCommandProperty); }
        set { SetValue(OkCommandProperty, value); }
    }

    // Using a DependencyProperty as the backing store for OkCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OkCommandProperty =
        DependencyProperty.Register("OkCommand", typeof(ICommand), typeof(PwtDatePicker), new PropertyMetadata(null));


}
