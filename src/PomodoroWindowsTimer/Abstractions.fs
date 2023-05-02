namespace PomodoroWindowsTimer.Abstractions

open FSharp.Control
open System
open PomodoroWindowsTimer.Types

type ITimePointQueue =
    inherit IDisposable
    abstract Start : unit -> unit
    abstract Enqueue : Async<TimePoint option>
    abstract Pick : TimePoint option
    abstract Scroll : Guid -> Async<unit>
