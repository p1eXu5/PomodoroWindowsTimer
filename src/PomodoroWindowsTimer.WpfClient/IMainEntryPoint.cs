using System;
using MaterialDesignThemes.Wpf;

namespace PomodoroWindowsTimer.WpfClient;

public interface IMainEntryPoint : IDisposable
{
    SnackbarMessageQueue? MessageQueue { set; }

    void WaitBootstrap();

    void Shutdown();
    void LoadMainWindow();

    // event EventHandler OnBootstrapped;
    // void Run();
}
