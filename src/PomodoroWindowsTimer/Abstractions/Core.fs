namespace PomodoroWindowsTimer.Abstractions

open System
open PomodoroWindowsTimer.Types

type ITimePointQueue =
    inherit IDisposable
    [<CLIEvent>]
    abstract TimePointsChanged : IEvent<TimePoint list * TimePointId option>
    [<CLIEvent>]
    abstract TimePointsLoopCompletted : IEvent<unit>
    abstract AddMany : TimePoint seq -> unit
    abstract Start : unit -> unit
    abstract TryGetNext : unit -> TimePoint option
    abstract Reload : TimePoint list -> unit
    abstract TryPick : unit -> TimePoint option
    /// Reorders queue that interested time point priority becomes lower. Returns True if time point exists.
    abstract ScrollTo : TimePointId -> bool
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
        abstract AddSubscriber : (LooperEvent -> unit) -> unit

        /// <summary>
        /// Tries to pick TimePoint from queue, if it present
        /// emits TimePointStarted event and sets ActiveTimePoint.
        /// </summary>
        abstract PreloadTimePoint : unit -> unit

        abstract GetActiveTimePoint : unit -> ActiveTimePoint option
        abstract GetActiveTimePointAndPlayingState : unit -> ActiveTimePointAndPlayingState option
        abstract GetActiveAndPlayedTimePoints : unit -> ActiveAndPlayedTimePoints option
    end


