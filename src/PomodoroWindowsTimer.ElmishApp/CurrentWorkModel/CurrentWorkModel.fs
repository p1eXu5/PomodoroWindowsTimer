namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open PomodoroWindowsTimer
open Elmish.Extensions
open PomodoroWindowsTimer.Types

/// Responsible for work event projection to database
type CurrentWorkModel =
    {
        Work: PomodoroWindowsTimer.Types.Work
    }
    member this.Id = this.Work.Id

module CurrentWorkModel =

    type Msg =
        | SetWork of Work
        | LooperMsg of LooperEvent


    let init work =
        {
            Work = work
        }
