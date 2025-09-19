﻿namespace PomodoroWindowsTimer.Abstractions

open System
open PomodoroWindowsTimer.Types

type ITimePointQueue =
    inherit IDisposable
    [<CLIEvent>]
    abstract TimePointsChanged : IEvent<TimePoint list>
    abstract AddMany : TimePoint seq -> unit
    abstract Start : unit -> unit
    abstract TryGetNext : unit -> TimePoint option
    abstract Reload : TimePoint list -> unit
    abstract TryPick : unit -> TimePoint option
    abstract ScrollTo : Guid -> unit
    abstract TryFind : TimePointId -> TimePoint option
    abstract GetTimePoints : unit -> (TimePoint list * TimePointId option)


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


