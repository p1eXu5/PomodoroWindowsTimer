namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types

type TimePointModel =
    {
        TimePoint: TimePoint
        IsSelected: bool
        IsPlayed: bool
        IsPlaying: bool
    }
    member this.Id = this.TimePoint.Id

module TimePointModel =

    type Msg =
        | SetName of string
        | SetTimeSpan of string
        | SetIsSelected of bool
        | SetIsSelectedNotPlaying
        | SetIsSelectedIsPlaying
        | SetIsPlayed of bool
        | SetIsPlaying of bool
        | Play
        | Stop
        | OnExn of exn

    let init timePoint =
        {
            TimePoint = timePoint
            IsSelected = false
            IsPlayed = false
            IsPlaying = false
        }



