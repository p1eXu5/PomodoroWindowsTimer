using System.Windows;
using System.Windows.Controls;
using PomodoroWindowsTimer.ElmishApp.Models;

namespace PomodoroWindowsTimer.WpfClient.Selectors;

public sealed class MainDialogContentTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object model, DependencyObject container)
    {
        if (container is FrameworkElement element && model is AppDialogModelModule.AppDialogId appDialogId)
        {
            if (appDialogId.IsBotSettingsDialogId )
            {
                return element.FindResource("dt_BotSettingsDialog") as DataTemplate;
            }

            if (appDialogId.IsRollbackWorkDialogId)
            {
                return element.FindResource("dt_RollbackWorkDialog") as DataTemplate;
            }

            if (appDialogId.IsSkipOrApplyMissingTimeDialogId)
            {
                return element.FindResource("dt_SkipOrApplyMissingTimeDialog") as DataTemplate;
            }

            if (appDialogId.IsRollbackWorkListDialogId)
            {
                return element.FindResource("dt_RollbackWorkListDialog") as DataTemplate;
            }
        }
        return null;
    }
}
