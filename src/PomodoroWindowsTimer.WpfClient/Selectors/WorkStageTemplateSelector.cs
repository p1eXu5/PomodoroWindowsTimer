using System;
using System.Windows;
using System.Windows.Controls;

namespace PomodoroWindowsTimer.WpfClient.Selectors;

public sealed class WorkStageTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object model, DependencyObject container)
    {
        if (container is FrameworkElement element && model != null)
        {
            WorkSelectorModelModule.Stage modelName = ((dynamic)model).Stage; // List, Create, Update

            if (modelName.Equals("LockUnlockBreakModelSettings", StringComparison.Ordinal))
                return element.FindResource("gp_Red") as DataTemplate;

            return element.FindResource("dt_WorkList") as DataTemplate;
        }

        return null;
    }
}
