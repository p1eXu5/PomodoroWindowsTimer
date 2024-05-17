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
                theme.SecondaryDark = new ColorPair(Color.FromRgb(0x30, 0x00,0x00), Colors.White);
                break;

            case TimePointKind.Work:
                theme.SetSecondaryColor(Colors.Lime);
                theme.SecondaryDark = new ColorPair(Color.FromRgb(0x1E, 0x29, 0x00), Colors.White);
                break;
        }

        paletteHelper.SetTheme(theme);
    }
}
