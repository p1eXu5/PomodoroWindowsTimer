using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;
using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient;

internal class ThemeSwitcher : IThemeSwitcher
{
    private readonly PaletteHelper _paletteHelper = new();

    public void SwitchTheme(TimePointKind value)
    {
        Theme theme = _paletteHelper.GetTheme();

        switch (value)
        {
            case TimePointKind.Break:
                theme.SetSecondaryColor(Colors.Red);
                break;

            case TimePointKind.Work:
                theme.SetSecondaryColor(Colors.Lime);
                break;
        }

        _paletteHelper.SetTheme(theme);
    }
}
