module CycleBell.ElmishApp.MainModel.Program

open Elmish
open CycleBell.ElmishApp.Models
open CycleBell.ElmishApp.Models.MainModel
open CycleBell.Looper
open CycleBell.ElmishApp
open CycleBell.Types


let update (looper: Looper) (msg: Msg) (model: MainModel) =
    match msg with
    | Msg.PickFirstTimePoint ->
        let atp = looper.PickFirst()
        { model with ActiveTimePoint = atp }, Cmd.none
    | Msg.Play ->
        looper.Resume()
        { model with LooperState = Playing }, Cmd.none

    | Msg.Stop ->
        looper.Stop()
        { model with LooperState = Stopped }, Cmd.none

    | Msg.LooperMsg evt ->
        let (activeTimePoint, cmd) =
            match evt with
            | LooperEvent.TimePointTimeReduced tp -> (tp |> Some, Cmd.none)
            | LooperEvent.TimePointStarted (tp, _) ->
                match tp.Kind with
#if DEBUG
                | Break -> (tp |> Some, Cmd.none)
                | Work -> (tp |> Some, Cmd.none)
#else
                | Break -> (tp |> Some, Cmd.OfAsync.attempt Infrastructure.minimize () Msg.OnError)
                | Work -> (tp |> Some, Cmd.OfAsync.attempt Infrastructure.restore () Msg.OnError)
#endif
            | _ -> (model.ActiveTimePoint, Cmd.none)

        { model with ActiveTimePoint = activeTimePoint }, cmd

    | Minimize ->
        model, Cmd.OfAsync.attempt Infrastructure.minimize () Msg.OnError

    | Msg.OnError ex ->
        model.ErrorQueue.EnqueuError(ex.Message)
        model, Cmd.none
