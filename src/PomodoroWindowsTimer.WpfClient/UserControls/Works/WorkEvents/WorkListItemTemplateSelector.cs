using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PomodoroWindowsTimer.ElmishApp.Models;

using System.Windows.Controls;
using System.Windows;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works.WorkEvents;

internal sealed class WorkListItemTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object model, DependencyObject container)
    {
        if (container is FrameworkElement element && model is not null)
        {
            WorkEventModelModule.WorkEventModelId workEventModelId = ((dynamic)model).WorkEventModelId;

            if (workEventModelId.IsWorkId)
            {
                return element.FindResource("dt_WorkEventRow") as DataTemplate;
            }

            if (workEventModelId.IsBreakId)
            {
                return element.FindResource("dt_BreakEventRow") as DataTemplate;
            }
        }
        return null;
    }
}
