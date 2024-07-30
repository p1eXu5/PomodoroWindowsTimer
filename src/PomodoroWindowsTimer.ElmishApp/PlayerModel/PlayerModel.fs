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
        ActiveTimePoint: ActiveTimePoint option

        LooperState : LooperState
        LastAtpWhenPlayOrNextIsManuallyPressed: UIInitiator option

        ShiftAndPreShiftTimes: ShiftAndPreShiftTimes option

        DisableSkipBreak: bool
        DisableMinimizeMaximizeWindows: bool

        RetrieveWorkSpentTimesState: AsyncDeferredState
    }
and
    LooperState =
        | Initialized
        | Playing
        | Stopped
        /// To restore LooperState when shifting end and previous state was not Playing.
        | TimeShifting of previousState: LooperState
and
    UIInitiator = UIInitiator of ActiveTimePoint
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
        | PostChangeActiveTimeSpan of AsyncOperation<unit, Result<WorkSpentTime list, string>>
        | OnError of string
        | OnExn of exn
    and
        LooperMsg =
            | TimePointTimeReduced of ActiveTimePoint
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
            match msg, model.ActiveTimePoint, model.ShiftAndPreShiftTimes with
            | Msg.PostChangeActiveTimeSpan (AsyncOperation.Finish (res, cts)), Some atp, Some shiftTimes ->
                model.RetrieveWorkSpentTimesState
                |> AsyncDeferredState.chooseRetrievedResultWithin res cts
                |> Option.map (Result.map (fun (deff, list) -> (deff, list, atp, shiftTimes)))
            | _ -> None

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        /// TODO: Cmd.ofMsg (Msg.AppDialogModelMsg (AppDialogModel.Msg.LoadRollbackWorkDialogModel (model.CurrentWork.Value.Id, time, diff)))
        | ShowRollbackDialog of WorkId * time: DateTimeOffset * diff: TimeSpan
        /// When slider has been rolled forward.
        | SkipOrApplyMissingTime of WorkId * diff: TimeSpan * atpKind: Kind

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
        |> Option.map (fun tp ->
            { model with LastAtpWhenPlayOrNextIsManuallyPressed = tp |> UIInitiator |> Some }
        )
        |> Option.defaultValue model

    let isLastAtpWhenPlayOrNextIsManuallyPressed (tpOpt: ActiveTimePoint option) (model: PlayerModel) =
        match model.LastAtpWhenPlayOrNextIsManuallyPressed, tpOpt with
        | Some (UIInitiator atp), Some tp -> atp.Id = tp.Id
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

    let getActiveTimeSpan (model: PlayerModel) =
        model.ActiveTimePoint
        |> Option.map (fun tp -> tp.TimeSpan)
        |> Option.defaultValue TimeSpan.Zero

    let getActiveSpentTime (model: PlayerModel) =
        match model.ActiveTimePoint with
        | Some atp -> (atp.RemainingTimeSpan).TotalSeconds
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