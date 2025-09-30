namespace PomodoroWindowsTimer.Types

open System

type ActiveAndPlayedTimePoints =
    {
        ActiveTimePoint: ActiveTimePoint
        PlayedTimePoints: Set<TimePointId>
        IsPlaying: bool
    }

type ActiveTimePointAndPlayingState =
    {
        ActiveTimePoint: ActiveTimePoint
        IsPlaying: bool
    }

type LooperEvent =
    | TimePointReady of ActiveTimePoint * sentTime: DateTimeOffset
    | TimePointStarted of TimePointStartedEventArgs * sentTime: DateTimeOffset
    | TimePointTimeReduced of TimePointTimeReducedEventArgs * sentTime: DateTimeOffset
    | TimePointStopped of ActiveTimePoint * sentTime: DateTimeOffset
and
    TimePointStartedEventArgs =
        {
            NewActiveTimePoint: ActiveTimePoint
            OldActiveTimePoint: ActiveTimePoint option
            SwitchingMode: TimePointSwitchingMode
        }
and
    TimePointTimeReducedEventArgs =
        {
            ActiveTimePoint: ActiveTimePoint
            IsPlaying: bool
        }
and
    [<Struct>]
    TimePointSwitchingMode =
        | Auto
        | Manual


// ------------------------------- modules

module TimePointStartedEventArgs =

    let init newActiveTimePoint oldActiveTimePoint switchingMode : TimePointStartedEventArgs =
        {
            NewActiveTimePoint = newActiveTimePoint
            OldActiveTimePoint = oldActiveTimePoint
            SwitchingMode = switchingMode
        }

    /// Sets OldActiveTimePoint equal to NewActiveTimePoint
    let ofActiveTimePoint activeTimePoint switchingMode : TimePointStartedEventArgs =
        {
            NewActiveTimePoint = activeTimePoint
            OldActiveTimePoint = activeTimePoint |> Some
            SwitchingMode = switchingMode
        }

module TimePointTimeReducedEventArgs =

    let init activeTimePoint isPlaying : TimePointTimeReducedEventArgs =
        {
            ActiveTimePoint = activeTimePoint
            IsPlaying = isPlaying
        }
