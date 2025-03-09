namespace PomodoroWindowsTimer.Wpf.Models;

public readonly record struct LinearDiagramItemValue(TimeOnly StartTime, TimeSpan Duration, bool? IsWork);
