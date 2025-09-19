namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types

[<RequireQualifiedAccess>]
type TimePointsDrawerModel =
    | None
    | RunningTimePoints of RunningTimePointListModel
    | TimePointsGenerator of TimePointsGeneratorModel

module TimePointsDrawerModel =

    open PomodoroWindowsTimer.ElmishApp

    [<RequireQualifiedAccess>]
    type Msg =
        | RunningTimePointsMsg of RunningTimePointListModel.Msg
        | TimePointsGeneratorMsg of TimePointsGeneratorModel.Msg
        | LooperMsg of LooperEvent
        | InitTimePointGenerator

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
