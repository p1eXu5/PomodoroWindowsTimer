using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.ElmishApp.Abstractions;

namespace PomodoroWindowsTimer.WpfClient;

internal sealed class ElmishProgramFactory(
    ILooper looper,
    ITimePointQueue timePointQueue,
    IWorkRepository workRepository,
    IWorkEventRepository workEventRepository,
    IActiveTimePointRepository activeTimePointRepository,
    ITelegramBot telegramBot,
    IWindowsMinimizer windowsMinimizer,
    IThemeSwitcher themeSwitcher,
    IUserSettings userSettings,
    [FromKeyedServices("main")] IErrorMessageQueue mainErrorMessageQueue,
    [FromKeyedServices("dialog")] IErrorMessageQueue dialogErrorMessageQueue,
    System.TimeProvider timeProvider,
    IExcelBook excelBook,
    ILoggerFactory loggerFactory
)
{

    internal ILooper Looper => looper;
    internal ITimePointQueue TimePointQueue => timePointQueue;
    internal IWorkRepository WorkRepository => workRepository;
    internal IWorkEventRepository WorkEventRepository => workEventRepository;
    internal IActiveTimePointRepository ActiveTimePointRepository => activeTimePointRepository;
    internal ITelegramBot TelegramBot => telegramBot;
    internal IWindowsMinimizer WindowsMinimizer => windowsMinimizer;
    internal IThemeSwitcher ThemeSwitcher => themeSwitcher;
    internal IUserSettings UserSettings => userSettings;
    internal IErrorMessageQueue MainErrorMessageQueue => mainErrorMessageQueue;
    internal IErrorMessageQueue DialogErrorMessageQueue => dialogErrorMessageQueue;
    internal System.TimeProvider TimeProvider => timeProvider;
    public IExcelBook ExcelBook => excelBook;
    internal ILoggerFactory LoggerFactory => loggerFactory;

    public void RunElmishProgram(Window mainWindow, Func<WorkStatisticWindow> workStatisticWindowFactory)
    =>
        ElmishApp.Program.main(
            mainWindow,
            workStatisticWindowFactory,
            Looper,
            TimePointQueue,
            WorkRepository,
            WorkEventRepository,
            ActiveTimePointRepository,
            TelegramBot,
            WindowsMinimizer,
            ThemeSwitcher,
            UserSettings,
            MainErrorMessageQueue,
            DialogErrorMessageQueue,
            TimeProvider,
            ExcelBook,
            LoggerFactory);
}
