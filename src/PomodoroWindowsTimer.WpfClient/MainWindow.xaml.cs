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

namespace PomodoroWindowsTimer.WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ICommand openFileCommand = ((dynamic)DataContext).PreChangeActiveTimeSpanCommand;
                if (openFileCommand.CanExecute(null))
                {
                    openFileCommand.Execute(null);
                }
            }
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ICommand openFileCommand = ((dynamic)DataContext).PostChangeActiveTimeSpanCommand;
            if (openFileCommand.CanExecute(null))
            {
                openFileCommand.Execute(null);
            }
        }
    }
}
