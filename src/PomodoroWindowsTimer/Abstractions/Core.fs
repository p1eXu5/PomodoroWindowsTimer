namespace PomodoroWindowsTimer.Abstractions

open System
open PomodoroWindowsTimer.Types


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


type ITimePointQueue =
    inherit IDisposable
    abstract AddMany : TimePoint seq -> unit
    abstract Start : unit -> unit
    abstract TryGetNext : unit -> TimePoint option
    abstract Reload : TimePoint list -> unit
    abstract TryPick : unit -> TimePoint option
    abstract ScrollTo : Guid -> unit
    abstract TryFind : TimePointId -> TimePoint option


type ILooper =
    interface
        inherit IDisposable
        abstract Start : unit -> unit
        abstract Stop : unit -> unit
        abstract Next : unit -> unit
        abstract Shift : float<sec> -> unit
        abstract ShiftAck : float<sec> -> unit
        abstract Resume : unit -> unit
        abstract AddSubscriber : (LooperEvent -> Async<unit>) -> unit
        /// Tryes to pick TimePoint from queue, if it present
        /// emits TimePointStarted event and sets ActiveTimePoint.
        abstract PreloadTimePoint : unit -> unit
        abstract GetActiveTimePoint : unit -> ActiveTimePoint option
    end


