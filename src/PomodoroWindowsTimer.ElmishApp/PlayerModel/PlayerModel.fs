﻿namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.Abstractions

// TODO: move to Player entity
type PlayerModel =
    {
        ActiveTimePoint: ActiveTimePoint option

        LooperState: LooperState
        LastAtpWhenPlayOrNextIsManuallyPressed: TimePointId option

        ShiftAndPreShiftTimes: ShiftAndPreShiftTimes option

        DisableSkipBreak: bool
        DisableMinimizeMaximizeWindows: bool

        RetrieveWorkSpentTimesState: AsyncDeferredState
    }
and
    // TODO: rename to Player State
    /// Player state.
    LooperState =
        | Initialized
        | Playing
        | Stopped
        /// To restore LooperState when shifting end and previous state was not Playing.
        | TimeShifting of previousState: LooperState
and
    WindowsState =
        | Maximized
        | Minimized
and
    [<Struct>]
    ShiftAndPreShiftTimes =
        {
            PreShiftActiveRemainingSeconds: float<sec>
            NewActiveRemainingSeconds: float<sec>
        }

module PlayerModel =

    [<RequireQualifiedAccess>]
    type Msg =
        | LooperMsg of LooperEvent
        | SetDisableSkipBreak of bool
        | SetDisableMinimizeMaximizeWindows of bool
        | StartTimePoint of Operation<Guid, unit>
        | Play
        | Next
        | Replay
        | Stop
        | Resume
        | PreChangeActiveTimeSpan
        | ChangeActiveTimeSpan of float
        | PostChangeActiveTimeSpan of AsyncOperation<unit, Result<WorkSpentTime list, string>>
        | OnError of string
        | OnExn of exn

    module Msg = 
        let tryNext (model: PlayerModel) =
            model.ActiveTimePoint
            |> Option.bind (fun atp ->
                match atp.Kind with
                | Kind.Break
                | Kind.LongBreak when model.DisableSkipBreak -> None
                | _ ->
                    Msg.Next |> Some
            )

        let playStopResume (model: PlayerModel) =
            match model.LooperState with
            | TimeShifting _
            | Initialized -> Msg.Play
            | Playing -> Msg.Stop
            | Stopped -> Msg.Resume


    module MsgWith =
        let (|``Start of PostChangeActiveTimeSpan``|_|) (model: PlayerModel) (msg: Msg) =
            match msg, model.ActiveTimePoint, model.ShiftAndPreShiftTimes with
            | Msg.PostChangeActiveTimeSpan (AsyncOperation.Start _), Some atp, Some shiftTimes ->
                let (deff, cts) = model.RetrieveWorkSpentTimesState |> AsyncDeferredState.forceInProgressWithCancellation
                (
                    deff
                    , cts
                    , atp
                    , shiftTimes
                )
                |> Some
            | _ -> None

        let (|``Finish of PostChangeActiveTimeSpan``|_|) (model: PlayerModel) (msg: Msg) =
            match msg, model.ActiveTimePoint with
            | Msg.PostChangeActiveTimeSpan (AsyncOperation.Finish (res, cts)), Some atp ->
                model.RetrieveWorkSpentTimesState
                |> AsyncDeferredState.chooseRetrievedResultWithin res cts
                |> Option.map (Result.map (fun (deff, list) -> (deff, list, atp)))
            | _ -> None


    [<RequireQualifiedAccess>]
    type Intent =
        | None
        /// When slider has been rolled forward.
        | SkipOrApplyMissingTime of (*Work * *)atpKind: Kind * atpId: TimePointId * diff: TimeSpan * time: DateTimeOffset
        /// When slider has been rolled backward and only one work is counted.
        | RollbackTime of WorkSpentTime * atpKind: Kind * atpId: TimePointId * time: DateTimeOffset
        /// When slider has been rolled backward and multiple worka are counted.
        | MultipleRollbackTime of WorkSpentTime list * atpKind: Kind * atpId: TimePointId * time: DateTimeOffset

    // ---------------------------------------------------

    let init (usrSettings: IUserSettings) =
        {
            ActiveTimePoint = None
            
            LooperState = Initialized
            LastAtpWhenPlayOrNextIsManuallyPressed = None

            ShiftAndPreShiftTimes = None

            DisableSkipBreak = usrSettings.DisableSkipBreak
            DisableMinimizeMaximizeWindows = false

            RetrieveWorkSpentTimesState = AsyncDeferredState.NotRequested
        }

    // ---------------------------------------------------

    let withLooperState looperState (model: PlayerModel) =
        { model with LooperState = looperState }

    let withNotRequestedRetrieveWorkSpentTimesState (model: PlayerModel) =
        { model with
            RetrieveWorkSpentTimesState =
                model.RetrieveWorkSpentTimesState |> AsyncDeferredState.forceNotRequestedWithCancellation
        }

    let timePointKindEnum (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map (fun tp ->
            match tp.Kind with
            | Work -> TimePointKind.Work
            | Break -> TimePointKind.Break
            | LongBreak -> TimePointKind.Break
        )
        |> Option.defaultValue TimePointKind.Undefined

    let withActiveTimePoint atp (model: PlayerModel) =
       { model with ActiveTimePoint = atp; }

    let zipTimePointKindEnum (model: PlayerModel) =
        (model, model |> timePointKindEnum)

    let withNoneLastAtpWhenPlayOrNextIsManuallyPressed (model: PlayerModel) =
        { model with LastAtpWhenPlayOrNextIsManuallyPressed = None }

    let withLastAtpWhenPlayOrNextIsManuallyPressed (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map (fun atp ->
            { model with LastAtpWhenPlayOrNextIsManuallyPressed = atp.Id |> Some }
        )
        |> Option.defaultValue model

    let isLastAtpWhenPlayOrNextIsManuallyPressed (atpOpt: ActiveTimePoint option) (model: PlayerModel) =
        match model.LastAtpWhenPlayOrNextIsManuallyPressed, atpOpt with
        | Some atpId, Some atp -> atpId = atp.Id
        | _ -> false

    let isLooperStateIsNotInitialized (model: PlayerModel) =
        match model.LooperState with
        | TimeShifting s ->
            match s with
            | Initialized -> false
            | _ -> true
        | Initialized -> false
        | _ -> true

    let isPlaying (model: PlayerModel) =
        match model.LooperState with
        | Playing -> true
        | _ -> false

    let activeTimePointIdValue (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.get
        |> _.Id

    let getRemainingTimeSpan (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map _.RemainingTimeSpan
        |> Option.defaultValue TimeSpan.Zero

    let getActiveSpentTime (model: PlayerModel) =
        match model.ActiveTimePoint with
        | Some atp -> (atp.TimeSpan - atp.RemainingTimeSpan).TotalSeconds
        | _ -> 0.0

    /// Returns model OriginTimePoint.TimeSpan.TotalSeconds.
    let getActiveTimeDuration (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map (fun tp -> tp.TimeSpan.TotalSeconds)
        |> Option.defaultValue 0.0

    let getActiveTimeKind (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map _.Kind
        |> Option.defaultValue Kind.Work

    let withStoppedLooper (looper: ILooper) (model: PlayerModel) =
        match model.LooperState with
        | LooperState.Playing ->
            looper.Stop()
            model |> withLooperState Stopped
        | _ -> model

    let withShiftedActiveTimePoint (looper: ILooper) (model: PlayerModel) =
        match model.ActiveTimePoint with
        | Some activeTp ->
            looper.Shift(activeTp.TimeSpan.TotalSeconds * 1.0<sec>)
            model
        | _ ->
            model


    let withNewActiveRemainingSeconds seconds (model: PlayerModel) =
        { model with
            ShiftAndPreShiftTimes = model.ShiftAndPreShiftTimes |> Option.map (fun sm -> { sm with NewActiveRemainingSeconds = seconds })
        }

    let withoutShiftAndPreShiftTimes (model: PlayerModel) =
        { model with ShiftAndPreShiftTimes = None }

    /// Sets model.ShiftAndPreShiftTimes and model.LooperState to TimeShifting.
    let withPreShiftState (model: PlayerModel) =
        let atp = model.ActiveTimePoint.Value
        { model with
            ShiftAndPreShiftTimes =
                {
                    PreShiftActiveRemainingSeconds = atp.RemainingTimeSpan.TotalSeconds * 1.0<sec>
                    NewActiveRemainingSeconds = atp.RemainingTimeSpan.TotalSeconds * 1.0<sec>
                }
                |> Some
            LooperState = TimeShifting model.LooperState
        }

    let withDisableSkipBreak v (model: PlayerModel) =
        { model with DisableSkipBreak = v }

    let withDisableMinimizeMaximizeWindows v (model: PlayerModel) =
        { model with DisableMinimizeMaximizeWindows = v }

    let withRetreiveWorkSpentTimesState deff (model: PlayerModel) =
        { model with RetrieveWorkSpentTimesState = deff }

    let canShiftActiveTime (model: PlayerModel) =
        match model.ActiveTimePoint, model.RetrieveWorkSpentTimesState with
        | Some _, AsyncDeferredState.NotRequested
        | Some _, AsyncDeferredState.Retrieved -> true
        | _ -> false