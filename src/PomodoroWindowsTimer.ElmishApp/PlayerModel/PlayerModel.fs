namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.TimePointQueue
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.Abstractions

type PlayerModel =
    {
        OriginTimePoint: TimePoint option
        ActiveTimePoint: TimePoint option

        LooperState : LooperState
        LastAtpWhenPlayOrNextIsManuallyPressed: UIInitiator option

        // to decide where to add time when Work time point shifting down:
        PreShiftActiveTimeSpan: float<sec>
        NewActiveTimeSpan: float<sec>

        DisableSkipBreak: bool
        DisableMinimizeMaximizeWindows: bool
    }
and
    LooperState =
        | Initialized
        | Playing
        | Stopped
        /// To restore LooperState when shifting end and previous state was not Playing.
        | TimeShiftingAfterNotPlaying of previousState: LooperState
and
    UIInitiator = UIInitiator of TimePoint

module PlayerModel =

    [<RequireQualifiedAccess>]
    type Msg =
        | LooperMsg of LooperMsg
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
        | PostChangeActiveTimeSpan
        | OnExn of exn
    and
        LooperMsg =
            | TimePointTimeReduced of TimePoint
            /// Includes SetActiveTimePoint and StoreStartedWorkEventTask 
            | TimePointStarted of TimePointStartedEventArgs

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
            | TimeShiftingAfterNotPlaying _
            | Initialized -> Msg.Play
            | Playing -> Msg.Stop
            | Stopped -> Msg.Resume

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        /// TODO: Cmd.ofMsg (Msg.AppDialogModelMsg (AppDialogModel.Msg.LoadRollbackWorkDialogModel (model.CurrentWork.Value.Id, time, diff)))
        | ShowRollbackDialog of WorkId * time: DateTimeOffset * diff: TimeSpan


    let init (usrSettings: IUserSettings) =
        {
            OriginTimePoint = None
            ActiveTimePoint = None
            
            LooperState = Initialized
            LastAtpWhenPlayOrNextIsManuallyPressed = None
            
            PreShiftActiveTimeSpan = -1.0<sec>
            NewActiveTimeSpan = -1.0<sec>

            DisableSkipBreak = usrSettings.DisableSkipBreak
            DisableMinimizeMaximizeWindows = false
        }

    // ---------------------------------------------------

    let withLooperState looperState (model: PlayerModel) =
        { model with LooperState = looperState }

    let timePointKindEnum (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map (fun tp ->
            match tp.Kind with
            | Work -> TimePointKind.Work
            | Break -> TimePointKind.Break
            | LongBreak -> TimePointKind.Break
        )
        |> Option.defaultValue TimePointKind.Undefined

    let withOriginTimePoint tp (model: PlayerModel) =
       { model with OriginTimePoint = tp; }

    let withActiveTimePoint atp (model: PlayerModel) =
       { model with ActiveTimePoint = atp; }

    let zipTimePointKindEnum (model: PlayerModel) =
        (model, model |> timePointKindEnum)

    let withNoneLastAtpWhenPlayOrNextIsManuallyPressed (model: PlayerModel) =
        { model with LastAtpWhenPlayOrNextIsManuallyPressed = None }

    let withLastAtpWhenPlayOrNextIsManuallyPressed (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map (fun tp ->
            { model with LastAtpWhenPlayOrNextIsManuallyPressed = tp |> UIInitiator |> Some }
        )
        |> Option.defaultValue model

    let isLastAtpWhenPlayOrNextIsManuallyPressed (tpOpt: TimePoint option) (model: PlayerModel) =
        match model.LastAtpWhenPlayOrNextIsManuallyPressed, tpOpt with
        | Some (UIInitiator atp), Some tp -> atp.Id = tp.Id
        | _ -> false

    let isLooperStateIsNotInitialized (model: PlayerModel) =
        match model.LooperState with
        | TimeShiftingAfterNotPlaying s ->
            match s with
            | Initialized -> false
            | _ -> true
        | Initialized -> false
        | _ -> true

    let isPlaying (model: PlayerModel) =
        match model.LooperState with
        | Playing -> true
        | _ -> false


    let getActiveTimeSpan (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map (fun tp -> tp.TimeSpan)
        |> Option.defaultValue TimeSpan.Zero

    let getActiveSpentTime (model: PlayerModel) =
        match model.OriginTimePoint, model.ActiveTimePoint with
        | Some tp, Some atp -> (tp.TimeSpan - atp.TimeSpan).TotalSeconds
        | _ -> 0.0

    let getActiveTimeDuration (model: PlayerModel) =
        model.OriginTimePoint
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

    let withShiftTime seconds (model: PlayerModel) =
        { model with NewActiveTimeSpan = seconds }

    let withoutShiftAndPreShiftTimes (model: PlayerModel) =
        { model with NewActiveTimeSpan = -1.0<sec>; PreShiftActiveTimeSpan = -1.0<sec>; }

    let withPreShiftActiveTimePointTimeSpan (model: PlayerModel) =
        { model with PreShiftActiveTimeSpan = model.ActiveTimePoint.Value.TimeSpan.TotalSeconds * 1.0<sec> }

    let withDisableSkipBreak v (model: PlayerModel) =
        { model with DisableSkipBreak = v }

    let withDisableMinimizeMaximizeWindows v (model: PlayerModel) =
        { model with DisableMinimizeMaximizeWindows = v }

