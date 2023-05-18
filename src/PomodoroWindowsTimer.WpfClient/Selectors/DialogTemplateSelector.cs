using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.WpfClient.Selectors
{
    public sealed class DialogTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object model, DependencyObject container)
        {
            FrameworkElement? element = container as FrameworkElement;

            if (element != null && model != null)
            {
                string? modelName = ((dynamic)model).ModelName;
                
                if (modelName is null) {
                    return null;
                }

                if (modelName.Equals("LockUnlockBreakModelSettings", StringComparison.Ordinal))
                    return element.FindResource("gp_Red") as DataTemplate;

                return element.FindResource("gp_Green") as DataTemplate;
            }
            return null;
        }
    }
}
