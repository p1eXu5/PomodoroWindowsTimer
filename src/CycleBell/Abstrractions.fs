namespace CycleBell.Abstrractions

open FSharp.Control
open System
open CycleBell.Types

type ITimePointQueue =
    inherit IDisposable
    abstract Start : unit -> unit
    abstract Enqueue : Async<TimePoint option>
