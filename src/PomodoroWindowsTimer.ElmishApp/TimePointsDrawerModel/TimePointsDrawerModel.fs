namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types


[<RequireQualifiedAccess>]
type TimePointsDrawerModel =
    | None
    | RunningTimePoints of RunningTimePointListModel
    | TimePointsGenerator of TimePointsGeneratorModel
and
    [<Struct>]
    RunningTimePointState =
        {
            Id: TimePointId
            IsPlayed: bool
        }

module RunningTimePointState =

    let ofTimePointModel (tpModel: TimePointModel) =
        {
            Id = tpModel.Id
            IsPlayed = tpModel.IsPlayed
        }


module TimePointsDrawerModel =

    open PomodoroWindowsTimer.ElmishApp

    [<RequireQualifiedAccess>]
    type Msg =
        | RunningTimePointsMsg of RunningTimePointListModel.Msg
        | TimePointsGeneratorMsg of TimePointsGeneratorModel.Msg
        | LooperMsg of LooperEvent
        | InitTimePointGenerator
        | InitRunningTimePoints


    [<RequireQualifiedAccess>]
    module MsgWith =

        let (|RunningTimePointsMsg|_|) model msg =
            match msg, model with
            | Msg.RunningTimePointsMsg subMsg, TimePointsDrawerModel.RunningTimePoints subModel ->
                (subMsg, subModel) |> Some
            | _ -> None

        let (|LooperMsg|_|) model msg =
            match msg, model with
            | Msg.LooperMsg subMsg, TimePointsDrawerModel.RunningTimePoints subModel ->
                (subMsg, subModel) |> Some
            | _ -> None

        let (|TimePointGeneratorMsg|_|) model msg =
            match msg, model with
            | Msg.TimePointsGeneratorMsg subMsg, TimePointsDrawerModel.TimePointsGenerator subModel ->
                (subMsg, subModel) |> Some
            | _ -> None

    // ---------------------------------------------------

    open Elmish

    /// <summary>
    /// Initializes TimePointsDrawerModel.RunningTimePoints
    /// </summary>
    let initWithRunningTimePoints initRunningTimePointListModel (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.None
        | TimePointsDrawerModel.TimePointsGenerator _ ->
            initRunningTimePointListModel ()
            |> TimePointsDrawerModel.RunningTimePoints
        | TimePointsDrawerModel.RunningTimePoints _ -> model

    /// <summary>
    /// Initializes TimePointsDrawerModel.TimePointsGenerator
    /// </summary>
    let initWithTimePointsGenerator initTimePointsGenerator (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.None
        | TimePointsDrawerModel.RunningTimePoints _ ->
            let (subModel, cmd) = initTimePointsGenerator()
            subModel |> TimePointsDrawerModel.TimePointsGenerator
            , Cmd.map Msg.TimePointsGeneratorMsg cmd

        | _ -> model, Cmd.none

    /// <summary>
    /// Initializes TimePointsDrawerModel.None
    /// </summary>
    let initWithNone (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.None -> model
        | TimePointsDrawerModel.RunningTimePoints _
        | TimePointsDrawerModel.TimePointsGenerator _ -> TimePointsDrawerModel.None

    let runningTimePoints (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.RunningTimePoints subModel -> subModel |> Some
        | _ -> None

    let timePointsGenerator (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.TimePointsGenerator subModel -> subModel |> Some
        | _ -> None

    let withRunningTimePoints subModel (_: TimePointsDrawerModel) =
        subModel |> TimePointsDrawerModel.RunningTimePoints
