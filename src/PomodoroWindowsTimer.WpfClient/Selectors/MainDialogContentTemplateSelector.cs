using System.Windows;
using System.Windows.Controls;
using PomodoroWindowsTimer.ElmishApp.Models;

namespace PomodoroWindowsTimer.WpfClient.Selectors;

public sealed class MainDialogContentTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object model, DependencyObject container)
    {
        if (container is FrameworkElement element && model != null)
        {
            AppDialogModelModule.AppDialogId appDialogId = ((dynamic)model).AppDialogId;

            if (appDialogId.IsBotSettingsDialogId)
            {
                return element.FindResource("dt_BotSettingsDialog") as DataTemplate;
            }

            if (appDialogId.IsWorkStatisticsDialogId)
            {
                return element.FindResource("dt_WorkStatisticsDialog") as DataTemplate;
            }
        }
        return null;
    }
}
