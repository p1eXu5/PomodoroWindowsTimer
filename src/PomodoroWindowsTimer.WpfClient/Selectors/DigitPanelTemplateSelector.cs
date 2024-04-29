using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.WpfClient.Selectors
{
    internal sealed class DigitPanelTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object isBreak, DependencyObject container)
        {
            FrameworkElement? element = container as FrameworkElement;

            if (element != null && isBreak != null)
            {
                if (isBreak is bool b && b)
                    return element.FindResource("gp_Red") as DataTemplate;

                return element.FindResource("gp_Green") as DataTemplate;
            }
            return null;
        }
    }
}
