namespace PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.Types
open System.Collections.Specialized
open System.Collections.Generic
open System.Diagnostics.CodeAnalysis

type IPatternSettings =
    interface
        abstract Patterns : Pattern list with get, set
    end

type ITimePointPrototypesSettings =
    interface
        abstract TimePointPrototypesSettings : string option with get, set
    end

type ITimePointSettings =
    interface
        abstract TimePointSettings : string option with get, set
    end

type IDisableSkipBreakSettings =
    interface
        abstract DisableSkipBreak : bool with get, set
    end

type ICurrentWorkItemSettings =
    interface
        abstract CurrentWork : Work option with get, set
    end

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
    [<MaybeNull>]
    abstract CurrentVersion : string | null with get, set
    abstract RecentDbFileList : ICollection<string> with get
    abstract AddDatabaseFileToRecent : dbFilePath: string -> unit
