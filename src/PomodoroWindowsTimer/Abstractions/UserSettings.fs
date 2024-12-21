namespace PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.Types

type IUserSettings =
    inherit IBotSettings
    inherit IPatternSettings
    inherit ITimePointPrototypesSettings
    inherit ITimePointSettings
    inherit IDisableSkipBreakSettings
    inherit ICurrentWorkItemSettings
    inherit IDatabaseSettings
    abstract LastStatisticPeriod: DateOnlyPeriod option with get, set
    // TODO: abstract RollbackWorkStrategy: RollbackWorkStrategy with get, set
    abstract LastDayCount: int with get, set
    abstract CurrentVersion: string with get, set
