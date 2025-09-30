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
        TimePoints: TimePointModel list
        ActiveTimePointId: TimePointId option
        DisableSkipBreak: bool
        DisableMinimizeMaximizeWindows: bool
    }


module RunningTimePointListModel =

    type Msg =
        | TimePointModelMsg of TimePointId * TimePointModel.Msg
        | SetActiveTimePointId of TimePointId option
        | LooperMsg of LooperEvent
        | SetDisableSkipBreak of bool
        | SetDisableMinimizeMaximizeWindows of bool
        | PlayerUserSettingsChanged
        | TimePointsChangedQueueMsg of TimePoint list * TimePointId option
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

    let withTimePointQueuePlayedTimePoints timePoints (activeAndPlayedTimePoints: ActiveAndPlayedTimePoints) (model: RunningTimePointListModel) =
        let activeTpId = activeAndPlayedTimePoints.ActiveTimePoint.OriginalId
        { model with
            ActiveTimePointId = activeTpId |> Some
            TimePoints =
                (timePoints, (false, []))
                ||> List.foldBack (fun tp (isSelected, res) ->
                    let tpModel = TimePointModel.init tp
                    if not isSelected && tp.Id = activeTpId then
                        (true, { tpModel with IsSelected = true; IsPlaying = activeAndPlayedTimePoints.IsPlaying } :: res)
                    elif activeAndPlayedTimePoints.PlayedTimePoints |> Set.contains tp.Id then
                        (isSelected, { tpModel with IsPlayed = true } :: res)
                    else
                        (isSelected, tpModel :: res)
                )
                |> snd
       
        }

    let withTimePointQueueTimePoints timePoints timePointIdOpt (model: RunningTimePointListModel) =
        { model with
            ActiveTimePointId = timePointIdOpt
            TimePoints =
                match timePointIdOpt with
                | Some activeTpId ->
                    (timePoints, (false, []))
                    ||> List.foldBack (fun tp (isSelected, res) ->
                        let tpModel = TimePointModel.init tp
                        if not isSelected && tp.Id = activeTpId then
                            (true, { tpModel with IsSelected = true } :: res)
                        else
                            (isSelected, tpModel :: res)
                    )
                    |> snd
                | None -> []
        }


    let init (timePointQueue: ITimePointQueue) (looper: ILooper) (playerUserSettings: IPlayerUserSettings) =
        let (timePoints, _) = timePointQueue.GetTimePoints()
        let activeAndPlayedTimePointsOpt = looper.GetActiveAndPlayedTimePoints()
        let model =
            {
                TimePoints = []
                ActiveTimePointId = None
                DisableSkipBreak = playerUserSettings.DisableSkipBreak
                DisableMinimizeMaximizeWindows = playerUserSettings.DisableMinimizeMaximizeWindows
            }
        match activeAndPlayedTimePointsOpt with
        | Some activeAndPlayedTimePoints ->
            model |> withTimePointQueuePlayedTimePoints timePoints activeAndPlayedTimePoints
        | None -> model

    let withActiveTimePointId timePointIdOpt (model: RunningTimePointListModel) =
        { model with
            ActiveTimePointId = timePointIdOpt
            TimePoints =
                match timePointIdOpt with
                | Some activeTpId ->
                    let oldActiveTpId = model.ActiveTimePointId

                    (model.TimePoints, (false, oldActiveTpId.IsNone, []))
                    ||> List.foldBack (fun tpModel (isSelected, isUnselected, res) ->
                        if not isSelected && tpModel.Id = activeTpId then
                            (true, isUnselected, { tpModel with IsSelected = true; IsPlayed = false } :: res)
                        elif not isUnselected && tpModel.IsSelected && tpModel.Id = oldActiveTpId.Value then
                            (isSelected, true, { tpModel with IsSelected = false; IsPlayed = true } :: res)
                        else
                            (isSelected, isUnselected, tpModel :: res)
                    )
                    |> fun (_, _, tpModels) -> tpModels
                | None -> []
        }

    let withDisableSkipBreak v (model: RunningTimePointListModel) =
        { model with DisableSkipBreak = v }

    let withDisableMinimizeMaximizeWindows v (model: RunningTimePointListModel) =
        { model with DisableMinimizeMaximizeWindows = v }

    let withNotPlayedTimePoints (model: RunningTimePointListModel) =
        { model with
            TimePoints =
                model.TimePoints
                |> List.map (fun tp ->
                    if tp.IsPlayed then
                        { tp with IsPlayed = false }
                    else
                        tp
                )
        }
