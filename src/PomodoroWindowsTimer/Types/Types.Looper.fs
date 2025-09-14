namespace PomodoroWindowsTimer.Types

open System

type LooperEvent =
    | TimePointStarted of TimePointStartedEventArgs * sentTime: DateTimeOffset
    | TimePointTimeReduced of TimePointTimeReducedEventArgs * sentTime: DateTimeOffset
    | TimePointStopped of ActiveTimePoint * sentTime: DateTimeOffset
and
    TimePointStartedEventArgs =
        {
            NewActiveTimePoint: ActiveTimePoint
            OldActiveTimePoint: ActiveTimePoint option
            IsPlaying: bool
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

    let init newActiveTimePoint oldActiveTimePoint isPlaying switchingMode : TimePointStartedEventArgs =
        {
            NewActiveTimePoint = newActiveTimePoint
            OldActiveTimePoint = oldActiveTimePoint
            IsPlaying = isPlaying
            SwitchingMode = switchingMode
        }

    let ofActiveTimePoint activeTimePoint isPlaying switchingMode : TimePointStartedEventArgs =
        {
            NewActiveTimePoint = activeTimePoint
            OldActiveTimePoint = activeTimePoint |> Some
            IsPlaying = isPlaying
            SwitchingMode = switchingMode
        }

module TimePointTimeReducedEventArgs =

    let init activeTimePoint isPlaying : TimePointTimeReducedEventArgs =
        {
            ActiveTimePoint = activeTimePoint
            IsPlaying = isPlaying
        }
