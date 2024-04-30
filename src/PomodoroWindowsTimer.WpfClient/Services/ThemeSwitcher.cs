using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.ElmishApp;

namespace PomodoroWindowsTimer.WpfClient;

internal class ThemeSwitcher : IThemeSwitcher
{
    public void SwitchTheme(TimePointKind value)
    {
        var paletteHelper = new PaletteHelper(); 
        Theme theme = new PaletteHelper().GetTheme();

        switch (value)
        {
            case TimePointKind.Break:
                theme.SetSecondaryColor(Colors.Red);
                break;

            case TimePointKind.Work:
                theme.SetSecondaryColor(Colors.Lime);
                break;
        }

        paletteHelper.SetTheme(theme);
    }
}
