﻿namespace PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.Types
open System.Collections.Specialized
open System.Collections.Generic

type IUserSettings =
    inherit IBotSettings
    inherit IPatternSettings
    inherit ITimePointPrototypesSettings
    inherit ITimePointSettings
    inherit IDisableSkipBreakSettings
    inherit ICurrentWorkItemSettings
    inherit IDatabaseSettings
    abstract LastStatisticPeriod : DateOnlyPeriod option with get, set
    // TODO: abstract RollbackWorkStrategy: RollbackWorkStrategy with get, set
    abstract LastDayCount : int with get, set
    abstract CurrentVersion : string with get, set
    abstract RecentDbFileList : ICollection<string> with get
    abstract AddDatabaseFileToRecent : dbFilePath: string -> unit
