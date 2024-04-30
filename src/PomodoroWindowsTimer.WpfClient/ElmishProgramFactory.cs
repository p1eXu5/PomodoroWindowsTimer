using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient;

internal sealed class ElmishProgramFactory(
    IWorkRepository workRepository,
    IWorkEventRepository workEventRepository,
    ITelegramBot telegramBot,
    IWindowsMinimizer windowsMinimizer,
    IThemeSwitcher themeSwitcher,
    IUserSettings userSettings,
    [FromKeyedServices("main")] IErrorMessageQueue mainErrorMessageQueue,
    [FromKeyedServices("dialog")] IErrorMessageQueue dialogErrorMessageQueue,
    ILoggerFactory loggerFactory
)
{
    internal IWorkRepository WorkRepository => workRepository;
    internal IWorkEventRepository WorkEventRepository => workEventRepository;
    internal ITelegramBot TelegramBot => telegramBot;
    internal IWindowsMinimizer WindowsMinimizer => windowsMinimizer;
    internal IThemeSwitcher ThemeSwitcher => themeSwitcher;
    internal IUserSettings UserSettings => userSettings;
    internal IErrorMessageQueue MainErrorMessageQueue => mainErrorMessageQueue;
    internal IErrorMessageQueue DialogErrorMessageQueue => dialogErrorMessageQueue;
    internal ILoggerFactory LoggerFactory => loggerFactory;



    public void RunElmishProgram(Window mainWindow)
    =>
        ElmishApp.Program.main(
            mainWindow,
            WorkRepository,
            WorkEventRepository,
            TelegramBot,
            WindowsMinimizer,
            ThemeSwitcher,
            UserSettings,
            MainErrorMessageQueue,
            DialogErrorMessageQueue,
            LoggerFactory);
}
