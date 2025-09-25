namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure

type RunningTimePointListModel =
    {
        TimePoints: TimePoint list
        ActiveTimePointId: TimePointId option
        DisableSkipBreak: bool
        DisableMinimizeMaximizeWindows: bool
    }


module RunningTimePointListModel =

    type Msg =
        | TimePointModelMsg of TimePointModel.Msg // Preserved message
        | SetActiveTimePointId of TimePointId option
        | LooperMsg of LooperEvent
        | SetDisableSkipBreak of bool
        | SetDisableMinimizeMaximizeWindows of bool
        | PlayerUserSettingsChanged
        | TimePointQueueMsg of TimePoint list * TimePointId option
        | TimePointsLoopComplettedQueueMsg
        | RequestTimePointGenerator
        | OnExn of exn

    // ---------------------------------------------------

    [<RequireQualifiedAccess>]
    [<Struct>]
    type Intent =
        | None
        | RequestTimePointGenerator

    // ---------------------------------------------------

    let init (timePointQueue: ITimePointQueue) (playerUserSettings: IPlayerUserSettings) =
        let (timePoints, timePointId) = timePointQueue.GetTimePoints()
        {
            TimePoints = timePoints
            ActiveTimePointId = timePointId
            DisableSkipBreak = playerUserSettings.DisableSkipBreak
            DisableMinimizeMaximizeWindows = playerUserSettings.DisableMinimizeMaximizeWindows
        }

    let withActiveTimePointId timePointId (model: RunningTimePointListModel) =
        { model with ActiveTimePointId = timePointId }

    let withDisableSkipBreak v (model: RunningTimePointListModel) =
        { model with DisableSkipBreak = v }

    let withDisableMinimizeMaximizeWindows v (model: RunningTimePointListModel) =
        { model with DisableMinimizeMaximizeWindows = v }

    let withTimePoints timePoints timePointIdOpt (model: RunningTimePointListModel) =
        if timePoints = model.TimePoints then
            { model with ActiveTimePointId = timePointIdOpt }
        else
            { model with TimePoints = timePoints; ActiveTimePointId = timePointIdOpt }
