using System;
using System.Windows;
using System.Windows.Controls;
using PomodoroWindowsTimer.ElmishApp;
using PomodoroWindowsTimer.ElmishApp.Models;

namespace PomodoroWindowsTimer.WpfClient.Selectors;

public sealed class WorkSubModelTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object model, DependencyObject container)
    {
        if (container is FrameworkElement element && model != null)
        {
            WorkSelectorModelModule.SubModelId subModelId = ((dynamic)model).SubmodelId; // List, Create, Update

            if (subModelId.IsWorkListId)
            {
                return element.FindResource("dt_WorkList") as DataTemplate;
            }

            if (subModelId.IsCreatingWorkId)
            {
                return element.FindResource("dt_CreatingWork") as DataTemplate;
            }

            if (subModelId.IsUpdatingWorkId)
            {
                return element.FindResource("dt_UpdatingWork") as DataTemplate;
            }
        }

        return null;
    }
}
