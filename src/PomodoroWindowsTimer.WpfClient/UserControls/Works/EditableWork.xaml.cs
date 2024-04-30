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
/// Interaction logic for CreatingWork.xaml
/// </summary>
public partial class EditableWork : UserControl
{
    public EditableWork()
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
        DependencyProperty.Register("OkCommand", typeof(ICommand), typeof(EditableWork), new PropertyMetadata(null));



    public string OkCommandContent
    {
        get { return (string)GetValue(OkCommandContentProperty); }
        set { SetValue(OkCommandContentProperty, value); }
    }

    // Using a DependencyProperty as the backing store for OkCommandContent.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OkCommandContentProperty =
        DependencyProperty.Register("OkCommandContent", typeof(string), typeof(EditableWork), new PropertyMetadata("OK"));



    public ICommand CancelCommand
    {
        get { return (ICommand)GetValue(CancelCommandProperty); }
        set { SetValue(CancelCommandProperty, value); }
    }

    // Using a DependencyProperty as the backing store for CancelCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(EditableWork), new PropertyMetadata(null));




    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(EditableWork), new PropertyMetadata("WORK"));


}
