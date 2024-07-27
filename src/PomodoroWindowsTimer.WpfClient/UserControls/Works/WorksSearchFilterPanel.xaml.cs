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

        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent SearchTextChangedEvent = EventManager.RegisterRoutedEvent(
            name: "SearchTextChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(TextChangedEventHandler),
            ownerType: typeof(WorksSearchFilterPanel));

        // Provide CLR accessors for assigning an event handler.
        public event TextChangedEventHandler SearchTextChanged
        {
            add { AddHandler(SearchTextChangedEvent, value); }
            remove { RemoveHandler(SearchTextChangedEvent, value); }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Create a RoutedEventArgs instance.
            TextChangedEventArgs routedEventArgs = new(SearchTextChangedEvent, e.UndoAction, e.Changes);

            // Raise the event, which will bubble up through the element tree.
            RaiseEvent(routedEventArgs);
        }

        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent CountChangedEvent = EventManager.RegisterRoutedEvent(
            name: "CountChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(TextChangedEventHandler),
            ownerType: typeof(WorksSearchFilterPanel));

        // Provide CLR accessors for assigning an event handler.
        public event TextChangedEventHandler CountChanged
        {
            add { AddHandler(CountChangedEvent, value); }
            remove { RemoveHandler(CountChangedEvent, value); }
        }

        private void CountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Create a RoutedEventArgs instance.
            TextChangedEventArgs routedEventArgs = new(CountChangedEvent, e.UndoAction, e.Changes);

            // Raise the event, which will bubble up through the element tree.
            RaiseEvent(routedEventArgs);
        }
    }
}
