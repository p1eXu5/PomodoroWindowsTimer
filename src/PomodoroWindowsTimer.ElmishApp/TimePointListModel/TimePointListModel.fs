namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure

type TimePointListModel =
    {
        TimePoints: TimePoint list
        ActiveTimePointId: TimePointId option
    }


module TimePointListModel =

    type Msg =
        | TimePointModelMsg of TimePointModel.Msg // Preserved message
        | SetActiveTimePointId of TimePointId option

    let init timePoint =
        {
            TimePoints = timePoint
            ActiveTimePointId = None
        }

    let withActiveTimePointId timePointId (model: TimePointListModel) =
        { model with ActiveTimePointId = timePointId }

