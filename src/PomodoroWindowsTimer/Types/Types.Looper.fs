namespace PomodoroWindowsTimer.Types

type LooperEvent =
    | TimePointStarted of TimePointStartedEventArgs
    | TimePointTimeReduced of ActiveTimePoint
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