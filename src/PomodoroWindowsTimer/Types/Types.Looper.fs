namespace PomodoroWindowsTimer.Types

open System

type LooperEvent =
    | TimePointStarted of TimePointStartedEventArgs * sentTime: DateTimeOffset
    | TimePointStopped of ActiveTimePoint * sentTime: DateTimeOffset
    | TimePointTimeReduced of ActiveTimePoint * sentTime: DateTimeOffset
and
    TimePointStartedEventArgs =
        {
            NewActiveTimePoint: ActiveTimePoint
            OldActiveTimePoint: ActiveTimePoint option
        }


// ------------------------------- modules

module TimePointStartedEventArgs =
    let init newActiveTimePoint oldActiveTimePoint : TimePointStartedEventArgs =
        {
            NewActiveTimePoint = newActiveTimePoint
            OldActiveTimePoint = oldActiveTimePoint
        }