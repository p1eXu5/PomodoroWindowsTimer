using System;

namespace PomodoroWindowsTimer.WpfClient;

public interface IMainEntryPoint : IDisposable
{
    void WaitBootstrap();

    void Exit();

    // event EventHandler OnBootstrapped;
    // void Run();
}
