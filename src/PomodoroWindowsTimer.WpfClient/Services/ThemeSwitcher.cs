using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;
using PomodoroWindowsTimer.ElmishApp.Abstractions;
using PomodoroWindowsTimer.ElmishApp;
using Microsoft.Extensions.Logging;

namespace PomodoroWindowsTimer.WpfClient;

internal class ThemeSwitcher : IThemeSwitcher
{
    private readonly ILogger<ThemeSwitcher> _logger;

    public ThemeSwitcher(ILogger<ThemeSwitcher> logger)
    {
        _logger = logger;
    }

    public void SwitchTheme(TimePointKind value)
    {
        var paletteHelper = new PaletteHelper(); 
        Theme theme = new PaletteHelper().GetTheme();

        switch (value)
        {
            case TimePointKind.Break:
                _logger.LogDebug("Switching to Break Theme...");
                theme.SetSecondaryColor(Colors.Red);
                theme.SecondaryDark = new ColorPair(Color.FromRgb(0x30, 0x00,0x00), Colors.White);
                break;

            case TimePointKind.Work:
                _logger.LogDebug("Switching to Work Theme...");
                theme.SetSecondaryColor(Colors.Lime);
                theme.SecondaryDark = new ColorPair(Color.FromRgb(0x1E, 0x29, 0x00), Colors.White);
                break;
        }

        paletteHelper.SetTheme(theme);
    }
}
