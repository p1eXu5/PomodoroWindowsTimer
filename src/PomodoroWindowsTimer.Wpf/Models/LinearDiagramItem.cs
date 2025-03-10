namespace PomodoroWindowsTimer.Wpf.Models;

public readonly record struct LinearDiagramItem(int Id, string Name, TimeOnly StartTime, TimeSpan Duration, bool? IsWork);
