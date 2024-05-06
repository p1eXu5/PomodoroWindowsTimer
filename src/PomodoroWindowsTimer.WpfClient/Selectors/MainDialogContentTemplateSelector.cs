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
            AppDialogModelModule.AppDialogId? appDialogId = ((dynamic)model).AppDialogId;

            if (appDialogId.HasValue && appDialogId.Value.IsBotSettingsDialogId )
            {
                return element.FindResource("dt_BotSettingsDialog") as DataTemplate;
            }

            if (appDialogId.HasValue && appDialogId.Value.IsWorkStatisticsDialogId)
            {
                return element.FindResource("dt_BotSettingsDialog") as DataTemplate;
            }
        }
        return null;
    }
}
