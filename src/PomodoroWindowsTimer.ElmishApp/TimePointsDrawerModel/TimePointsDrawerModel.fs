namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types


[<RequireQualifiedAccess>]
type TimePointsDrawerModel =
    | None of RunningTimePointState list
    | RunningTimePoints of RunningTimePointListModel
    | TimePointsGenerator of TimePointsGeneratorModel * RunningTimePointState list
and
    [<Struct>]
    RunningTimePointState =
        {
            Id: TimePointId
            IsPlayed: bool
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
            | Msg.TimePointsGeneratorMsg subMsg, TimePointsDrawerModel.TimePointsGenerator (subModel, tpStates) ->
                (subMsg, subModel, tpStates) |> Some
            | _ -> None

    // ---------------------------------------------------

    open Elmish

    /// Inits TimePointsDrawerModel.RunningTimePoints
    let initWithRunningTimePoints initRunningTimePointListModel (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.None tpStates
        | TimePointsDrawerModel.TimePointsGenerator (_, tpStates) ->
            let tppListModel = initRunningTimePointListModel ()
            let rec restoredTimePoints (tps: TimePointModel list) states res =
                match tps, states with
                | [], [] -> res |> List.rev |> Some
                | tp::tpTail, state::stateTail when tp.Id = state.Id ->
                    restoredTimePoints tpTail stateTail ({ tp with IsPlayed = state.IsPlayed } :: res)
                | _ -> None

            match restoredTimePoints tppListModel.TimePoints tpStates [] with
            | Some l -> { tppListModel with TimePoints = l }
            | None -> tppListModel
            |> TimePointsDrawerModel.RunningTimePoints
        | TimePointsDrawerModel.RunningTimePoints _ -> model

    /// Inits TimePointsDrawerModel.TimePointsGenerator
    let initWithTimePointsGenerator initTimePointsGenerator (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.None _ ->
            let (subModel, cmd) = initTimePointsGenerator()
            (subModel, []) |> TimePointsDrawerModel.TimePointsGenerator
            , Cmd.map Msg.TimePointsGeneratorMsg cmd

        | TimePointsDrawerModel.RunningTimePoints rtpListModel ->
            let states = rtpListModel.TimePoints |> List.map (fun tp -> { Id = tp.Id; IsPlayed = tp.IsPlayed })
            let (subModel, cmd) = initTimePointsGenerator()
            (subModel, states) |> TimePointsDrawerModel.TimePointsGenerator
            , Cmd.map Msg.TimePointsGeneratorMsg cmd

        | _ -> model, Cmd.none

    /// Inits TimePointsDrawerModel.None
    let initWithNone (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.None _ -> model
        | TimePointsDrawerModel.TimePointsGenerator (_, states) -> states |> TimePointsDrawerModel.None
        | TimePointsDrawerModel.RunningTimePoints rtpListModel ->
            let states = rtpListModel.TimePoints |> List.map (fun tp -> { Id = tp.Id; IsPlayed = tp.IsPlayed })
            states |> TimePointsDrawerModel.None

    let runningTimePoints (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.RunningTimePoints subModel -> subModel |> Some
        | _ -> None

    let timePointsGenerator (model: TimePointsDrawerModel) =
        match model with
        | TimePointsDrawerModel.TimePointsGenerator (subModel, _) -> subModel |> Some
        | _ -> None

    let withRunningTimePoints subModel (_: TimePointsDrawerModel) =
        subModel |> TimePointsDrawerModel.RunningTimePoints
