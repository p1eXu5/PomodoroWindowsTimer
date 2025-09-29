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

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works
{
    /// <summary>
    /// Interaction logic for WorksSearchFilterPanel.xaml
    /// </summary>
    public partial class WorksSearchFilterPanel : UserControl
    {
        public WorksSearchFilterPanel()
        {
            InitializeComponent();
        }

        // Provide CLR accessors for assigning an event handler.
        public event SearchTextChangedEventHandler SearchTextChanged
        {
            add { AddHandler(SearchTextChangedEvent, value); }
            remove { RemoveHandler(SearchTextChangedEvent, value); }
        }

        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent SearchTextChangedEvent = EventManager.RegisterRoutedEvent(
            name: "SearchTextChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(SearchTextChangedEventHandler),
            ownerType: typeof(WorksSearchFilterPanel));

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // Create a RoutedEventArgs instance.
                SearchTextChangedEventArgs routedEventArgs = new(SearchTextChangedEvent, tb.Text);

                // Raise the event, which will bubble up through the element tree.
                RaiseEvent(routedEventArgs);
            }
        }

        // Provide CLR accessors for assigning an event handler.
        public event CountChangedEventHandler CountChanged
        {
            add { AddHandler(CountChangedEvent, value); }
            remove { RemoveHandler(CountChangedEvent, value); }
        }

        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent CountChangedEvent = EventManager.RegisterRoutedEvent(
            name: "CountChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(CountChangedEventHandler),
            ownerType: typeof(WorksSearchFilterPanel));

        private void CountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // Create a RoutedEventArgs instance.
                CountChangedEventArgs routedEventArgs = new(CountChangedEvent, tb.Text);

                // Raise the event, which will bubble up through the element tree.
                RaiseEvent(routedEventArgs);
            }
        }
    }
}
