module CycleBell.ElmishApp.MainModel.Program

open Elmish
open CycleBell.ElmishApp.Models
open CycleBell.ElmishApp.Models.MainModel
open CycleBell.Looper
open CycleBell.ElmishApp
open CycleBell.Types


let update (looper: Looper) (msg: Msg) (model: MainModel) =
    match msg with
    | Msg.Play ->
        looper.Resume()
        { model with LooperState = Playing }, Cmd.none

    | Msg.Stop ->
        looper.Stop()
        { model with LooperState = Stopped }, Cmd.none

    | Msg.LooperMsg evt ->
        let activeTimePoint =
            match evt with
            | LooperEvent.TimePointTimeReduced tp
            | LooperEvent.TimePointStarted (tp, _) -> tp |> Some
            | _ -> model.ActiveTimePoint

        let cmd =
            match activeTimePoint with
            | Some tp when tp.Kind = Kind.Break ->
                Cmd.OfAsync.attempt Infrastructure.minimize () Msg.OnError
            | _ ->
                Cmd.none


        { model with ActiveTimePoint = activeTimePoint }, cmd

    | Minimize ->
        model, Cmd.OfAsync.attempt Infrastructure.minimize () Msg.OnError

    | Msg.OnError ex ->
        model.ErrorQueue.EnqueuError(ex.Message)
        model, Cmd.none
