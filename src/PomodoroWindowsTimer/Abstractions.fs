namespace PomodoroWindowsTimer.Abstractions

open FSharp.Control
open System
open PomodoroWindowsTimer.Types

type ITimePointQueue =
    inherit IDisposable
    abstract Start : unit -> unit
    abstract TryEnqueue : TimePoint option with get
    abstract Reload : TimePoint list -> unit
    abstract TryPick : TimePoint option
    abstract Scroll : Guid -> unit

type ILooper =
    inherit IDisposable
    abstract Start : unit -> unit
    abstract Shift : float<sec> -> unit
    abstract Resume : unit -> unit
    abstract AddSubscriber : (LooperEvent -> Async<unit>) -> unit
    abstract PreloadTimePoint : unit -> unit

