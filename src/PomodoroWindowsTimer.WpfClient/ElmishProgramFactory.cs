﻿using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient;

internal sealed class ElmishProgramFactory(
    IThemeSwitcher themeSwitcher,
    IUserSettings userSettings,
    [FromKeyedServices("main")] IErrorMessageQueue mainErrorMessageQueue,
    ILoggerFactory loggerFactory
)
{
    public IUserSettings UserSettings => userSettings;
    public IThemeSwitcher ThemeSwitcher => themeSwitcher;
    public IErrorMessageQueue MainErrorMessageQueue => mainErrorMessageQueue;
    public ILoggerFactory LoggerFactory => loggerFactory;

    public void RunElmishProgram(Window mainWindow)
    =>
        ElmishApp.Program.main(
            mainWindow,
            ThemeSwitcher,
            UserSettings,
            MainErrorMessageQueue,
            LoggerFactory);
}
